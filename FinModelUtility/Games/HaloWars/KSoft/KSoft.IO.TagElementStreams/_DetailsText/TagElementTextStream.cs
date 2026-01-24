using Contracts = System.Diagnostics.Contracts;

namespace KSoft.IO
{
	/// <remarks><typeparamref name="TCursor"/> needs to implement <see cref="Text.ITextLineInfo">LineInfo</see></remarks>
	public abstract partial class TagElementTextStream<TDoc, TCursor> : TagElementStream<TDoc, TCursor, string>
		where TDoc : class
		where TCursor : class
	{
		/// <summary>Element's qualified name, or null if <see cref="Cursor"/> is null</summary>
		public abstract string CursorName { get; }

		#region GuidFormatString
		string mGuidFormatString_ = Values.KGuid.K_FORMAT_HYPHENATED;
		/// <summary>
		/// The formatting string for read/writing Guid values.
		/// Hyphenated is the default format. Setting this to NULL reverts the format to default
		/// </summary>
		public string GuidFormatString
		{
			get { return this.mGuidFormatString_; }
			set { this.mGuidFormatString_ = value ?? Values.KGuid.K_FORMAT_HYPHENATED; }
		}
		#endregion

		#region SingleFormatSpecifier
		string mSingleFormatSpecifier_ = Numbers.K_SINGLE_ROUND_TRIP_FORMAT_SPECIFIER;
		/// <summary>RoundTrip by default</summary>
		public string SingleFormatSpecifier
		{
			get { return this.mSingleFormatSpecifier_; }
			set { this.mSingleFormatSpecifier_ = value; }
		}
		#endregion

		#region DoubleFormatSpecifier
		string mDoubleFormatSpecifier_ = Numbers.K_DOUBLE_ROUND_TRIP_FORMAT_SPECIFIER;
		/// <summary>RoundTrip by default</summary>
		public string DoubleFormatSpecifier
		{
			get { return this.mDoubleFormatSpecifier_; }
			set { this.mDoubleFormatSpecifier_ = value; }
		}
		#endregion

		[Contracts.Pure]
		public override bool ValidateNameArg(string name) { return !string.IsNullOrEmpty(name); }

		protected TagElementTextStream()
		{
			this.mReadErrorState_ = new TextStreamReadErrorState(this);
		}

		public void UseDefaultFloatFormatSpecifiers()
		{
			this.SingleFormatSpecifier = Numbers.K_FLOAT_DEFAULT_FORMAT_SPECIFIER;
			this.DoubleFormatSpecifier = Numbers.K_FLOAT_DEFAULT_FORMAT_SPECIFIER;
		}
		public void UseRoundTripFloatFormatSpecifiers()
		{
			this.SingleFormatSpecifier = Numbers.K_SINGLE_ROUND_TRIP_FORMAT_SPECIFIER;
			this.DoubleFormatSpecifier = Numbers.K_DOUBLE_ROUND_TRIP_FORMAT_SPECIFIER;
		}
	};

	static partial class TagElementTextStreamUtils;
}