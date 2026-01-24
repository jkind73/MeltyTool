#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using TWord = System.UInt32;

namespace KSoft.IO
{
	partial class BitStream
	{
		/// <summary>Read an <see cref="System.Char"/> from the stream</summary>
		/// <param name="bitCount">Number of bits to read</param>
		/// <returns></returns>
		public char ReadChar(int bitCount = Bits.K_CHAR_BIT_COUNT
			)
		{
			Contract.Requires(bitCount <= Bits.K_CHAR_BIT_COUNT);

			this.ReadWord(out TWord word, bitCount);

			return (char)word;
		}
		/// <summary>Read an <see cref="System.Byte"/> from the stream</summary>
		/// <param name="bitCount">Number of bits to read</param>
		/// <returns></returns>
		public byte ReadByte(int bitCount = Bits.K_BYTE_BIT_COUNT
			)
		{
			Contract.Requires(bitCount <= Bits.K_BYTE_BIT_COUNT);

			this.ReadWord(out TWord word, bitCount);

			return (byte)word;
		}
		/// <summary>Read an <see cref="System.SByte"/> from the stream</summary>
		/// <param name="bitCount">Number of bits to read</param>
		/// <param name="signExtend">If true, the result will have the MSB extended</param>
		/// <returns></returns>
		public sbyte ReadSByte(int bitCount = Bits.K_S_BYTE_BIT_COUNT
			, bool signExtend = false
			)
		{
			Contract.Requires(bitCount <= Bits.K_S_BYTE_BIT_COUNT);

			this.ReadWord(out TWord word, bitCount);
			if (signExtend && bitCount != Bits.K_S_BYTE_BIT_COUNT)
				return (sbyte)Bits.SignExtend( (sbyte)word, bitCount );

			return (sbyte)word;
		}
		/// <summary>Read an <see cref="System.UInt16"/> from the stream</summary>
		/// <param name="bitCount">Number of bits to read</param>
		/// <returns></returns>
		public ushort ReadUInt16(int bitCount = Bits.K_U_INT16_BIT_COUNT
			)
		{
			Contract.Requires(bitCount <= Bits.K_U_INT16_BIT_COUNT);

			this.ReadWord(out TWord word, bitCount);

			return (ushort)word;
		}
		/// <summary>Read an <see cref="System.Int16"/> from the stream</summary>
		/// <param name="bitCount">Number of bits to read</param>
		/// <param name="signExtend">If true, the result will have the MSB extended</param>
		/// <returns></returns>
		public short ReadInt16(int bitCount = Bits.K_INT16_BIT_COUNT
			, bool signExtend = false
			)
		{
			Contract.Requires(bitCount <= Bits.K_INT16_BIT_COUNT);

			this.ReadWord(out TWord word, bitCount);
			if (signExtend && bitCount != Bits.K_INT16_BIT_COUNT)
				return (short)Bits.SignExtend( (short)word, bitCount );

			return (short)word;
		}
		/// <summary>Read an <see cref="System.UInt32"/> from the stream</summary>
		/// <param name="bitCount">Number of bits to read</param>
		/// <returns></returns>
		public uint ReadUInt32(int bitCount = Bits.K_U_INT32_BIT_COUNT
			)
		{
			Contract.Requires(bitCount <= Bits.K_U_INT32_BIT_COUNT);

			this.ReadWord(out TWord word, bitCount);

			return (uint)word;
		}
		/// <summary>Read an <see cref="System.Int32"/> from the stream</summary>
		/// <param name="bitCount">Number of bits to read</param>
		/// <param name="signExtend">If true, the result will have the MSB extended</param>
		/// <returns></returns>
		public int ReadInt32(int bitCount = Bits.K_INT32_BIT_COUNT
			, bool signExtend = false
			)
		{
			Contract.Requires(bitCount <= Bits.K_INT32_BIT_COUNT);

			this.ReadWord(out TWord word, bitCount);
			if (signExtend && bitCount != Bits.K_INT32_BIT_COUNT)
				return (int)Bits.SignExtend( (int)word, bitCount );

			return (int)word;
		}
		/// <summary>Read an <see cref="System.UInt64"/> from the stream</summary>
		/// <param name="bitCount">Number of bits to read</param>
		/// <returns></returns>
		public ulong ReadUInt64(int bitCount = Bits.K_U_INT64_BIT_COUNT
			)
		{
			Contract.Requires(bitCount <= Bits.K_U_INT64_BIT_COUNT);

			uint msbWord = 0;
			int msbBitCount = bitCount > Bits.K_INT32_BIT_COUNT ? bitCount - Bits.K_INT32_BIT_COUNT : 0;
			int lsbBitCount = bitCount > Bits.K_INT32_BIT_COUNT ? bitCount - msbBitCount : bitCount;

			if (msbBitCount > 0)
				this.ReadWord(out msbWord, msbBitCount);
			this.ReadWord(out uint lsbWord, lsbBitCount);

			ulong word = (ulong)msbWord << lsbBitCount;
			word |= (ulong)lsbWord;

			return (ulong)word;
		}
		/// <summary>Read an <see cref="System.Int64"/> from the stream</summary>
		/// <param name="bitCount">Number of bits to read</param>
		/// <param name="signExtend">If true, the result will have the MSB extended</param>
		/// <returns></returns>
		public long ReadInt64(int bitCount = Bits.K_INT64_BIT_COUNT
			, bool signExtend = false
			)
		{
			Contract.Requires(bitCount <= Bits.K_INT64_BIT_COUNT);

			uint msbWord = 0;
			int msbBitCount = bitCount > Bits.K_INT32_BIT_COUNT ? bitCount - Bits.K_INT32_BIT_COUNT : 0;
			int lsbBitCount = bitCount > Bits.K_INT32_BIT_COUNT ? bitCount - msbBitCount : bitCount;

			if (msbBitCount > 0)
				this.ReadWord(out msbWord, msbBitCount);
			this.ReadWord(out uint lsbWord, lsbBitCount);

			ulong word = (ulong)msbWord << lsbBitCount;
			word |= (ulong)lsbWord;
			if (signExtend && bitCount != Bits.K_INT64_BIT_COUNT)
				return (long)Bits.SignExtend( (long)word, bitCount );

			return (long)word;
		}

