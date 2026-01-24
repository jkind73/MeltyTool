using System;
using System.Security.Cryptography;

namespace KSoft.Phoenix.Resource
{
	using PhxHash = Security.Cryptography.PhxHash;

	using FileFlagsStreamer = IO.EnumBinaryStreamer<GameFile.FileFlags, ushort>;

	public sealed class GameFile
		: IDisposable
		, IO.IEndianStreamSerializable
	{
	[Flags]
		internal enum FileFlags : ushort
		{
			COMPRESS_CONTENT = 1<<0,
			ENCRYPT_CONTENT = 1<<1,
			ENCRYPT_HEADER = 1<<2,

			K_ALL = COMPRESS_CONTENT | ENCRYPT_CONTENT | ENCRYPT_HEADER,
		};
		const ushort K_VERSION_ = 0x17;

		const int K_MAX_CONTENT_SIZE_ = 0xDD240;
		/// <summary>Number of random words that follow the content payload</summary>
		const int K_RANDOM_BLOCK_WORDS_ = 0xA00;

		FileFlags flags_ = FileFlags.K_ALL;
		internal SHA1CryptoServiceProvider ShaContext { get; set; } = new SHA1CryptoServiceProvider();

		public MediaHeader Header { get; private set; } = new MediaHeader();

		public byte[] Content { get; set; }
		byte[] PaddingBytes { get; set; }

		public void GenerateHash()
		{
			this.ShaContext.Initialize();

			PhxHash.UInt16(this.ShaContext, (ushort) this.flags_);
			PhxHash.UInt16(this.ShaContext, K_VERSION_);

			this.Header.UpdateHash(this.ShaContext);
		}

		static uint WriteRandomBlock(IO.EndianWriter s, uint seed = 1)
		{
			for (int x = K_RANDOM_BLOCK_WORDS_; x > 0; x--)
			{
				uint r8 = seed << 17;
				uint r7 = r8 ^ seed;
				uint r6 = r7 >> 13;
				uint r5 = r6 ^ r7;
				uint r4 = r5 << 5;
				seed = r4 ^ r5;
				s.Write(seed);
			}

			return seed;
		}

	static void Stream(IO.EndianStream s, bool crypt, IO.IEndianStreamSerializable obj,
			long size = 0, ulong userKey = 0, Action<IO.EndianStream> streamLeftovers = null)
		{
			if(!crypt)
			{
				obj.Serialize(s);

				if (s.IsWriting && streamLeftovers != null)
					streamLeftovers(s);
			}
			else
			{
				using (var ms = new System.IO.MemoryStream())
				using (var crypted = new IO.EndianStream(ms, s.ByteOrder))
				{
					crypted.StreamMode = s.StreamMode;

					if (s.IsReading)
					{
						var tea = new Security.Cryptography.PhxTea(s.Reader, crypted.Writer);
						tea.InitializeKey(Security.Cryptography.PhxTea.KKeyGameFile, userKey);
						tea.Decrypt(size);

						crypted.Seek(0);
					}

					obj.Serialize(crypted);

					if (streamLeftovers != null)
						streamLeftovers(crypted);

					if (s.IsWriting)
					{
						crypted.Seek(0);

						var tea = new Security.Cryptography.PhxTea(crypted.Reader, s.Writer);
						tea.InitializeKey(Security.Cryptography.PhxTea.KKeyGameFile, userKey);
						tea.Encrypt(size);
					}
				}
			}
		}

	public void Dispose()
		{
			if (this.ShaContext != null)
			{
				this.ShaContext.Dispose();
				this.ShaContext = null;
			}
		}

		#region IEndianStreamSerializable Members
		void ReadLeftovers(IO.EndianReader er)
		{
			this.PaddingBytes = new byte[(int)(er.BaseStream.Length - er.BaseStream.Position)];
			er.Read(this.PaddingBytes, 0, this.PaddingBytes.Length);
		}
		void WriteLeftovers(IO.EndianWriter ew)
		{
//			WriteRandomBlocks(ew);

			if (ew.BaseStream.Length < K_MAX_CONTENT_SIZE_)
			{
				int paddingBytesCount = System.Math.Min(this.PaddingBytes.Length, K_MAX_CONTENT_SIZE_ - (int)(ew.BaseStream.Length));
				ew.Write(this.PaddingBytes, 0, paddingBytesCount);
			}

			if (ew.BaseStream.Length < K_MAX_CONTENT_SIZE_)
			{
				byte[] zero = new byte[K_MAX_CONTENT_SIZE_ - (int)(ew.BaseStream.Length)];
				Array.Clear(zero, 0, zero.Length);
				ew.Write(zero, 0, zero.Length);
			}
		}
		void StreamLeftovers(IO.EndianStream s)
		{
				 if (s.IsReading)
					 this.ReadLeftovers(s.Reader);
			else if (s.IsWriting)
				this.WriteLeftovers(s.Writer);
		}
		void StreamCompressedContent(IO.EndianStream s)
		{
			if (s.IsReading)
			{
				using (var cs = new CompressedStream(true))
				{
					Stream(s, EnumFlags.Test(this.flags_, FileFlags.ENCRYPT_CONTENT), cs,
						userKey: this.Header.dataCryptKey, streamLeftovers: this.StreamLeftovers);

					cs.Decompress();
					this.Content = cs.UncompressedData;
				}
			}
			else if (s.IsWriting)
			{
				using (var cs = new CompressedStream(true))
				using (var ms = new System.IO.MemoryStream(K_MAX_CONTENT_SIZE_))
				using (var sout = new IO.EndianWriter(ms, s.ByteOrder))
				{
					sout.Write(this.Content);
					sout.Seek(0);

					cs.InitializeFromStream(ms);
					cs.Compress();

					Stream(s, EnumFlags.Test(this.flags_, FileFlags.ENCRYPT_CONTENT), cs,
						userKey: this.Header.dataCryptKey, streamLeftovers: this.StreamLeftovers);
				}
			}
		}
		public void Serialize(IO.EndianStream s)
		{
			s.Owner = this;

			if (s.IsWriting)
			{
				//Flags = EnumFlags.Remove(Flags, FileFlags.EncryptHeader | FileFlags.EncryptContent);
			}

			s.Stream(ref this.flags_, FileFlagsStreamer.Instance);
			s.StreamVersion(K_VERSION_);

			Stream(s, EnumFlags.Test(this.flags_, FileFlags.ENCRYPT_HEADER), this.Header, MediaHeader.K_SIZE_OF);
			this.GenerateHash();

			if (EnumFlags.Test(this.flags_, FileFlags.COMPRESS_CONTENT))
			{
				this.StreamCompressedContent(s);
			}
			else
			{
				if (s.IsReading)
					this.Content = new byte[(int)(s.BaseStream.Length - s.BaseStream.Position)];

				s.Stream(this.Content);
			}
		}
		#endregion

		#region IEndianStreamable Members
#if false // TODO: verify the new IEndianStreamSerializable impl
		public void Read(IO.EndianReader s)
		{
			s.Owner = this;

			Flags = s.Read(FileFlagsStreamer.Instance);
			Version = s.ReadUInt16();
			if (Version != kVersion) throw new IO.VersionMismatchException(s.BaseStream,
				kVersion, Version);

			Read(s, EnumFlags.Test(Flags, FileFlags.EncryptHeader), Header, MediaHeader.kSizeOf);
			GenerateHash();

			if (EnumFlags.Test(Flags, FileFlags.CompressContent))
			{
				using (var cs = new CompressedStream(true))
				{
					Read(s, EnumFlags.Test(Flags, FileFlags.EncryptContent), cs,
						userKey: Header.DataCryptKey, readLeftovers: ReadLeftovers);

					cs.Decompress();
					Content = cs.UncompressedData;
				}
			}
			else
				Content = s.ReadBytes((int)(s.BaseStream.Length - s.BaseStream.Position));
		}
		public void Write(IO.EndianWriter s)
		{
			//Flags = EnumFlags.Remove(Flags, FileFlags.EncryptHeader | FileFlags.EncryptContent);

			s.Write(Flags, FileFlagsStreamer.Instance);
			s.Write((ushort)kVersion);

			Write(s, EnumFlags.Test(Flags, FileFlags.EncryptHeader), Header, MediaHeader.kSizeOf);
			GenerateHash();

			if (EnumFlags.Test(Flags, FileFlags.CompressContent))
			{
				using (var cs = new CompressedStream(true))
				using (var ms = new System.IO.MemoryStream(kMaxContentSize))
				using (var sout = new IO.EndianWriter(ms, Shell.EndianFormat.Big))
				{
					sout.Write(Content);
					sout.Seek(0);

					cs.InitializeFromStream(ms);
					cs.Compress();

					Write(s, EnumFlags.Test(Flags, FileFlags.EncryptContent), cs,
						userKey: Header.DataCryptKey, writeLeftovers: WriteLeftovers);
				}
			}
			else
				s.Write(Content);
		}
#endif
		#endregion
	};
}
