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
		/// <summary>Read an <see cref="System.Char"/> to the stream</summary>
		/// <param name="value">value to write to the stream</param>
		/// <param name="bitCount">Number of bits to write</param>
		public void Write(char value, int bitCount = Bits.K_CHAR_BIT_COUNT)
		{
			Contract.Requires(bitCount <= Bits.K_CHAR_BIT_COUNT);

			TWord word = (TWord)value;
			this.WriteWord(word, bitCount);
		}
		/// <summary>Read an <see cref="System.Byte"/> to the stream</summary>
		/// <param name="value">value to write to the stream</param>
		/// <param name="bitCount">Number of bits to write</param>
		public void Write(byte value, int bitCount = Bits.K_BYTE_BIT_COUNT)
		{
			Contract.Requires(bitCount <= Bits.K_BYTE_BIT_COUNT);

			TWord word = (TWord)value;
			this.WriteWord(word, bitCount);
		}
		/// <summary>Read an <see cref="System.SByte"/> to the stream</summary>
		/// <param name="value">value to write to the stream</param>
		/// <param name="bitCount">Number of bits to write</param>
		public void Write(sbyte value, int bitCount = Bits.K_S_BYTE_BIT_COUNT)
		{
			Contract.Requires(bitCount <= Bits.K_S_BYTE_BIT_COUNT);

			TWord word = (TWord)value;
			this.WriteWord(word, bitCount);
		}
		/// <summary>Read an <see cref="System.UInt16"/> to the stream</summary>
		/// <param name="value">value to write to the stream</param>
		/// <param name="bitCount">Number of bits to write</param>
		public void Write(ushort value, int bitCount = Bits.K_U_INT16_BIT_COUNT)
		{
			Contract.Requires(bitCount <= Bits.K_U_INT16_BIT_COUNT);

			TWord word = (TWord)value;
			this.WriteWord(word, bitCount);
		}
		/// <summary>Read an <see cref="System.Int16"/> to the stream</summary>
		/// <param name="value">value to write to the stream</param>
		/// <param name="bitCount">Number of bits to write</param>
		public void Write(short value, int bitCount = Bits.K_INT16_BIT_COUNT)
		{
			Contract.Requires(bitCount <= Bits.K_INT16_BIT_COUNT);

			TWord word = (TWord)value;
			this.WriteWord(word, bitCount);
		}
		/// <summary>Read an <see cref="System.UInt32"/> to the stream</summary>
		/// <param name="value">value to write to the stream</param>
		/// <param name="bitCount">Number of bits to write</param>
		public void Write(uint value, int bitCount = Bits.K_U_INT32_BIT_COUNT)
		{
			Contract.Requires(bitCount <= Bits.K_U_INT32_BIT_COUNT);

			TWord word = (TWord)value;
			this.WriteWord(word, bitCount);
		}
		/// <summary>Read an <see cref="System.Int32"/> to the stream</summary>
		/// <param name="value">value to write to the stream</param>
		/// <param name="bitCount">Number of bits to write</param>
		public void Write(int value, int bitCount = Bits.K_INT32_BIT_COUNT)
		{
			Contract.Requires(bitCount <= Bits.K_INT32_BIT_COUNT);

			TWord word = (TWord)value;
			this.WriteWord(word, bitCount);
		}
		/// <summary>Read an <see cref="System.UInt64"/> to the stream</summary>
		/// <param name="value">value to write to the stream</param>
		/// <param name="bitCount">Number of bits to write</param>
		public void Write(ulong value, int bitCount = Bits.K_U_INT64_BIT_COUNT)
		{
			Contract.Requires(bitCount <= Bits.K_U_INT64_BIT_COUNT);

			uint msbWord = (uint)(value >> Bits.K_INT32_BIT_COUNT);
			uint lsbWord = (uint)value;
			int msbBitCount = bitCount > Bits.K_INT32_BIT_COUNT ? bitCount - Bits.K_INT32_BIT_COUNT : 0;
			int lsbBitCount = bitCount > Bits.K_INT32_BIT_COUNT ? bitCount - msbBitCount : bitCount;

			if(msbBitCount > 0)
				this.WriteWord(msbWord, msbBitCount);
			this.WriteWord(lsbWord, lsbBitCount);
		}
		/// <summary>Read an <see cref="System.Int64"/> to the stream</summary>
		/// <param name="value">value to write to the stream</param>
		/// <param name="bitCount">Number of bits to write</param>
		public void Write(long value, int bitCount = Bits.K_INT64_BIT_COUNT)
		{
			Contract.Requires(bitCount <= Bits.K_INT64_BIT_COUNT);

			uint msbWord = (uint)(value >> Bits.K_INT32_BIT_COUNT);
			uint lsbWord = (uint)value;
			int msbBitCount = bitCount > Bits.K_INT32_BIT_COUNT ? bitCount - Bits.K_INT32_BIT_COUNT : 0;
			int lsbBitCount = bitCount > Bits.K_INT32_BIT_COUNT ? bitCount - msbBitCount : bitCount;

			if(msbBitCount > 0)
				this.WriteWord(msbWord, msbBitCount);
			this.WriteWord(lsbWord, lsbBitCount);
		}
	};
}