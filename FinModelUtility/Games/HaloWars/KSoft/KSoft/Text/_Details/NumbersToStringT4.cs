using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using StringBuilder = System.Text.StringBuilder;

namespace KSoft
{
	partial class Numbers
	{
		#region UInt32
		static void ToStringBuilder(StringBuilder sb, uint value, int radix, int startIndex, string digits)
		{
			var radixInWord = (uint)radix;
			do {
				int digitIndex = (int)(value % radixInWord);
				sb.Insert(startIndex, digits[digitIndex]);
				value /= radixInWord;
			} while (value > 0);
		}
		// List<> has a(n in-place) Reverse method. StringBuilder doesn't. That's why.
		// We use additional memory (List<>.ToArrray allocates a new array) but have less computational complexity
		static void ToStringBuilder(List<char> sb, uint value, int radix, string digits)
		{
			int startIndex = sb.Count;

			var radixInWord = (uint)radix;
			do {
				int digitIndex = (int)(value % radixInWord);
				sb.Add(digits[digitIndex]);
				value /= radixInWord;
			} while (value > 0);

			sb.Reverse(startIndex, sb.Count-startIndex);
		}
		static string ToStringImpl(uint value, int radix, string digits)
		{
			var sb = new List<char>();
			ToStringBuilder(sb, value, radix, digits);

			return new string(sb.ToArray());
		}
		public static string ToString(uint value, int radix = K_BASE10, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);

			return ToStringImpl(value, radix, digits);
		}
		public static string ToString(uint value, NumeralBase radix = NumeralBase.DECIMAL, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			return ToStringImpl(value, (int)radix, digits);
		}
		public static StringBuilder ToStringBuilder(StringBuilder sb, uint value, NumeralBase radix = NumeralBase.DECIMAL, int startIndex = -1, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(sb != null);
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires(startIndex.IsNoneOrPositive());
			if(startIndex.IsNone())
				startIndex = sb.Length;

			ToStringBuilder(sb, value, (int)radix, startIndex, digits);
			return sb;
		}
		public static List<char> ToStringBuilder(List<char> sb, uint value, NumeralBase radix = NumeralBase.DECIMAL, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(sb != null);
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			ToStringBuilder(sb, value, (int)radix, digits);
			return sb;
		}
		#endregion
		#region Int32
		static void ToStringBuilder(StringBuilder sb, int value, int radix, int startIndex, string digits)
		{
			// Sign support only exist for decimal and lower bases
			if (radix <= K_BASE10 && value < 0)
			{
				sb.Append('-');
				++startIndex;
				value = -value; // change the value to positive
			}
			else if (radix > K_BASE10 && value < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, "Sign support only exist for decimal and lower bases");
			}

