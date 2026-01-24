using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.Resource
{
	using Adler32 = Security.Cryptography.Adler32;

	partial class CompressedStream
	{
		static class InMemoryRep
		{
			const int K_OFFSET_SOURCE_SIZE_ = 0;
			const int K_OFFSET_SOURCE_ADLER_ = K_OFFSET_SOURCE_SIZE_ + sizeof(ulong);
			const int K_OFFSET_COMPRESSED_SIZE_ = K_OFFSET_SOURCE_ADLER_ + sizeof(uint);
			const int K_OFFSET_COMPRESSED_ADLER_ = K_OFFSET_COMPRESSED_SIZE_ + sizeof(ulong);
			const int K_OFFSET_MODE_ = K_OFFSET_COMPRESSED_ADLER_ + sizeof(uint);

	  public static uint Checksum(ulong srcSize, uint srcAdler, ulong cmpSize, uint cmpAdler,
				uint mode = 0)
			{
				var bc = Adler32.BitComputer.New;

				bc.ComputeBe(srcSize);
				bc.ComputeBe(srcAdler);
				bc.ComputeBe(cmpSize);
				bc.ComputeBe(cmpAdler);
				bc.ComputeBe(mode);

				return bc.ComputeFinish();
			}
		};

		static readonly Header KBufferedHeader = new Header() {
			headerAdler32 = 0x00330004,
			streamMode = (uint)Mode.BUFFERED,
			uncompressedSize = 0,	compressedSize = 0,
			uncompressedAdler32 = 1,	compressedAdler32 = 1,
		};

		public static void CompressFromStream(IO.EndianWriter blockStream, System.IO.Stream source,
			out uint streamAdler, out int streamSize)
		{
			Contract.Requires<ArgumentNullException>(blockStream != null);
			Contract.Requires<ArgumentNullException>(source != null);

			using (var ms = new System.IO.MemoryStream((int)source.Length + Header.K_SIZE_OF))
			using (var s = new IO.EndianStream(ms, Shell.EndianFormat.BIG, permissions: FA.Write))
			using (var cs = new CompressedStream())
			{
				s.StreamMode = FA.Write;

				cs.InitializeFromStream(source);
				cs.Compress();

				cs.Serialize(s);

				ms.Position = 0;
				streamSize = (int)ms.Length;
				streamAdler = Adler32.Compute(ms, streamSize);

				ms.WriteTo(blockStream.BaseStream);
			}
		}
		public static byte[] DecompressFromStream(IO.EndianStream blockStream)
		{
			Contract.Requires<ArgumentNullException>(blockStream != null);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			byte[] buffer;
			using (var cs = new CompressedStream())
			{
				cs.Serialize(blockStream);
				cs.Decompress();
				buffer = cs.UncompressedData;
			}

			return buffer;
		}
	};
}