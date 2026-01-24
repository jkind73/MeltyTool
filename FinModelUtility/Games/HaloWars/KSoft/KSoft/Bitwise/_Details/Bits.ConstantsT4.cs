using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft
{
	partial class Bits
	{
		#region kBitCount
		/// <summary>Number of bits in a <see cref="System.Char"/></summary>
		public const int K_CHAR_BIT_COUNT = sizeof(char) * 8;

		/// <summary>Number of bits in a <see cref="System.Byte"/></summary>
		public const int K_BYTE_BIT_COUNT = sizeof(byte) * 8;

		/// <summary>Number of bits in a <see cref="System.SByte"/></summary>
		public const int K_S_BYTE_BIT_COUNT = sizeof(sbyte) * 8;

		/// <summary>Number of bits in a <see cref="System.UInt16"/></summary>
		public const int K_U_INT16_BIT_COUNT = sizeof(ushort) * 8;

		/// <summary>Number of bits in a <see cref="System.Int16"/></summary>
		public const int K_INT16_BIT_COUNT = sizeof(short) * 8;

		/// <summary>Number of bits in a <see cref="System.UInt32"/></summary>
		public const int K_U_INT32_BIT_COUNT = sizeof(uint) * 8;

		/// <summary>Number of bits in a <see cref="System.Int32"/></summary>
		public const int K_INT32_BIT_COUNT = sizeof(int) * 8;

		/// <summary>Number of bits in a <see cref="System.UInt64"/></summary>
		public const int K_U_INT64_BIT_COUNT = sizeof(ulong) * 8;

		/// <summary>Number of bits in a <see cref="System.Int64"/></summary>
		public const int K_INT64_BIT_COUNT = sizeof(long) * 8;

		/// <summary>Number of bits in a <see cref="System.Single"/></summary>
		public const int K_SINGLE_BIT_COUNT = sizeof(float) * 8;

		/// <summary>Number of bits in a <see cref="System.Double"/></summary>
		public const int K_DOUBLE_BIT_COUNT = sizeof(double) * 8;

		#endregion

		#region kBitShift
		/// <summary>Bit shift value for getting the bit count of a an <see cref="System.Byte"/> element</summary>
		public const int K_BYTE_BIT_SHIFT =	3;

		/// <summary>Bit shift value for getting the bit count of a an <see cref="System.SByte"/> element</summary>
		public const int K_S_BYTE_BIT_SHIFT =	3;

		/// <summary>Bit shift value for getting the bit count of a an <see cref="System.UInt16"/> element</summary>
		public const int K_U_INT16_BIT_SHIFT =	4;

		/// <summary>Bit shift value for getting the bit count of a an <see cref="System.Int16"/> element</summary>
		public const int K_INT16_BIT_SHIFT =	4;

		/// <summary>Bit shift value for getting the bit count of a an <see cref="System.UInt32"/> element</summary>
		public const int K_U_INT32_BIT_SHIFT =	5;

		/// <summary>Bit shift value for getting the bit count of a an <see cref="System.Int32"/> element</summary>
		public const int K_INT32_BIT_SHIFT =	5;

		/// <summary>Bit shift value for getting the bit count of a an <see cref="System.UInt64"/> element</summary>
		public const int K_U_INT64_BIT_SHIFT =	6;

		/// <summary>Bit shift value for getting the bit count of a an <see cref="System.Int64"/> element</summary>
		public const int K_INT64_BIT_SHIFT =	6;

		/// <summary>Bit shift value for getting the bit count of a an <see cref="System.Single"/> element</summary>
		public const int K_SINGLE_BIT_SHIFT =	5;

		/// <summary>Bit shift value for getting the bit count of a an <see cref="System.Double"/> element</summary>
		public const int K_DOUBLE_BIT_SHIFT =	6;

		#endregion

		#region kBitMod
		/// <summary>Bitwise AND value for emulating modulus operations on <see cref="System.Byte"/> elements</summary>
		public const int K_BYTE_BIT_MOD = 7;

		/// <summary>Bitwise AND value for emulating modulus operations on <see cref="System.SByte"/> elements</summary>
		public const int K_S_BYTE_BIT_MOD = 7;

		/// <summary>Bitwise AND value for emulating modulus operations on <see cref="System.UInt16"/> elements</summary>
		public const int K_U_INT16_BIT_MOD = 15;

		/// <summary>Bitwise AND value for emulating modulus operations on <see cref="System.Int16"/> elements</summary>
		public const int K_INT16_BIT_MOD = 15;

		/// <summary>Bitwise AND value for emulating modulus operations on <see cref="System.UInt32"/> elements</summary>
		public const int K_U_INT32_BIT_MOD = 31;

		/// <summary>Bitwise AND value for emulating modulus operations on <see cref="System.Int32"/> elements</summary>
		public const int K_INT32_BIT_MOD = 31;

		/// <summary>Bitwise AND value for emulating modulus operations on <see cref="System.UInt64"/> elements</summary>
		public const int K_U_INT64_BIT_MOD = 63;

		/// <summary>Bitwise AND value for emulating modulus operations on <see cref="System.Int64"/> elements</summary>
		public const int K_INT64_BIT_MOD = 63;

		#endregion

		#region kBitmaskLookup
		/// <summary>Bit count to bit-mask look up table for 8-bit words</summary>
		public static readonly byte[] KBitmaskLookup8 = BitmaskByteLookUpTableGenerate(K_BYTE_BIT_COUNT);

		/// <summary>Bit count to bit-mask look up table for 16-bit words</summary>
		public static readonly ushort[] KBitmaskLookup16 = BitmaskUInt16LookUpTableGenerate(K_U_INT16_BIT_COUNT);

		/// <summary>Bit count to bit-mask look up table for 32-bit words</summary>
		public static readonly uint[] KBitmaskLookup32 = BitmaskUInt32LookUpTableGenerate(K_U_INT32_BIT_COUNT);

		/// <summary>Bit count to bit-mask look up table for 64-bit words</summary>
		public static readonly ulong[] KBitmaskLookup64 = BitmaskUInt64LookUpTableGenerate(K_U_INT64_BIT_COUNT);

		#endregion

		public static bool GetBitConstants(Type integerType,
			out int byteCount, out int bitCount, out int bitShift, out int bitMod)
		{
			Contract.Requires/*<ArgumentNullException>*/(integerType != null);

			byteCount = bitCount = bitShift = bitMod = TypeExtensions.K_NONE_INT32;

			switch(Type.GetTypeCode(integerType))
			{
				case TypeCode.Byte:
				case TypeCode.SByte:
					byteCount = sizeof(byte);
					bitCount = K_BYTE_BIT_COUNT;
					bitShift = K_BYTE_BIT_SHIFT;
					bitMod = K_BYTE_BIT_MOD;
					break;

				case TypeCode.UInt16:
				case TypeCode.Int16:
					byteCount = sizeof(ushort);
					bitCount = K_U_INT16_BIT_COUNT;
					bitShift = K_U_INT16_BIT_SHIFT;
					bitMod = K_U_INT16_BIT_MOD;
					break;

				case TypeCode.UInt32:
				case TypeCode.Int32:
					byteCount = sizeof(uint);
					bitCount = K_U_INT32_BIT_COUNT;
					bitShift = K_U_INT32_BIT_SHIFT;
					bitMod = K_U_INT32_BIT_MOD;
					break;

				case TypeCode.UInt64:
				case TypeCode.Int64:
					byteCount = sizeof(ulong);
					bitCount = K_U_INT64_BIT_COUNT;
					bitShift = K_U_INT64_BIT_SHIFT;
					bitMod = K_U_INT64_BIT_MOD;
					break;

				default: return false;
			}

			return true;
		}
	};
}
