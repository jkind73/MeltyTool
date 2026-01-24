using System;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Resource
{
	sealed class GameTitleMemory0
		: IO.IEndianStreamSerializable
	{
		const byte K_VERSION_ = 5;
		public int automaticDifficultyMultiplier; // max: 200
		public int unk8; // max: 100

		byte unkLength_; // 0xD in mine (latest) TU
		// 0x1...0xD
		public byte[] UnkC { get; private set; }
		// 6 6 5 2 3 3 3 0 0 0 4 0 0
		public byte[] Unk18 { get; private set; }

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamVersion(K_VERSION_);
			s.Stream(ref this.automaticDifficultyMultiplier);
			s.Stream(ref this.unk8);

			s.Stream(ref this.unkLength_);
			if (s.IsReading)
			{
				this.UnkC = new byte[this.unkLength_];
				this.Unk18 = new byte[this.unkLength_];
			}
			s.Stream(this.UnkC);
			s.Stream(this.Unk18);
		}
		#endregion
	};

	sealed class GameTitleMemory1
		: IO.IEndianStreamSerializable
	{
	const byte K_VERSION_TU_ = 0x1C;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamVersion(K_VERSION_TU_);

			Contract.Assert(false, "TODO");
		}
		#endregion
	};

	public sealed class GameTitleMemory
	{
		long mMemoryOffset0_, mMemoryOffset1_;
		GameTitleMemory0 mMemory0_ = new GameTitleMemory0();
		GameTitleMemory1 mMemory1_ = new GameTitleMemory1();

		public void SetTitleMemoryOffsets(Stream gpdBaseStream, long tm0, long tm1)
		{
			Contract.Requires<ArgumentNullException>(gpdBaseStream != null);
			Contract.Requires<ArgumentException>(gpdBaseStream.CanSeek);
			Contract.Requires<ArgumentOutOfRangeException>(tm0 >= 0 && tm0 < gpdBaseStream.Length);
			Contract.Requires<ArgumentOutOfRangeException>(tm1 >= 0 && tm1 < gpdBaseStream.Length);

			this.mMemoryOffset0_ = tm0;
			this.mMemoryOffset1_ = tm1;
		}

		void SerializeTitleMemory(IO.EndianStream gpdStream, long tmOffset, IO.IEndianStreamSerializable tm)
		{
			gpdStream.Seek(tmOffset);

			if (gpdStream.IsReading)
			{
				byte[] uncompressedData = CompressedStream.DecompressFromStream(gpdStream);
				using (var ms = new MemoryStream(uncompressedData, false))
				using (var s = new IO.EndianStream(ms, FileAccess.Read))
				{
					s.StreamMode = FileAccess.Read;
					s.Stream(tm);
				}
			}
			else if (gpdStream.IsWriting)
			{
				Contract.Assert(false, "TODO");

				using (var ms = new MemoryStream())
				using (var s = new IO.EndianStream(ms, FileAccess.Write))
				{
					s.StreamMode = FileAccess.Write;
					s.Stream(tm);

					ms.Seek(0, SeekOrigin.Begin);
					uint streamAdlr;
					int streamSize;
					CompressedStream.CompressFromStream(gpdStream.Writer, ms, out streamAdlr, out streamSize);
				}
			}
		}
		public void SerializeTitleMemory0(IO.EndianStream gpdStream)
		{
			Contract.Requires<ArgumentNullException>(gpdStream != null);
			Contract.Requires<ArgumentException>(gpdStream.BaseStream.CanSeek);

			this.SerializeTitleMemory(gpdStream, this.mMemoryOffset0_, this.mMemory0_);
		}
		public void SerializeTitleMemory1(IO.EndianStream gpdStream)
		{
			Contract.Requires<ArgumentNullException>(gpdStream != null);
			Contract.Requires<ArgumentException>(gpdStream.BaseStream.CanSeek);

			this.SerializeTitleMemory(gpdStream, this.mMemoryOffset1_, this.mMemory1_);
		}
	};
}