
namespace KSoft.Phoenix
{
	static class SingleFixedPoint
	{
	const double K_SCALE_TO_SINGLE_MULTIPLIER_ = 0.0001;//System.Math.Pow(10, -kExponent);
		const double K_SCALE_FROM_SINGLE_MULTIPLIER_ = 10000;//System.Math.Pow(10, kExponent);

		const double K_MAX_ =   K_SCALE_TO_SINGLE_MULTIPLIER_ * (double) Bitwise.Int24.MAX_VALUE;
		const double K_MIN_ = -(K_SCALE_TO_SINGLE_MULTIPLIER_ * (double)(Bitwise.Int24.MAX_VALUE - 1));

		public static bool InRange(float value)
		{
			return value >= K_MIN_ && value <= K_MAX_;
		}

		public static float ToSingle(uint value)
		{
			bool isSigned = Bitwise.Int24.IsSigned(value);
			value = Bitwise.Int24.GetNumber(value);

			float single = (float)(K_SCALE_TO_SINGLE_MULTIPLIER_ * value);
			return isSigned ? -single : single;
		}
		public static uint FromSingle(float single)
		{
			bool isSigned = single < 0.0F;
			double d = System.Math.Abs(single);

			uint data = (uint)(d * K_SCALE_FROM_SINGLE_MULTIPLIER_);

			return Bitwise.Int24.SetSigned(data, isSigned);
		}
	};
}