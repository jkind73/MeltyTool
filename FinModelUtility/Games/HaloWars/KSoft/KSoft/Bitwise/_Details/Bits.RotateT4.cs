using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft
{
	partial class Bits
	{
		[Contracts.Pure]
		public static byte RotateLeft(byte x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < K_BYTE_BIT_COUNT);

			return (byte)( (x << shift) | (x >> (K_BYTE_BIT_COUNT - shift)) );
		}
		[Contracts.Pure]
		public static byte RotateRight(byte x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < K_BYTE_BIT_COUNT);

			return (byte)( (x >> shift) | (x << (K_BYTE_BIT_COUNT - shift)) );
		}

		[Contracts.Pure]
		public static ushort RotateLeft(ushort x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < K_INT16_BIT_COUNT);

			return (ushort)( (x << shift) | (x >> (K_INT16_BIT_COUNT - shift)) );
		}
		[Contracts.Pure]
		public static ushort RotateRight(ushort x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < K_INT16_BIT_COUNT);

			return (ushort)( (x >> shift) | (x << (K_INT16_BIT_COUNT - shift)) );
		}

		[Contracts.Pure]
		public static uint RotateLeft(uint x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < K_INT32_BIT_COUNT);

			return (uint)( (x << shift) | (x >> (K_INT32_BIT_COUNT - shift)) );
		}
		[Contracts.Pure]
		public static uint RotateRight(uint x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < K_INT32_BIT_COUNT);

			return (uint)( (x >> shift) | (x << (K_INT32_BIT_COUNT - shift)) );
		}

		[Contracts.Pure]
		public static ulong RotateLeft(ulong x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < K_INT64_BIT_COUNT);

			return (ulong)( (x << shift) | (x >> (K_INT64_BIT_COUNT - shift)) );
		}
		[Contracts.Pure]
		public static ulong RotateRight(ulong x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < K_INT64_BIT_COUNT);

			return (ulong)( (x >> shift) | (x << (K_INT64_BIT_COUNT - shift)) );
		}

	};
}
