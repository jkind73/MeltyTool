using System;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

// http://en.wikipedia.org/wiki/Unix_File_System

namespace KSoft.Phoenix.Resource.ECF
{
	public struct EcfHeader
		: IO.IEndianStreamSerializable
	{
		public const uint K_SIGNATURE = 0xDABA7737;
		public const int K_SIZE_OF = 0x20;
		public const int K_ADLER32_START_OFFSET = sizeof(uint) * 3;

		public int headerSize;
		// Checksum of the TotalSize field and onward, added to the checksum of everything after the header (HeaderSize - sizeof(ECFHeader))
		public uint adler32;

		public int totalSize;
		public short chunkCount;
		private ushort mFlags_;
		private uint mId_; // The signature of the data which this header encapsulates
		private ushort mExtraDataSize_;

		public int Adler32BufferLength { get { return this.headerSize - K_ADLER32_START_OFFSET; } }
		public uint Id { get { return this.mId_; } }
		public ushort ExtraDataSize { get { return this.mExtraDataSize_; } }

		public void InitializeChunkInfo(uint dataId, uint dataChunkExtraDataSize = 0)
		{
			this.mId_ = dataId;
			this.mExtraDataSize_ = (ushort)dataChunkExtraDataSize;
		}

		public void BeginBlock(IO.IKSoftBinaryStream s)
		{
			s.VirtualAddressTranslationInitialize(Shell.ProcessorSize.X32);
			s.VirtualAddressTranslationPush(s.PositionPtr);
		}
		public void EndBlock(IO.IKSoftBinaryStream s)
		{
			s.VirtualAddressTranslationPop();
		}

		public void UpdateTotalSize(Stream s, int startOffset = 0)
		{
			Contract.Requires(startOffset >= 0);

			this.totalSize = (int)(s.Length - startOffset);
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamSignature(K_SIGNATURE);
			s.Stream(ref this.headerSize);
			s.Stream(ref this.adler32);
			s.Stream(ref this.totalSize);
			s.Stream(ref this.chunkCount);
			s.Stream(ref this.mFlags_);

			if (s.IsReading && this.mFlags_ != 0)
			{
				// TODO: trace
				System.Diagnostics.Debugger.Break();
			}

			s.Stream(ref this.mId_);
			s.Stream(ref this.mExtraDataSize_);
			s.Pad(sizeof(short) + sizeof(int));
		}
		#endregion

		public int CalculateChunkEntriesSize(
			int assumedChunkCount = TypeExtensions.K_NONE)
		{
			if (assumedChunkCount.IsNone())
				assumedChunkCount = this.chunkCount;

			int entriesSize = EcfChunk.K_SIZE_OF;
			entriesSize += this.ExtraDataSize;
			entriesSize *= assumedChunkCount;

			return entriesSize;
		}

		public uint ComputeAdler32(Stream stream, long headerPosition)
		{
			Contract.Requires(stream != null);
			Contract.Requires(headerPosition >= 0);

			long currentPosition = stream.Position;

			long adlerStartPosition = headerPosition + K_ADLER32_START_OFFSET;
			stream.Seek(adlerStartPosition, SeekOrigin.Begin);
			var adler = Security.Cryptography.Adler32.Compute(stream, this.Adler32BufferLength);

			stream.Seek(currentPosition, SeekOrigin.Begin);

			return adler;
		}

		public void ComputeAdler32AndWrite(IO.EndianStream s, long headerPosition)
		{
			Contract.Requires(s != null);
			Contract.Requires(headerPosition >= 0);

			long currentPosition = s.BaseStream.Position;

			long adlerStartPosition = headerPosition + K_ADLER32_START_OFFSET;
			s.BaseStream.Seek(adlerStartPosition, SeekOrigin.Begin);
			var adler = Security.Cryptography.Adler32.Compute(s.BaseStream, this.Adler32BufferLength);
			this.adler32 = adler;

			s.BaseStream.Seek(headerPosition, SeekOrigin.Begin);
			this.Serialize(s);

			s.BaseStream.Seek(currentPosition, SeekOrigin.Begin);
		}

		public static bool VerifyIsEcf(IO.EndianReader s)
		{
			const int kSizeofSignature = sizeof(uint);

			Contract.Requires<InvalidOperationException>(s.BaseStream.CanRead);
			Contract.Requires<InvalidOperationException>(s.BaseStream.CanSeek);

			var baseStream = s.BaseStream;
			if ((baseStream.Length - baseStream.Position) < kSizeofSignature)
			{
				return false;
			}

			uint sig = s.ReadUInt32();
			baseStream.Seek(-kSizeofSignature, SeekOrigin.Current);

			return sig == K_SIGNATURE;
		}
	};
}