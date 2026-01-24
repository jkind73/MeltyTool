using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	public interface ICanThrowReadExceptionsWithExtraDetails
	{
		void ThrowReadExeception(Exception detailsException);
	};

	public sealed class TextStreamReadErrorState
		: Text.IHandleTextParseError
		, ICanThrowReadExceptionsWithExtraDetails
	{
		readonly IKSoftStream mStream_;
		Text.ITextLineInfo mReadLineInfo_;
		public Func<Exception> GetLineInfoException { get; private set; }

		public TextStreamReadErrorState(IKSoftStream textStream)
		{
			Contract.Requires<ArgumentNullException>(textStream != null);

			this.mStream_ = textStream;
			this.mReadLineInfo_ = null;
			this.GetLineInfoException = this.GetLineInfoExceptionInternal;
		}

		/// <summary>Line info of the last read that took place</summary>
		/// <remarks>Rather, about to take place. Should be set before a read with a possible error executes</remarks>
		public Text.ITextLineInfo LastReadLineInfo
		{
			get { return this.mReadLineInfo_; }
			set { this.mReadLineInfo_ = value; }
		}

		const string K_READ_LINE_INFO_IS_NULL_MSG_ =
			"A Text stream reader implementation failed to set the LastReadLineInfo before a read took place. " +
			"Guess what? Said read just failed";

		private Exception GetLineInfoExceptionInternal()
		{
			Contract.Assert(this.mReadLineInfo_ != null, K_READ_LINE_INFO_IS_NULL_MSG_);

			return new Text.TextLineInfoException(this.mReadLineInfo_, this.mStream_.StreamName);
		}

		private Text.TextLineInfoException GetReadException(Exception detailsException)
		{
			return new Text.TextLineInfoException(detailsException, this.mReadLineInfo_, this.mStream_.StreamName);
		}

		/// <summary>Throws a <see cref="Text.TextLineInfoException"/> using <see cref="LastReadLineInfo"/></summary>
		/// <param name="detailsException">The details (inner) exception of what went wrong</param>
		public void ThrowReadExeception(Exception detailsException)
		{
			Contract.Assert(this.mReadLineInfo_ != null, K_READ_LINE_INFO_IS_NULL_MSG_);

			throw this.GetReadException(detailsException);
		}

		public void LogReadExceptionWarning(Exception detailsException)
		{
			Contract.Assert(this.mReadLineInfo_ != null, K_READ_LINE_INFO_IS_NULL_MSG_);

			Debug.Trace.Io.TraceEvent(System.Diagnostics.TraceEventType.Warning, TypeExtensions.K_NONE,
				"Failed to parse tag value: {0}",
				this.GetReadException(detailsException));
		}
	};
}
