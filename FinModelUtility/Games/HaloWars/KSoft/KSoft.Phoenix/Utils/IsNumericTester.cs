
namespace KSoft.Phoenix
{
	public struct IsNumericTester
	{
		public bool allowExponential;
		public int startOffset;

		public int returnFailOffset;
		public int returnIntegralDigits;
		public int returnFractionalDigits;
		public int returnSignificantDigits;

		private void InitializeReturns()
		{
			this.returnFailOffset = TypeExtensions.K_NONE;
			this.returnIntegralDigits = this.returnFractionalDigits = this.returnSignificantDigits = 0;
		}

		private enum Phase
		{
			// D is the Fortran-style exponent character
			// [whitespace] [sign] [digits] [.digits] [ {d | D | e | E}[sign]digits]
			//  0            1      2        34          4              5    6

			/** <summary>0</summary> */ WHITESPACE,
			/** <summary>1</summary> */ SIGN,
			/** <summary>2</summary> */ DIGITS,
			/** <summary>3</summary> */ FRACTIONAL_SIGN,
			/** <summary>4</summary> */ FRACTIONAL_DIGITS,
			/** <summary>5</summary> */ EXPONENT_SIGN,
			/** <summary>6</summary> */ EXPONENT_DIGITS,
			/** <summary>7</summary> */ TRAILING_WHITE_SPACE,

			K_NUMBER_OF
		};

		public bool Test(string str)
		{
			this.InitializeReturns();

			if (str == null)
				return this.TestFailed(0);

			bool foundDigits = false;

			bool foundFirstNonZeroDigit = false;
			var curPhase = Phase.WHITESPACE;

			for (int curPos = this.startOffset; curPos < str.Length; curPos++)
			{
				char c = str[curPos];

				bool cIsDigit = char.IsDigit(c);
				if (cIsDigit)
					foundDigits = true;

				for (bool nextChar = true; nextChar; )
				{
					nextChar = false;

					switch (curPhase)
					{
						case Phase.WHITESPACE: {
							if (IsIgnoredWhitespace(c))
								nextChar = true;
							else
								curPhase = Phase.SIGN;
						} break;

						case Phase.SIGN: {
							if (IsDigitSign(c))
								nextChar = true;

							curPhase = Phase.DIGITS;
						} break;

						case Phase.DIGITS: {
							if (cIsDigit)
							{
								this.returnIntegralDigits++;

								if (c != '0')
									foundFirstNonZeroDigit = true;

								if (foundFirstNonZeroDigit)
									this.returnSignificantDigits++;

								nextChar = true;
							}
							else
								curPhase = Phase.FRACTIONAL_SIGN;
						} break;

						case Phase.FRACTIONAL_SIGN: {
							if(c == '.')
							{
								nextChar = true;
								curPhase = Phase.FRACTIONAL_DIGITS;
							}
							else if (IsExponentCharacter(c))
							{
								if (!this.allowExponential)
									return this.TestFailed(curPos);

								nextChar = true;
								curPhase = Phase.EXPONENT_SIGN;
							}
							else
							{
								return this.TestFailed(curPos);
							}
						} break;

						case Phase.FRACTIONAL_DIGITS: {
							if (cIsDigit)
							{
								this.returnFractionalDigits++;

								if (c != '0')
									foundFirstNonZeroDigit = true;

								if (foundFirstNonZeroDigit)
									this.returnSignificantDigits++;

								nextChar = true;
							}
							else if (IsExponentCharacter(c))
							{
								if (!this.allowExponential)
									return this.TestFailed(curPos);

								nextChar = true;
								curPhase = Phase.EXPONENT_SIGN;
							}
							else if (IsIgnoredWhitespace(c))
							{
								nextChar = true;
								curPhase = Phase.TRAILING_WHITE_SPACE;
							}
							else
							{
								return this.TestFailed(curPos);
							}
						} break;

						case Phase.EXPONENT_SIGN: {
							if (IsDigitSign(c))
								nextChar = true;

							curPhase = Phase.EXPONENT_DIGITS;
						} break;

						case Phase.EXPONENT_DIGITS: {
							if (cIsDigit)
							{
								nextChar = true;
							}
							else if (IsIgnoredWhitespace(c))
							{
								nextChar = true;
								curPhase = Phase.TRAILING_WHITE_SPACE;
							}
							else
							{
								this.TestFailed(curPos);
							}
						} break;

						case Phase.TRAILING_WHITE_SPACE: {
							if (IsIgnoredWhitespace(c))
								nextChar = true;
							else
								return this.TestFailed(curPos);
						} break;
					}
				}
			}

			return foundDigits;
		}

		private bool TestFailed(int curPos)
		{
			this.returnFailOffset = curPos;
			return false;
		}

		private static bool IsIgnoredWhitespace(char c)
		{
			return c == ' ' || c == '\t';
		}
		private static bool IsDigitSign(char c)
		{
			return c == '-' || c == '+';
		}
		private static bool IsExponentCharacter(char c)
		{
			return
				c == 'e' || c == 'd' ||
				c == 'E' || c == 'D';
		}
	};
}