		/// <summary>Read an <see cref="System.Char"/> from the stream</summary>
		/// <param name="value">value read from the stream</param>
		/// <param name="bitCount">Number of bits to read</param>
		public void Read(out char value, int bitCount = Bits.K_CHAR_BIT_COUNT
			)
		{
			Contract.Requires(bitCount <= Bits.K_CHAR_BIT_COUNT);

			value = this.ReadChar(bitCount);
		}
		/// <summary>Read an <see cref="System.Byte"/> from the stream</summary>
		/// <param name="value">value read from the stream</param>
		/// <param name="bitCount">Number of bits to read</param>
		public void Read(out byte value, int bitCount = Bits.K_BYTE_BIT_COUNT
			)
		{
			Contract.Requires(bitCount <= Bits.K_BYTE_BIT_COUNT);

			value = this.ReadByte(bitCount);
		}
		/// <summary>Read an <see cref="System.SByte"/> from the stream</summary>
		/// <param name="value">value read from the stream</param>
		/// <param name="bitCount">Number of bits to read</param>
		/// <param name="signExtend">If true, the result will have the MSB extended</param>
		public void Read(out sbyte value, int bitCount = Bits.K_S_BYTE_BIT_COUNT
			, bool signExtend = false
			)
		{
			Contract.Requires(bitCount <= Bits.K_S_BYTE_BIT_COUNT);

			value = this.ReadSByte(bitCount, signExtend);
		}
		/// <summary>Read an <see cref="System.UInt16"/> from the stream</summary>
		/// <param name="value">value read from the stream</param>
		/// <param name="bitCount">Number of bits to read</param>
		public void Read(out ushort value, int bitCount = Bits.K_U_INT16_BIT_COUNT
			)
		{
			Contract.Requires(bitCount <= Bits.K_U_INT16_BIT_COUNT);

			value = this.ReadUInt16(bitCount);
		}
		/// <summary>Read an <see cref="System.Int16"/> from the stream</summary>
		/// <param name="value">value read from the stream</param>
		/// <param name="bitCount">Number of bits to read</param>
		/// <param name="signExtend">If true, the result will have the MSB extended</param>
		public void Read(out short value, int bitCount = Bits.K_INT16_BIT_COUNT
			, bool signExtend = false
			)
		{
			Contract.Requires(bitCount <= Bits.K_INT16_BIT_COUNT);

			value = this.ReadInt16(bitCount, signExtend);
		}
		/// <summary>Read an <see cref="System.UInt32"/> from the stream</summary>
		/// <param name="value">value read from the stream</param>
		/// <param name="bitCount">Number of bits to read</param>
		public void Read(out uint value, int bitCount = Bits.K_U_INT32_BIT_COUNT
			)
		{
			Contract.Requires(bitCount <= Bits.K_U_INT32_BIT_COUNT);

			value = this.ReadUInt32(bitCount);
		}
		/// <summary>Read an <see cref="System.Int32"/> from the stream</summary>
		/// <param name="value">value read from the stream</param>
		/// <param name="bitCount">Number of bits to read</param>
		/// <param name="signExtend">If true, the result will have the MSB extended</param>
		public void Read(out int value, int bitCount = Bits.K_INT32_BIT_COUNT
			, bool signExtend = false
			)
		{
			Contract.Requires(bitCount <= Bits.K_INT32_BIT_COUNT);

			value = this.ReadInt32(bitCount, signExtend);
		}
		/// <summary>Read an <see cref="System.UInt64"/> from the stream</summary>
		/// <param name="value">value read from the stream</param>
		/// <param name="bitCount">Number of bits to read</param>
		public void Read(out ulong value, int bitCount = Bits.K_U_INT64_BIT_COUNT
			)
		{
			Contract.Requires(bitCount <= Bits.K_U_INT64_BIT_COUNT);

			value = this.ReadUInt64(bitCount);
		}
		/// <summary>Read an <see cref="System.Int64"/> from the stream</summary>
		/// <param name="value">value read from the stream</param>
		/// <param name="bitCount">Number of bits to read</param>
		/// <param name="signExtend">If true, the result will have the MSB extended</param>
		public void Read(out long value, int bitCount = Bits.K_INT64_BIT_COUNT
			, bool signExtend = false
			)
		{
			Contract.Requires(bitCount <= Bits.K_INT64_BIT_COUNT);

			value = this.ReadInt64(bitCount, signExtend);
		}
	};
}
