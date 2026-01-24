using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	partial class TagElementTextStreamUtils
	{
		enum ParseErrorType
		{
			NONE,
			/// <summary>Input string is null or empty</summary>
			NO_INPUT,
			/// <summary>The input can't be parsed as-is</summary>
			INVALID_VALUE,
		};
		static ParseErrorType ParseVerifyInput(string input)
		{
			return string.IsNullOrEmpty(input)
				? ParseErrorType.NO_INPUT
				: ParseErrorType.NONE;
		}
		static ParseErrorType ParseVerifyResult(ParseErrorType result, bool parseResult)
		{
			Util.MarkUnusedVariable(ref result);

			return parseResult
				? ParseErrorType.NONE
				: ParseErrorType.INVALID_VALUE;
		}

		/// <summary></summary>
		/// <param name="type"></param>
		/// <param name="noThrow">Does the caller want exceptions to be thrown on errors?</param>
		/// <param name="input"></param>
		/// <param name="errorState"></param>
		/// <returns>True if no error handling was needed. Else, an exception is throw (if allowed)</returns>
		static bool ParseHandleError(ParseErrorType type, bool noThrow, string input
			, TextStreamReadErrorState errorState)
		{
			Exception detailsException = null;
			switch (type)
			{
			case ParseErrorType.NO_INPUT:
				if (noThrow)
					return false;

				detailsException = new ArgumentException
					("Input null or empty", nameof(input));
				break;

			case ParseErrorType.INVALID_VALUE:
				detailsException = new ArgumentException(string.Format
					(Util.InvariantCultureInfo, "Couldn't parse \"{0}\"", input), nameof(input));
				break;

			default:
				return true;
			}

			if (noThrow == false)
				errorState.ThrowReadExeception(detailsException);

			errorState.LogReadExceptionWarning(detailsException);
			return true;
		}

		public static bool ParseString(string input, ref char value, bool noThrow
			, TextStreamReadErrorState errorState)
		{
			var result = ParseVerifyInput(input);
			if (result == ParseErrorType.NONE)
				value = input[0];

			return ParseHandleError(result, noThrow, input, errorState);
		}
		public static bool ParseString(string input, ref bool value, bool noThrow
			, TextStreamReadErrorState errorState)
		{
			var result = ParseVerifyInput(input);
			if (result == ParseErrorType.NONE)
				value = Text.Util.ParseBooleanLazy(input);

			return ParseHandleError(result, noThrow, input, errorState);
		}

		#region Real
		public static bool ParseString(string input, ref float value, bool noThrow
			, TextStreamReadErrorState errorState)
		{
			var result = ParseVerifyInput(input);
			if (result == ParseErrorType.NONE)
			{
				// #HACK HaloWars data has floats with C-based 'f' suffix
				char lastChar = input[input.Length - 1];
				if (lastChar == 'f' || lastChar == 'F')
					input = input.Substring(0, input.Length - 1);

				result = ParseVerifyResult(result, Numbers.FloatTryParseInvariant(input, out value));
			}

			return ParseHandleError(result, noThrow, input, errorState);
		}
		public static bool ParseString(string input, ref double value, bool noThrow
			, TextStreamReadErrorState errorState)
		{
			var result = ParseVerifyInput(input);
			if (result == ParseErrorType.NONE)
				result = ParseVerifyResult(result, Numbers.DoubleTryParseInvariant(input, out value));

			return ParseHandleError(result, noThrow, input, errorState);
		}
		#endregion
	};

	partial class TagElementTextStream<TDoc, TCursor>
	{
		#region Parse Util
		TextStreamReadErrorState mReadErrorState_;

		/// <summary>Sets the node that the current read operation is querying</summary>
		/// <remarks>It should be assumed that this isn't reset after the current read successfully finishes</remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
		protected object ReadErrorNode { set {
			// and in fact, this property can't be used to set mReadErrorNode to null with this assert.
			// TextStream implementations should only ever need to set the node and forget
			Contract.Assert(value is Text.ITextLineInfo);

			this.mReadErrorState_.LastReadLineInfo = (Text.ITextLineInfo)value;
		} }
		/// <summary>Throws a <see cref="Text.TextLineInfoException"/></summary>
		/// <param name="detailsException">The details (inner) exception of what went wrong</param>
		public sealed override void ThrowReadException(Exception detailsException) => this.mReadErrorState_.ThrowReadExeception(detailsException);

		public Text.TextLineInfo TryGetLastReadLineInfo() => this.IsReading && this.mReadErrorState_.LastReadLineInfo != null
				? new Text.TextLineInfo(this.mReadErrorState_.LastReadLineInfo)
				: Text.TextLineInfo.Empty;

		/// <summary>Argument value for noThrow to throw exceptions</summary>
		const bool K_THROW_EXCEPT_ = false;
		/// <summary>Argument value for noThrow to not throw exceptions</summary>
		const bool K_NO_EXCEPT_ = true;

		protected bool ReadEnumInternal<TEnum>(string enumString, ref TEnum enumValue)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			var result = TagElementStreamParseEnumUtil.Parse(this.IgnoreCaseOnEnums,
				enumString, ref enumValue);

			if (result == TagElementStreamParseEnumResult.FAILED_MEMBER_NOT_FOUND &&
			    this.ExceptionOnEnumParseFail)
			{
				this.ThrowReadException(new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
					                        "'{0}' is not a member of {1}",
					                        enumString, typeof(TEnum) )));
			}

			return result == TagElementStreamParseEnumResult.SUCCESS;
		}
		protected bool ReadEnumInternal<TEnum>(string enumString, ref int enumValue)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			var result = TagElementStreamParseEnumUtil.Parse<TEnum>(this.IgnoreCaseOnEnums,
				enumString, ref enumValue);

			if (result == TagElementStreamParseEnumResult.FAILED_MEMBER_NOT_FOUND &&
			    this.ExceptionOnEnumParseFail)
			{
				this.ThrowReadException(new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
					                        "'{0}' is not a member of {1}",
					                        enumString, typeof(TEnum))));
			}

			return result == TagElementStreamParseEnumResult.SUCCESS;
		}
		#endregion

		#region ReadElement impl
		protected abstract string GetInnerText(TCursor n);

		protected override void ReadElementEnum<TEnum>(TCursor n, ref TEnum enumValue) => this.ReadEnumInternal(this.GetInnerText(n), ref enumValue);
		protected override void ReadElementEnum<TEnum>(TCursor n, ref int enumValue) => this.ReadEnumInternal<TEnum>(this.GetInnerText(n), ref enumValue);

		protected override void ReadElement(TCursor n, ref Values.KGuid value) =>
			value = Values.KGuid.ParseExact(this.GetInnerText(n), this.mGuidFormatString_);
		#endregion

		/// <summary>Interpret the Name of <see cref="Cursor"/> as a member of <typeparamref name="TEnum"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="enumValue">value to receive the data</param>
		public override void ReadCursorName<TEnum>(ref TEnum enumValue) => this.ReadEnumInternal(this.CursorName, ref enumValue);

		#region ReadAttribute
		/// <summary>Streams out the attribute data of <paramref name="name"/></summary>
		/// <param name="name">Attribute name</param>
		/// <returns></returns>
		protected abstract string ReadAttribute(string name);

		public override void ReadAttributeEnum<TEnum>(string name, ref TEnum enumValue) => this.ReadEnumInternal(this.ReadAttribute(name), ref enumValue);
		public override void ReadAttributeEnum<TEnum>(string name, ref int enumValue) => this.ReadEnumInternal<TEnum>(this.ReadAttribute(name), ref enumValue);

		public override void ReadAttribute(string name, ref Values.KGuid value) =>
			value = Values.KGuid.ParseExact(this.ReadAttribute(name), this.mGuidFormatString_);
		#endregion

		#region ReadElementOpt
		/// <summary>Streams out the InnerText of element <paramref name="name"/></summary>
		/// <param name="name">Element name</param>
		/// <returns></returns>
		protected abstract string ReadElementOpt(string name);

		public override bool ReadElementEnumOpt<TEnum>(string name, ref TEnum enumValue)
		{
			string str = this.ReadElementOpt(name);
			return !string.IsNullOrEmpty(str)
				&&
				this.ReadEnumInternal(str, ref enumValue);
		}
		public override bool ReadElementEnumOpt<TEnum>(string name, ref int enumValue)
		{
			string str = this.ReadElementOpt(name);
			return !string.IsNullOrEmpty(str)
				&&
				this.ReadEnumInternal<TEnum>(str, ref enumValue);
		}

		public override bool ReadElementOpt(string name, ref Values.KGuid value)
		{
			string str = this.ReadElementOpt(name);
			return !string.IsNullOrEmpty(str) &&
				Values.KGuid.TryParseExactHyphenated(str, out value);
		}
		#endregion

		#region ReadAttributeOpt
		/// <summary>Streams out the attribute data of <paramref name="name"/></summary>
		/// <param name="name">Attribute name</param>
		/// <returns></returns>
		protected abstract string ReadAttributeOpt(string name);

		public override bool ReadAttributeEnumOpt<TEnum>(string name, ref TEnum enumValue)
		{
			string str = this.ReadAttributeOpt(name);
			return !string.IsNullOrEmpty(str)
				&&
				this.ReadEnumInternal(str, ref enumValue);
		}
		public override bool ReadAttributeEnumOpt<TEnum>(string name, ref int enumValue)
		{
			string str = this.ReadAttributeOpt(name);
			return !string.IsNullOrEmpty(str)
				&&
				this.ReadEnumInternal<TEnum>(str, ref enumValue);
		}

		public override bool ReadAttributeOpt(string name, ref Values.KGuid value)
		{
			string str = this.ReadAttributeOpt(name);
			return !string.IsNullOrEmpty(str) &&
				Values.KGuid.TryParseExactHyphenated(str, out value);
		}
		#endregion
	};
}
