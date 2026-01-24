
namespace KSoft.Phoenix.Resource
{
	partial class CompressedStream
	{
		struct Header
			: IO.IEndianStreamSerializable
		{
			public const int K_SIZE_OF = 0x24;

			internal uint headerAdler32;
			public uint streamMode;
			public ulong uncompressedSize, compressedSize;
			public uint uncompressedAdler32, compressedAdler32;

			public bool UseBufferedStreaming { get {
				return this.streamMode == (uint)Mode.BUFFERED;
			} }

			public void UpdateHeaderCrc()
			{
				this.headerAdler32 = InMemoryRep.Checksum(this.uncompressedSize,
				                                          this.uncompressedAdler32,
				                                          this.compressedSize,
				                                          this.compressedAdler32,
				                                          this.streamMode);
			}

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				if (s.IsWriting)
					this.UpdateHeaderCrc();

				s.StreamSignature(K_SIGNATURE);
				s.Stream(ref this.headerAdler32);
				s.Stream(ref this.streamMode);
				s.Stream(ref this.uncompressedSize);
				s.Stream(ref this.compressedSize);
				s.Stream(ref this.uncompressedAdler32);
				s.Stream(ref this.compressedAdler32);
			}
			#endregion
		};
	};
}