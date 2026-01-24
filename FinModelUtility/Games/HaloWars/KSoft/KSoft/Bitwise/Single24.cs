using System.Diagnostics.CodeAnalysis;

namespace KSoft.Bitwise
{
	[SuppressMessage("Microsoft.Design", "CA1823:AvoidUnusedPrivateFields")]
	public static class Single24
	{
		public const float MIN_VALUE = K_MIN_;
		public const float MAX_VALUE = K_MAX_;

		#region Single bit definitions
		static class Single32
		{
			internal const int K_MANTISSA_BIT_INDEX = 0;
			internal const int K_MANTISSA_BIT_COUNT = 23;
			// 0x007FFFFF, (1 << 23) - 1
			internal const uint K_MANTISSA_BIT_MASK = (1U<<K_MANTISSA_BIT_COUNT)-1;

			internal const int K_EXPONENT_BIT_INDEX = K_MANTISSA_BIT_INDEX + K_MANTISSA_BIT_COUNT;
			internal const int K_EXPONENT_BIT_COUNT = 8;
			// 0x7F800000, ((1 << 8) - 1) << 23
			internal const uint K_EXPONENT_BIT_MASK = ((1U<<K_EXPONENT_BIT_COUNT)-1) << K_EXPONENT_BIT_INDEX;

			internal const int K_SIGN_BIT_INDEX = K_EXPONENT_BIT_INDEX + K_EXPONENT_BIT_COUNT;
			internal const int K_SIGN_BIT_COUNT = 1;
			// 0x80000000, 1 << 31
			internal const uint K_SIGN_BIT_MASK = 1U << K_SIGN_BIT_INDEX;

			internal const uint K_SIGN_BIT = K_SIGN_BIT_MASK;

			internal const int K_EXPONENT_BIAS = (1 << (K_EXPONENT_BIT_COUNT-1)) - 1;
		};
		#endregion

		#region Single24 bit definitions
		const int K_MANTISSA_BIT_INDEX_ = 0;
		const int K_MANTISSA_BIT_COUNT_ = 17;
		// 0x0001FFFF, (1 << 17) - 1
		const uint K_MANTISSA_BIT_MASK_ = (1U << K_MANTISSA_BIT_COUNT_) - 1;

		const int K_EXPONENT_BIT_INDEX_ = K_MANTISSA_BIT_INDEX_ + K_MANTISSA_BIT_COUNT_;
		const int K_EXPONENT_BIT_COUNT_ = 6;
		// 0x007E0000, ((1 << 6) - 1) << 17
		const uint K_EXPONENT_BIT_MASK_ = ((1U << K_EXPONENT_BIT_COUNT_) - 1) << K_EXPONENT_BIT_INDEX_;

		const int K_SIGN_BIT_INDEX_ = K_EXPONENT_BIT_INDEX_ + K_EXPONENT_BIT_COUNT_;

	// 0x00800000, 1 << 23
	const uint K_SIGN_BIT_MASK_ = 1U << K_SIGN_BIT_INDEX_;

		const uint K_SIGN_BIT_ = K_SIGN_BIT_MASK_;

		const int K_EXPONENT_BIAS_ = (1 << (K_EXPONENT_BIT_COUNT_ - 1)) - 1;
		const int K_EXPONENT_BIAS_DIFF_ = Single32.K_EXPONENT_BIAS - K_EXPONENT_BIAS_;

		const int K_MANTISSA_BIT_DIFF_ = Single32.K_MANTISSA_BIT_COUNT - K_MANTISSA_BIT_COUNT_;
	#endregion

	#region Min\Max
	// min\max values for a signed single
	internal const uint K_MIN_INT = 0xFFFFFF;
		internal const uint K_MAX_INT = 0x7FFFFF;
		const float K_MIN_ = -8.589902E+09F;
		const float K_MAX_ = 8.589902E+09F;
		#endregion

		public static bool InRange(float value)
		{
			return value >= K_MIN_ && value <= K_MAX_;
		}

		public static uint FromSingle(float singleValue)
		{
			uint data = ByteSwap.SingleToUInt32(singleValue);
			uint mantissa = (data & Single32.K_MANTISSA_BIT_MASK) >> Single32.K_MANTISSA_BIT_INDEX;
			uint exponent = (data & Single32.K_EXPONENT_BIT_MASK) >> Single32.K_EXPONENT_BIT_INDEX;
			uint sign = (data & Single32.K_SIGN_BIT_MASK) >> Single32.K_SIGN_BIT_INDEX;
			uint v = 0;

			if (exponent == 0) v = K_SIGN_BIT_;
			else
			{
				sign = sign == 1 ? K_SIGN_BIT_ : 0U;

				exponent -= K_EXPONENT_BIAS_DIFF_;
				exponent <<= K_EXPONENT_BIT_INDEX_;

				mantissa >>= K_MANTISSA_BIT_DIFF_;
				mantissa <<= K_MANTISSA_BIT_INDEX_;

				v = exponent | mantissa;
				v |= sign;
			}

			return v;
		}
		public static float ToSingle(uint data)
		{
			uint mantissa = (data & K_MANTISSA_BIT_MASK_) >> K_MANTISSA_BIT_INDEX_;
			uint exponent = (data & K_EXPONENT_BIT_MASK_) >> K_EXPONENT_BIT_INDEX_;
			uint sign = (data & K_SIGN_BIT_MASK_) >> K_SIGN_BIT_INDEX_;
			uint v = 0;

			if (exponent == 0) v = Single32.K_SIGN_BIT;
			else
			{
				sign = sign == 1 ? Single32.K_SIGN_BIT : 0;
				exponent += K_EXPONENT_BIAS_DIFF_;
				exponent <<= K_EXPONENT_BIT_INDEX_;

				v = exponent | mantissa;
				v <<= K_MANTISSA_BIT_DIFF_;
				v |= sign;
			}

			return ByteSwap.SingleFromUInt32(v);
		}
	};
}