			var radixInWord = (int)radix;
			do {
				int digitIndex = (int)(value % radixInWord);
				sb.Insert(startIndex, digits[digitIndex]);
				value /= radixInWord;
			} while (value > 0);
		}
		// List<> has a(n in-place) Reverse method. StringBuilder doesn't. That's why.
		// We use additional memory (List<>.ToArrray allocates a new array) but have less computational complexity
		static void ToStringBuilder(List<char> sb, int value, int radix, string digits)
		{
			int startIndex = sb.Count;

			bool isSigned = false;
			// Sign support only exist for decimal and lower bases
			if (radix <= K_BASE10 && value < 0)
			{
				isSigned = true;
				value = -value; // change the value to positive
			}
			else if (radix > K_BASE10 && value < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, "Sign support only exist for decimal and lower bases");
			}

			var radixInWord = (int)radix;
			do {
				int digitIndex = (int)(value % radixInWord);
				sb.Add(digits[digitIndex]);
				value /= radixInWord;
			} while (value > 0);

			if (isSigned)
				sb.Add('-');

			sb.Reverse(startIndex, sb.Count-startIndex);
		}
		static string ToStringImpl(int value, int radix, string digits)
		{
			var sb = new List<char>();
			ToStringBuilder(sb, value, radix, digits);

			return new string(sb.ToArray());
		}
		public static string ToString(int value, int radix = K_BASE10, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);

			return ToStringImpl(value, radix, digits);
		}
		public static string ToString(int value, NumeralBase radix = NumeralBase.DECIMAL, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			return ToStringImpl(value, (int)radix, digits);
		}
		public static StringBuilder ToStringBuilder(StringBuilder sb, int value, NumeralBase radix = NumeralBase.DECIMAL, int startIndex = -1, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(sb != null);
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires(startIndex.IsNoneOrPositive());
			if(startIndex.IsNone())
				startIndex = sb.Length;

			ToStringBuilder(sb, value, (int)radix, startIndex, digits);
			return sb;
		}
		public static List<char> ToStringBuilder(List<char> sb, int value, NumeralBase radix = NumeralBase.DECIMAL, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(sb != null);
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			ToStringBuilder(sb, value, (int)radix, digits);
			return sb;
		}
		#endregion
		#region UInt64
		static void ToStringBuilder(StringBuilder sb, ulong value, int radix, int startIndex, string digits)
		{
			var radixInWord = (ulong)radix;
			do {
				int digitIndex = (int)(value % radixInWord);
				sb.Insert(startIndex, digits[digitIndex]);
				value /= radixInWord;
			} while (value > 0);
		}
		// List<> has a(n in-place) Reverse method. StringBuilder doesn't. That's why.
		// We use additional memory (List<>.ToArrray allocates a new array) but have less computational complexity
		static void ToStringBuilder(List<char> sb, ulong value, int radix, string digits)
		{
			int startIndex = sb.Count;

			var radixInWord = (ulong)radix;
			do {
				int digitIndex = (int)(value % radixInWord);
				sb.Add(digits[digitIndex]);
				value /= radixInWord;
			} while (value > 0);

			sb.Reverse(startIndex, sb.Count-startIndex);
		}
		static string ToStringImpl(ulong value, int radix, string digits)
		{
			var sb = new List<char>();
			ToStringBuilder(sb, value, radix, digits);

			return new string(sb.ToArray());
		}
		public static string ToString(ulong value, int radix = K_BASE10, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);

			return ToStringImpl(value, radix, digits);
		}
		public static string ToString(ulong value, NumeralBase radix = NumeralBase.DECIMAL, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			return ToStringImpl(value, (int)radix, digits);
		}
		public static StringBuilder ToStringBuilder(StringBuilder sb, ulong value, NumeralBase radix = NumeralBase.DECIMAL, int startIndex = -1, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(sb != null);
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires(startIndex.IsNoneOrPositive());
			if(startIndex.IsNone())
				startIndex = sb.Length;

			ToStringBuilder(sb, value, (int)radix, startIndex, digits);
			return sb;
		}
		public static List<char> ToStringBuilder(List<char> sb, ulong value, NumeralBase radix = NumeralBase.DECIMAL, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(sb != null);
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			ToStringBuilder(sb, value, (int)radix, digits);
			return sb;
		}
		#endregion
		#region Int64
		static void ToStringBuilder(StringBuilder sb, long value, int radix, int startIndex, string digits)
		{
			// Sign support only exist for decimal and lower bases
			if (radix <= K_BASE10 && value < 0)
			{
				sb.Append('-');
				++startIndex;
				value = -value; // change the value to positive
			}
			else if (radix > K_BASE10 && value < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, "Sign support only exist for decimal and lower bases");
			}

			var radixInWord = (long)radix;
			do {
				int digitIndex = (int)(value % radixInWord);
				sb.Insert(startIndex, digits[digitIndex]);
				value /= radixInWord;
			} while (value > 0);
		}
		// List<> has a(n in-place) Reverse method. StringBuilder doesn't. That's why.
		// We use additional memory (List<>.ToArrray allocates a new array) but have less computational complexity
		static void ToStringBuilder(List<char> sb, long value, int radix, string digits)
		{
			int startIndex = sb.Count;

			bool isSigned = false;
			// Sign support only exist for decimal and lower bases
			if (radix <= K_BASE10 && value < 0)
			{
				isSigned = true;
				value = -value; // change the value to positive
			}
			else if (radix > K_BASE10 && value < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, "Sign support only exist for decimal and lower bases");
			}

			var radixInWord = (long)radix;
			do {
				int digitIndex = (int)(value % radixInWord);
				sb.Add(digits[digitIndex]);
				value /= radixInWord;
			} while (value > 0);

			if (isSigned)
				sb.Add('-');

			sb.Reverse(startIndex, sb.Count-startIndex);
		}
		static string ToStringImpl(long value, int radix, string digits)
		{
			var sb = new List<char>();
			ToStringBuilder(sb, value, radix, digits);

			return new string(sb.ToArray());
		}
		public static string ToString(long value, int radix = K_BASE10, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);

			return ToStringImpl(value, radix, digits);
		}
		public static string ToString(long value, NumeralBase radix = NumeralBase.DECIMAL, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			return ToStringImpl(value, (int)radix, digits);
		}
		public static StringBuilder ToStringBuilder(StringBuilder sb, long value, NumeralBase radix = NumeralBase.DECIMAL, int startIndex = -1, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(sb != null);
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires(startIndex.IsNoneOrPositive());
			if(startIndex.IsNone())
				startIndex = sb.Length;

			ToStringBuilder(sb, value, (int)radix, startIndex, digits);
			return sb;
		}
		public static List<char> ToStringBuilder(List<char> sb, long value, NumeralBase radix = NumeralBase.DECIMAL, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(sb != null);
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			ToStringBuilder(sb, value, (int)radix, digits);
			return sb;
		}
		#endregion

		#region Byte
		public static string ToString(byte value, int radix = K_BASE10, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);

			return ToStringImpl(value, radix, digits);
		}
		public static string ToString(byte value, NumeralBase radix = NumeralBase.DECIMAL, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			return ToStringImpl(value, (int)radix, digits);
		}
		#endregion
		#region SByte
		public static string ToString(sbyte value, int radix = K_BASE10, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);

			return ToStringImpl(value, radix, digits);
		}
		public static string ToString(sbyte value, NumeralBase radix = NumeralBase.DECIMAL, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			return ToStringImpl(value, (int)radix, digits);
		}
		#endregion
		#region UInt16
		public static string ToString(ushort value, int radix = K_BASE10, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);

			return ToStringImpl(value, radix, digits);
		}
		public static string ToString(ushort value, NumeralBase radix = NumeralBase.DECIMAL, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			return ToStringImpl(value, (int)radix, digits);
		}
		#endregion
		#region Int16
		public static string ToString(short value, int radix = K_BASE10, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);

			return ToStringImpl(value, radix, digits);
		}
		public static string ToString(short value, NumeralBase radix = NumeralBase.DECIMAL, string digits = K_BASE_64DIGITS)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			return ToStringImpl(value, (int)radix, digits);
		}
		#endregion

		#region ToStringList UInt32
		public static string ToStringList(StringListDesc desc, IEnumerable<uint> values,
			Predicate<IEnumerable<uint>> writeTerminator = null)
		{
			Contract.Requires(!desc.requiresTerminator || writeTerminator != null);
			Contract.Ensures(Contract.Result<string>() != null);

			var chars = new List<char>();

			bool needsSeparator = false;
			int radix = (int)desc.radix;
			if (values != null)
			{
				foreach (var value in values)
				{
					if (needsSeparator)
						chars.Add(desc.separator);
					else
						needsSeparator = true;

					ToStringBuilder(chars, value, radix, desc.digits);
				}

				if (writeTerminator != null && writeTerminator(values))
					chars.Add(desc.terminator);
			}

			return new string(chars.ToArray());
		}
		#endregion
		#region ToStringList Int32
		public static string ToStringList(StringListDesc desc, IEnumerable<int> values,
			Predicate<IEnumerable<int>> writeTerminator = null)
		{
			Contract.Requires(!desc.requiresTerminator || writeTerminator != null);
			Contract.Ensures(Contract.Result<string>() != null);

			var chars = new List<char>();

			bool needsSeparator = false;
			int radix = (int)desc.radix;
			if (values != null)
			{
				foreach (var value in values)
				{
					if (needsSeparator)
						chars.Add(desc.separator);
					else
						needsSeparator = true;

					ToStringBuilder(chars, value, radix, desc.digits);
				}

				if (writeTerminator != null && writeTerminator(values))
					chars.Add(desc.terminator);
			}

			return new string(chars.ToArray());
		}
		#endregion
		#region ToStringList UInt64
		public static string ToStringList(StringListDesc desc, IEnumerable<ulong> values,
			Predicate<IEnumerable<ulong>> writeTerminator = null)
		{
			Contract.Requires(!desc.requiresTerminator || writeTerminator != null);
			Contract.Ensures(Contract.Result<string>() != null);

			var chars = new List<char>();

			bool needsSeparator = false;
			int radix = (int)desc.radix;
			if (values != null)
			{
				foreach (var value in values)
				{
					if (needsSeparator)
						chars.Add(desc.separator);
					else
						needsSeparator = true;

					ToStringBuilder(chars, value, radix, desc.digits);
				}

				if (writeTerminator != null && writeTerminator(values))
					chars.Add(desc.terminator);
			}

			return new string(chars.ToArray());
		}
		#endregion
		#region ToStringList Int64
		public static string ToStringList(StringListDesc desc, IEnumerable<long> values,
			Predicate<IEnumerable<long>> writeTerminator = null)
		{
			Contract.Requires(!desc.requiresTerminator || writeTerminator != null);
			Contract.Ensures(Contract.Result<string>() != null);

			var chars = new List<char>();

			bool needsSeparator = false;
			int radix = (int)desc.radix;
			if (values != null)
			{
				foreach (var value in values)
				{
					if (needsSeparator)
						chars.Add(desc.separator);
					else
						needsSeparator = true;

					ToStringBuilder(chars, value, radix, desc.digits);
				}

				if (writeTerminator != null && writeTerminator(values))
					chars.Add(desc.terminator);
			}

			return new string(chars.ToArray());
		}
		#endregion
	};
}
