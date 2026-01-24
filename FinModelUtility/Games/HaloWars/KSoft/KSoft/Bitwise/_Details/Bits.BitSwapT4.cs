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
		public static byte BitSwap(byte value, int startBitIndex = K_BYTE_BIT_COUNT-1)
		{
			Contract.Requires(startBitIndex > 0, K_BIT_SWAP_START_BIT_INDEX_NOT_GREATER_THAN_ZERO_);
			Contract.Requires(startBitIndex < K_BYTE_BIT_COUNT);

			if (value != 0 && value != byte.MaxValue)
			{
				uint bits = 0;
				const uint kOne = 1U;

				int bitsShift = 0;
				int valueShift = startBitIndex;
				for (var valueMask = kOne << startBitIndex;
					valueShift >= 0;
					valueMask >>= 1, valueShift--, bitsShift++)
					bits |= ((value & valueMask) >> valueShift) << bitsShift;

				value = (byte)bits;
			}
			return value;
		}

		[Contracts.Pure]
		public static ushort BitSwap(ushort value, int startBitIndex = K_INT16_BIT_COUNT-1)
		{
			Contract.Requires(startBitIndex > 0, K_BIT_SWAP_START_BIT_INDEX_NOT_GREATER_THAN_ZERO_);
			Contract.Requires(startBitIndex < K_INT16_BIT_COUNT);

			if (value != 0 && value != ushort.MaxValue)
			{
				uint bits = 0;
				const uint kOne = 1U;

				int bitsShift = 0;
				int valueShift = startBitIndex;
				for (var valueMask = kOne << startBitIndex;
					valueShift >= 0;
					valueMask >>= 1, valueShift--, bitsShift++)
					bits |= ((value & valueMask) >> valueShift) << bitsShift;

				value = (ushort)bits;
			}
			return value;
		}

		[Contracts.Pure]
		public static uint BitSwap(uint value, int startBitIndex = K_INT32_BIT_COUNT-1)
		{
			Contract.Requires(startBitIndex > 0, K_BIT_SWAP_START_BIT_INDEX_NOT_GREATER_THAN_ZERO_);
			Contract.Requires(startBitIndex < K_INT32_BIT_COUNT);

			if (value != 0 && value != uint.MaxValue)
			{
				uint bits = 0;
				const uint kOne = 1U;

				int bitsShift = 0;
				int valueShift = startBitIndex;
				for (var valueMask = kOne << startBitIndex;
					valueShift >= 0;
					valueMask >>= 1, valueShift--, bitsShift++)
					bits |= ((value & valueMask) >> valueShift) << bitsShift;

				value = (uint)bits;
			}
			return value;
		}

		[Contracts.Pure]
		public static ulong BitSwap(ulong value, int startBitIndex = K_INT64_BIT_COUNT-1)
		{
			Contract.Requires(startBitIndex > 0, K_BIT_SWAP_START_BIT_INDEX_NOT_GREATER_THAN_ZERO_);
			Contract.Requires(startBitIndex < K_INT64_BIT_COUNT);

			if (value != 0 && value != ulong.MaxValue)
			{
				ulong bits = 0;
				const ulong kOne = 1UL;

				int bitsShift = 0;
				int valueShift = startBitIndex;
				for (var valueMask = kOne << startBitIndex;
					valueShift >= 0;
					valueMask >>= 1, valueShift--, bitsShift++)
					bits |= ((value & valueMask) >> valueShift) << bitsShift;

				value = (ulong)bits;
			}
			return value;
		}

	};
}
