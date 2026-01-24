using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft
{
	// Getting matching results with radix <= 63:
	// http://www.pgregg.com/projects/php/base_conversion/base_conversion.php

	// http://bitplane.net/2010/08/java-float-fast-parser/
	// http://tinodidriksen.com/2011/05/28/cpp-convert-string-to-double-speed/

	public static partial class Numbers
	{
		enum ParseErrorType
		{
			NONE,
			/// <summary>Input string is null or empty</summary>
			NO_INPUT,
			/// <summary>The input can't be parsed as-is</summary>
			INVALID_VALUE,
			INVALID_START_INDEX,
		};

		public const int K_BASE10 = 10;
		public const int K_BASE36 = 36;
		public const int K_BASE64 = 64;
		public const string K_BASE_64DIGITS = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz+/";
		public const string K_BASE_64DIGITS_RFC4648 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

		[Contracts.Pure]
		static bool HandleParseError(ParseErrorType errorType, bool noThrow, string s, int startIndex
			, Text.IHandleTextParseError handler = null)
		{
			Exception detailsException = null;

			switch (errorType)
			{
			case ParseErrorType.NO_INPUT:
				if (noThrow)
					return false;

				detailsException = new ArgumentException
					("Input null or empty", nameof(s));
				break;

			case ParseErrorType.INVALID_VALUE:
				detailsException = new ArgumentException(string.Format
					(Util.InvariantCultureInfo, "Couldn't parse '{0}'", s), nameof(s));
				break;

			case ParseErrorType.INVALID_START_INDEX:
				detailsException = new ArgumentOutOfRangeException(nameof(s), string.Format
					(Util.InvariantCultureInfo, "'{0}' is out of range of the input length of '{1}'", startIndex, s.Length));
				break;

			default:
				return true;
			}

			if (handler == null)
				handler = Text.Util.DefaultTextParseErrorHandler;

			if (noThrow == false)
				handler.ThrowReadExeception(detailsException);

			handler.LogReadExceptionWarning(detailsException);
			return true;
		}

		[Contracts.Pure]
		public static bool IsValidLookupTable(NumeralBase radix, string digits)
		{
			return radix >= NumeralBase.BINARY && (int)radix <= digits.Length;
		}
		[Contracts.Pure]
		public static bool IsValidLookupTable(NumbersRadix radix, string digits)
		{
			return radix >= NumbersRadix.BINARY && (int)radix <= digits.Length;
		}


		[SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
		public struct StringListDesc
		{
			public const char K_DEFAULT_SEPARATOR = ',';
			public const char K_DEFAULT_TERMINATOR = ';';
			public static StringListDesc Default { get => new StringListDesc(K_DEFAULT_SEPARATOR); }

			public string digits;
			public NumbersRadix radix;
			/// <remarks><b>false</b> by default</remarks>
			public bool requiresTerminator;

			public char separator;
			public char terminator;

			public StringListDesc(char separator, char terminator = K_DEFAULT_TERMINATOR,
				NumbersRadix radix = NumbersRadix.DECIMAL, string digits = K_BASE_64DIGITS)
			{
				Contract.Requires(!string.IsNullOrEmpty(digits));
				Contract.Requires(IsValidLookupTable(radix, digits));

				this.digits = digits;
				this.radix = radix;
				this.requiresTerminator = false;

				this.separator = separator;
				this.terminator = terminator;
			}

			[Contracts.Pure]
			internal int PredictedCount(string values)
			{
				Contract.Assume(values != null);

				int count = 1;

				// using StringSegment and its Enumerator won't allocate any reference types
				var sseg = new Collections.StringSegment(values);
				foreach (char c in sseg)
				{
					if (c == this.separator)
						count++;
					else if (c == this.terminator)
						break;
				}

				return count;
			}
		};

		// #REVIEW: IsWhiteSpace can be rather expensive, and it is used in TryParseImpl. Perhaps we can make a variant
		// that can safely assume all characters are non-ws, and have TryParseList impls call it instead?
		// The TryParse() below would need to be updated to catch trailing ws

		// #REVIEW: add an option to just flat out skip unsuccessful items?

		abstract class TryParseNumberListBase<T, TListItem>
			where T : struct
		{
			protected readonly StringListDesc mDesc;
			protected readonly string mValues;
			protected List<TListItem> mList;

			protected TryParseNumberListBase(StringListDesc desc, string values)
			{
				this.mDesc = desc;
				this.mValues = values;
			}

			protected abstract IEnumerable<T?> EmptyResult { get; }

			void InitializeList()
			{
				// ReSharper disable once ImpureMethodCallOnReadonlyValueField - yes IT IS fucking Pure you POS
				int predicatedCount = this.mDesc.PredictedCount(this.mValues);
				this.mList = new List<TListItem>(predicatedCount);
			}

			protected abstract TListItem CreateItem(int start, int length);

			protected abstract IEnumerable<T?> CreateResult();

			public IEnumerable<T?> TryParse()
			{
				if (this.mValues == null)
					return this.EmptyResult;

				this.InitializeList();

				bool foundTerminator = false;
				int valueLength = this.mValues.Length;
				for (int start = 0; !foundTerminator && start < valueLength; )
				{
					// Skip any starting whitespace
					while (start < valueLength && char.IsWhiteSpace(this.mValues[start]))
						++start;

					int end = start;
					int length = 0;
					while (end < valueLength)
					{
						char c = this.mValues[end];
						foundTerminator = c == this.mDesc.terminator;
						// NOTE: TryParseImpl actually handles leading and trailing whitespace
						if (c == this.mDesc.separator || foundTerminator)
							break;

						// NOTE: we wouldn't want to update length if we hit ws before the separator and the TryParseImpl assumes no ws
						++length;
						++end;
					}

					if (length > 0)
						this.mList.Add(this.CreateItem(start, length));

					start = end + 1;
				}

				// #REVIEW: should we add support for throwing an exception or such when a terminator isn't encountered?

				return this.mList.Count == 0
					? this.EmptyResult
					: this.CreateResult();
			}
		};

		// Single.ToString(string): "if format is null or an empty string, the return value for this isntance is formatted with the general numeric format specifier ("G")
		public const string K_FLOAT_DEFAULT_FORMAT_SPECIFIER = null;
		public const string K_FLOAT_ROUND_TRIP_FORMAT_SPECIFIER = "G9";
		public const string K_SINGLE_ROUND_TRIP_FORMAT_SPECIFIER = K_FLOAT_ROUND_TRIP_FORMAT_SPECIFIER;
		public const string K_DOUBLE_ROUND_TRIP_FORMAT_SPECIFIER = "G17";

		// based on the reference source, this is what the default number styles are
		public const NumberStyles K_FLOAT_TRY_PARSE_NUMBER_STYLES = 0
			| NumberStyles.Float
			| NumberStyles.AllowThousands;
		public static bool FloatTryParseInvariant(string s, out float result)
		{
			return float.TryParse(s, K_FLOAT_TRY_PARSE_NUMBER_STYLES, CultureInfo.InvariantCulture, out result);
		}
		public static float FloatParseInvariant(string s)
		{
			return float.Parse(s, K_FLOAT_TRY_PARSE_NUMBER_STYLES, CultureInfo.InvariantCulture);
		}

		// based on the reference source, this is what the default number styles are
		public const NumberStyles K_DOUBLE_TRY_PARSE_NUMBER_STYLES = K_FLOAT_TRY_PARSE_NUMBER_STYLES;
		public static bool DoubleTryParseInvariant(string s, out double result)
		{
			return double.TryParse(s, K_DOUBLE_TRY_PARSE_NUMBER_STYLES, CultureInfo.InvariantCulture, out result);
		}
		public static double DoubleParseInvariant(string s)
		{
			return double.Parse(s, K_DOUBLE_TRY_PARSE_NUMBER_STYLES, CultureInfo.InvariantCulture);
		}
	};
};
