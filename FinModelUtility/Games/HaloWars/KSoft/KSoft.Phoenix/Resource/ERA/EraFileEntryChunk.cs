using System;

namespace KSoft.Phoenix.Resource
{
	public sealed class EraFileEntryChunk
		: ECF.EcfChunk
	{
		#region SizeOf
		public new const int K_SIZE_OF = 0x38;
		public const int K_COMPRESSSED_DATA_TIGER_HASH_SIZE = 16;
		public const uint K_EXTRA_DATA_SIZE = K_SIZE_OF - ECF.EcfChunk.K_SIZE_OF;

		public static int CalculateFileChunksSize(int fileCount)
		{
			return K_SIZE_OF * fileCount;
		}
		#endregion

		#region Struct fields
		private ulong mFileTimeBits_; // FILETIME
		public int dataUncompressedSize;
		// First 128 bits of the compressed data's Tiger hash
		public byte[] compressedDataTiger128 = new byte[K_COMPRESSSED_DATA_TIGER_HASH_SIZE];
		// actually only 24 bits, big endian
		public uint fileNameOffset;
		#endregion

		public string fileName;
		public DateTime FileDateTime
		{
			get { return DateTime.FromFileTimeUtc((long) this.mFileTimeBits_); }
			set { this.mFileTimeBits_ = (ulong)value.ToFileTimeUtc(); }
		}

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			var eraUtil = s.Owner as EraFileUtil;
			long position = s.BaseStream.Position;

			base.Serialize(s);

			s.Stream(ref this.mFileTimeBits_);
			s.Stream(ref this.dataUncompressedSize);

			if (s.IsWriting)
			{
				Bitwise.ByteSwap.SwapInt64(this.compressedDataTiger128, sizeof(ulong) * 0);
				Bitwise.ByteSwap.SwapInt64(this.compressedDataTiger128, sizeof(ulong) * 1);
			}
			s.Stream(this.compressedDataTiger128);
			{
				Bitwise.ByteSwap.SwapInt64(this.compressedDataTiger128, sizeof(ulong) * 0);
				Bitwise.ByteSwap.SwapInt64(this.compressedDataTiger128, sizeof(ulong) * 1);
			}


			s.StreamUInt24(ref this.fileNameOffset);
			s.Pad8();

			if (eraUtil != null && eraUtil.DebugOutput != null)
			{
				eraUtil.DebugOutput.Write("FileEntry: {0} @{1} offset={2} end={3} size={4} dsize={5} adler={6} ",
					this.entryId.ToString("X16"),
					position.ToString("X8"),
					this.dataOffset.u32.ToString("X8"),
					(this.dataOffset.u32 + this.dataSize).ToString("X8"),
					this.dataSize.ToString("X8"),
					this.dataUncompressedSize.ToString("X8"),
					this.adler32.ToString("X8"));

				if (!string.IsNullOrEmpty(this.fileName))
					eraUtil.DebugOutput.Write(this.fileName);

				eraUtil.DebugOutput.WriteLine();
			}
		}
		#endregion

		#region Xml Streaming
		string FileDateTimeString { get {
			return this.FileDateTime.ToString("u"); // UniversalSorta­bleDateTimePat­tern
		} }

		protected override void WriteFields(IO.XmlElementStream s, bool includeFileData)
		{
			if (includeFileData && this.mFileTimeBits_ != 0)
				s.WriteAttribute("fileTime", this.mFileTimeBits_.ToString("X16"));

			// only because it's interesting to have, never read back in
			s.WriteAttribute("fileDateTime", this.FileDateTimeString);

			base.WriteFields(s, includeFileData);

			// When we extract, we decode xmbs
			string fn = this.fileName;
			if (ResourceUtils.IsXmbFile(fn))
			{
				bool removeXmbExt = true;

				var expander = s.Owner as EraFileExpander;
				if (expander != null && expander.expanderOptions.Test(EraFileExpanderOptions.DONT_TRANSLATE_XMB_FILES))
					removeXmbExt = false;

				if (removeXmbExt)
					ResourceUtils.RemoveXmbExtension(ref fn);
			}
			s.WriteAttribute("name", fn);

			if (includeFileData)
			{
				if (this.dataUncompressedSize != this.dataSize)
					s.WriteAttribute("fullSize", this.dataUncompressedSize.ToString("X8"));

				s.WriteAttribute("compressedDataHash",
					Text.Util.ByteArrayToString(this.compressedDataTiger128));

				s.WriteAttribute("nameOffset", this.fileNameOffset.ToString("X6"));
			}

			this.WriteFlags(s);
			this.WriteResourceFlags(s);
		}

		protected override void ReadFields(IO.XmlElementStream s, bool includeFileData)
		{
			base.ReadFields(s, includeFileData);

			s.ReadAttributeOpt("fileTime", ref this.mFileTimeBits_, NumeralBase.HEX);

			s.ReadAttribute("name", ref this.fileName);

			s.ReadAttributeOpt("fullSize", ref this.dataUncompressedSize, NumeralBase.HEX);
			s.ReadAttributeOpt("nameOffset", ref this.fileNameOffset, NumeralBase.HEX);

			string hashString = null;
			if (s.ReadAttributeOpt("compressedDataHash", ref hashString))
				this.compressedDataTiger128 = Text.Util.ByteStringToArray(hashString);
		}
		#endregion

		#region Buffer Util
		protected override byte[] DecompressFromBuffer(IO.EndianStream blockStream, byte[] buffer)
		{
			uint resultAdler;
			return ResourceUtils.Decompress(buffer, this.dataUncompressedSize, out resultAdler);
		}

		public override void BuildBuffer(IO.EndianStream blockStream, System.IO.Stream sourceFile,
			Security.Cryptography.TigerHashBase hasher)
		{
			base.BuildBuffer(blockStream, sourceFile, hasher);

			this.ComputeHash(blockStream, hasher);
			Array.Copy(hasher.Hash, 0, this.compressedDataTiger128, 0, this.compressedDataTiger128.Length);
		}

		protected override void CompressSourceToStream(IO.EndianWriter blockStream, System.IO.Stream sourceFile)
		{
			this.dataUncompressedSize = (int)sourceFile.Length;

			base.CompressSourceToStream(blockStream, sourceFile);
		}
		#endregion

		public override string ToString()
		{
			return string.Format("{0}",
			                     this.fileName);
		}
	};
}