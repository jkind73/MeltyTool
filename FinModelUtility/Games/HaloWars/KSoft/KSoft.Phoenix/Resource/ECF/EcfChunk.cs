using System;
using System.IO;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Resource.ECF
{
	public enum EcfCompressionType : byte
	{
		STORED,
		/// <summary>File data is stored in a standard compressed buffer (no header/footer/etc data, just plain old compressed bytes)</summary>
		DEFLATE_RAW,
		/// <summary>File data is stored in a <see cref="CompressedStream"/> buffer</summary>
		DEFLATE_STREAM,
	};

	enum EcfChunkResourceFlags : ushort
	{
		CONTIGUOUS,
		WRITE_COMBINED, // only valid with Contiguous
		IS_DEFLATE_STREAM,
		IS_RESOURCE_TAG,
	};

	public class EcfChunk
		: IO.IEndianStreamSerializable
	{
		public const int K_MAX_COUNT = 32768;
		public const uint K_MAX_SIZE = 1024U * 1024U * 1024U;

		public const int K_SIZE_OF = 0x18;
		internal const string K_XML_ELEMENT_STREAM_NAME = "chunk";

		public const int K_DEFAULT_ALIGNMENT_BIT = 2;
		const byte K_COMPRESSION_TYPE_MASK_ = 7;

		#region Struct fields
		public ulong entryId;
		public Values.PtrHandle dataOffset = Values.PtrHandle.Null32; // offset within the parent block
		public int dataSize;
		public uint adler32;
		public byte flags;
		public byte dataAlignmentBit = K_DEFAULT_ALIGNMENT_BIT;
		private ushort mResourceFlags_;
		#endregion

		public ulong DecompressedDataTiger64
		{
			get { return this.entryId; }
			set { this.entryId = value; }
		}

		public EcfCompressionType CompressionType
		{
			get { return (EcfCompressionType)(this.flags & K_COMPRESSION_TYPE_MASK_); }
			set { this.flags |= (byte)( ((byte)(value)) & K_COMPRESSION_TYPE_MASK_ ); }
		}

		#region Resource flags
		public bool IsContiguous
		{
			get { return Bitwise.Flags.Test(this.mResourceFlags_, 1U<<(ushort)EcfChunkResourceFlags.CONTIGUOUS); }
			set { Bitwise.Flags.Modify(value, ref this.mResourceFlags_, (ushort)1U<<(ushort)EcfChunkResourceFlags.CONTIGUOUS); }
		}

		public bool IsWriteCombined
		{
			get { return Bitwise.Flags.Test(this.mResourceFlags_, 1U<<(ushort)EcfChunkResourceFlags.WRITE_COMBINED); }
			set { Bitwise.Flags.Modify(value, ref this.mResourceFlags_, (ushort)1U<<(ushort)EcfChunkResourceFlags.WRITE_COMBINED); }
		}

		public bool IsDeflateStream
		{
			get { return Bitwise.Flags.Test(this.mResourceFlags_, 1U<<(ushort)EcfChunkResourceFlags.IS_DEFLATE_STREAM); }
			set { Bitwise.Flags.Modify(value, ref this.mResourceFlags_, (ushort)1U<<(ushort)EcfChunkResourceFlags.IS_DEFLATE_STREAM); }
		}

		public bool IsResourceTag
		{
			get { return Bitwise.Flags.Test(this.mResourceFlags_, 1U<<(ushort)EcfChunkResourceFlags.IS_RESOURCE_TAG); }
			set { Bitwise.Flags.Modify(value, ref this.mResourceFlags_, (ushort)1U<<(ushort)EcfChunkResourceFlags.IS_RESOURCE_TAG); }
		}
		#endregion

		/// <summary>
		/// Flag is set but CompressionType is Stored. This will be the case with XMBs.
		/// </summary>
		public bool IsDeflateStreamButNoCompression { get {
			return this.IsDeflateStream && this.CompressionType == EcfCompressionType.STORED;
		} }

		public void SeekTo(IO.IKSoftBinaryStream blockStream)
		{
			blockStream.Seek((long) this.dataOffset);
		}
		public byte[] GetRawBuffer(IO.EndianStream blockStream)
		{
			this.SeekTo(blockStream);
			byte[] result = blockStream.Reader.ReadBytes(this.dataSize);

			return result;
		}

		#region Buffer Util
		public byte[] GetBuffer(IO.EndianStream blockStream)
		{
			byte[] result = null;

			var assumedCompressionType = this.CompressionType;

			// #NOTE CompressionType can be Stored but IsDeflateStream can be true (seen it in XMB).
			// So just handle the flag as we do EcfCompressionType.DeflateStream
			if (this.IsDeflateStream)
			{
				assumedCompressionType = EcfCompressionType.DEFLATE_STREAM;
			}

			switch (assumedCompressionType)
			{
				case EcfCompressionType.STORED:
					result = this.GetRawBuffer(blockStream);
					break;

				case EcfCompressionType.DEFLATE_RAW:
					result = this.GetRawBuffer(blockStream);
					result = this.DecompressFromBuffer(blockStream, result);
					break;

				case EcfCompressionType.DEFLATE_STREAM:
					result = this.DecompressFromStream(blockStream);
					break;

				default:
					throw new KSoft.Debug.UnreachableException(assumedCompressionType.ToString());
			}

			return result;
		}

		protected virtual byte[] DecompressFromBuffer(IO.EndianStream blockStream, byte[] buffer)
		{
			throw new InvalidOperationException(string.Format(
				"Can't get the decompressed bytes for {0} (from {1}). Need to know the uncompressed data size",
				this.entryId, blockStream.StreamName));
		}

		byte[] DecompressFromStream(IO.EndianStream blockStream)
		{
			this.SeekTo(blockStream);
			return CompressedStream.DecompressFromStream(blockStream);
		}

		public virtual void BuildBuffer(IO.EndianStream blockStream, Stream sourceFile
			, Security.Cryptography.TigerHashBase hasher = null)
		{
			blockStream.AlignToBoundry(this.dataAlignmentBit);

			sourceFile.Seek(0, SeekOrigin.Begin);
			if (hasher != null)
				this.UpdateDecompressedDataTigerHash(sourceFile, hasher);

			Contract.Assert(blockStream.BaseStream.Position == blockStream.BaseStream.Length);

			this.dataOffset = blockStream.PositionPtr;

			// #TODO determine if compressing the sourceFile data has any savings (eg, 7% smaller)

			var assumedCompressionType = this.CompressionType;

			// #NOTE CompressionType can be Stored but IsDeflateStream can be true (seen it in XMB).
			// So just handle the flag as we do EcfCompressionType.DeflateStream
			if (this.IsDeflateStream)
			{
				assumedCompressionType = EcfCompressionType.DEFLATE_STREAM;
			}

			switch (assumedCompressionType)
			{
				case EcfCompressionType.STORED:
				{
					// Update this ECF's size
					this.dataSize = (int)sourceFile.Length;
					// Also update this ECF's checksum
					this.adler32 = Security.Cryptography.Adler32.Compute(sourceFile, this.dataSize, restorePosition: true);
					// Copy the source file's bytes to the block stream
					sourceFile.CopyTo(blockStream.BaseStream);
					break;
				}

				case EcfCompressionType.DEFLATE_RAW:
					this.CompressSourceToStream(blockStream.Writer, sourceFile);
					break;

				case EcfCompressionType.DEFLATE_STREAM:
					this.CompressSourceToCompressionStream(blockStream.Writer, sourceFile);
					break;

				default:
					throw new KSoft.Debug.UnreachableException(assumedCompressionType.ToString());
			}

			Contract.Assert(blockStream.BaseStream.Position == ((long) this.dataOffset + this.dataSize));
		}

		protected virtual void CompressSourceToStream(IO.EndianWriter blockStream, Stream sourceFile)
		{
			// Read the source bytes
			byte[] buffer = new byte[sourceFile.Length];
			for (int x = 0; x < buffer.Length;)
			{
				int n = sourceFile.Read(buffer, x, buffer.Length - x);
				x += n;
			}

			// Compress the source bytes into a new buffer
			byte[] result = ResourceUtils.Compress(buffer, out this.adler32); // Also update this ECF's checksum
			this.dataSize = result.Length;                                       // Update this ECF's size

			// Write the compressed bytes to the block stream
			blockStream.Write(result);
		}

		protected void CompressSourceToCompressionStream(IO.EndianWriter blockStream, Stream sourceFile)
		{
			// Build a CompressedStream from the source file and write it to the block stream
			CompressedStream.CompressFromStream(blockStream, sourceFile,
				out this.adler32, out this.dataSize);  // Update this ECF's checksum and size
		}
		#endregion

		#region Hash Utils
		[Contracts.Pure]
		public uint ComputeAdler32(IO.EndianStream blockStream)
		{
			Contract.Requires(blockStream != null);

			this.SeekTo(blockStream);
			uint adler = Security.Cryptography.Adler32.Compute(blockStream.BaseStream, this.dataSize);
			return adler;
		}

		[Contracts.Pure]
		public void ComputeHash(IO.EndianStream blockStream, Security.Cryptography.TigerHashBase hasher)
		{
			Contract.Requires(blockStream != null);
			Contract.Requires(hasher != null);

			hasher.Initialize();
			hasher.ComputeHash(blockStream.BaseStream,
				(long) this.dataOffset,
				this.dataSize);
		}

		protected void UpdateDecompressedDataTigerHash(Stream source, Security.Cryptography.TigerHashBase hasher)
		{
			hasher.ComputeHash(source, 0, (int)source.Length,
				restorePosition: true);

			ulong tiger64;
			hasher.TryGetAsTiger64(out tiger64);
			this.DecompressedDataTiger64 = tiger64;
		}
		#endregion

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.entryId);
			s.StreamVirtualAddress(ref this.dataOffset);
			s.Stream(ref this.dataSize);
			s.Stream(ref this.adler32);
			s.Stream(ref this.flags);
			s.Stream(ref this.dataAlignmentBit);
			s.Stream(ref this.mResourceFlags_);
		}
		#endregion

		#region Xml Streaming
		public void Read(IO.XmlElementStream s, bool includeFileData)
		{
			this.ReadFields(s, includeFileData);
		}
		public void Write(IO.XmlElementStream s, bool includeFileData)
		{
			using (s.EnterCursorBookmark(K_XML_ELEMENT_STREAM_NAME))
				this.WriteFields(s, includeFileData);
		}

		protected virtual void ReadFields(IO.XmlElementStream s, bool includeFileData)
		{
			s.ReadAttributeOpt("id", ref this.entryId, NumeralBase.HEX);
			//s.ReadAttributeOpt("flags", ref Flags, NumeralBase.Hex);
			s.ReadAttributeOpt("align", ref this.dataAlignmentBit, NumeralBase.HEX);
			if (includeFileData)
			{
				s.ReadAttributeOpt("offset", ref this.dataOffset.u32, NumeralBase.HEX);
				s.ReadAttributeOpt("size", ref this.dataSize, NumeralBase.HEX);
			}

			this.ReadFlags(s);
			this.ReadResourceFlags(s);
		}
		protected virtual void WriteFields(IO.XmlElementStream s, bool includeFileData)
		{
			s.WriteAttribute("id", this.entryId.ToString("X16"));
			//if (Flags != 0)
			//	s.WriteAttribute("flags", Flags.ToString("X1"));
			if (this.dataAlignmentBit != K_DEFAULT_ALIGNMENT_BIT)
				s.WriteAttribute("align", this.dataAlignmentBit.ToString("X1"));
			if (includeFileData)
			{
				s.WriteAttribute("offset", this.dataOffset.u32.ToString("X8"));
				s.WriteAttribute("size", this.dataSize.ToString("X8"));
			}
		}

		protected void ReadFlags(IO.XmlElementStream s)
		{
			var compType = EcfCompressionType.STORED;
			if (s.ReadAttributeEnumOpt("Compression", ref compType))
				this.CompressionType = compType;
		}
		protected void WriteFlags(IO.XmlElementStream s)
		{
			if (this.flags == 0)
				return;

			s.WriteAttributeEnum("Compression", this.CompressionType);
		}

		protected void ReadResourceFlags(IO.XmlElementStream s)
		{
			bool flag = false;
			if (s.ReadAttributeOpt("IsContiguous", ref flag))
				this.IsContiguous = flag;
			if (s.ReadAttributeOpt("IsWriteCombined", ref flag))
				this.IsWriteCombined = flag;
			if (s.ReadAttributeOpt("IsDeflateStream", ref flag))
				this.IsDeflateStream = flag;
			if (s.ReadAttributeOpt("IsResourceTag", ref flag))
				this.IsResourceTag = flag;
		}
		protected void WriteResourceFlags(IO.XmlElementStream s)
		{
			if (this.mResourceFlags_ == 0)
				return;

			if (this.IsContiguous)
				s.WriteAttribute("IsContiguous", true);
			if (this.IsWriteCombined)
				s.WriteAttribute("IsWriteCombined", true);
			if (this.IsDeflateStream)
				s.WriteAttribute("IsDeflateStream", true);
			if (this.IsResourceTag)
				s.WriteAttribute("IsResourceTag", true);
		}
		#endregion
	};
}
