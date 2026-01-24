
namespace KSoft.Bitwise
{
	public static class Int24
	{
		public const uint MAX_VALUE = K_DATA_BIT_MASK;

		#region Data\Number Access
		internal const int K_DATA_BIT_INDEX = 0;
		const int K_DATA_BIT_COUNT_ = 23;
		internal const uint K_DATA_BIT_MASK = (1 << K_DATA_BIT_COUNT_) - 1;

		public static uint GetNumber(uint data)
		{
			return data & K_DATA_BIT_MASK;
		}
		#endregion

		#region Sign bit access
		internal const int K_SIGN_BIT_INDEX = K_DATA_BIT_INDEX + K_DATA_BIT_COUNT_;
		const int K_SIGN_BIT_COUNT_ = 1;
		internal const int K_SIGN_BIT_MASK = K_SIGN_BIT_COUNT_ << K_SIGN_BIT_INDEX;

		public static bool IsSigned(uint data)
		{
			return Flags.Test(data, K_SIGN_BIT_MASK);
		}
		public static uint SetSigned(uint data, bool isSigned)
		{
			return Flags.Modify(isSigned, data, K_SIGN_BIT_MASK);
		}
		#endregion

		public static bool InRange(uint v)
		{
			return v <= K_DATA_BIT_MASK;
		}
	};
}
