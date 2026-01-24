using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	/// <summary>Temporarily bookmarks a stream's <see cref="IKSoftStream.Owner"/></summary>
	[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct IkSoftStreamOwnerBookmark : IDisposable
	{
		IKSoftStream mStream_;
		readonly object mOldOwner_;

		/// <summary>Saves the stream's owner so a new one can be specified, but is then later restored to the previous owner, via <see cref="Dispose()"/></summary>
		/// <param name="stream">The underlying stream for this bookmark</param>
		/// <param name="newOwner"></param>
		public IkSoftStreamOwnerBookmark(IKSoftStream stream, object newOwner)
		{
			Contract.Requires(stream != null);

			this.mOldOwner_ = (this.mStream_ = stream).Owner;
			this.mStream_.Owner = newOwner;
		}

		/// <summary>Returns the owner of the underlying stream to the previous owner</summary>
		public void Dispose()
		{
			if (this.mStream_ != null)
			{
				this.mStream_.Owner = this.mOldOwner_;
				this.mStream_ = null;
			}
		}
	};

	/// <summary>Temporarily bookmarks a stream's <see cref="IKSoftStream.UserData"/></summary>
	[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct IkSoftStreamUserDataBookmark : IDisposable
	{
		IKSoftStream mStream_;
		readonly object mOldUserData_;

		/// <summary>Saves the stream's UserData so a new one can be specified, but is then later restored to the previous UserData, via <see cref="Dispose()"/></summary>
		/// <param name="stream">The underlying stream for this bookmark</param>
		/// <param name="newUserData"></param>
		public IkSoftStreamUserDataBookmark(IKSoftStream stream, object newUserData)
		{
			Contract.Requires(stream != null);

			this.mOldUserData_ = (this.mStream_ = stream).UserData;
			this.mStream_.UserData = newUserData;
		}

		/// <summary>Returns the UserData of the underlying stream to the previous UserData</summary>
		public void Dispose()
		{
			if (this.mStream_ != null)
			{
				this.mStream_.UserData = this.mOldUserData_;
				this.mStream_ = null;
			}
		}
	};

	/// <summary>Temporarily bookmarks a stream's <see cref="IKSoftStreamModeable.StreamMode"/></summary>
	[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct IkSoftStreamModeBookmark : IDisposable
	{
		IKSoftStreamModeable mStream_;
		readonly FileAccess mOldMode_;

		/// <summary>Saves the stream's StreamMode so a new one can be specified, but is then later restored to the previous StreamMode, via <see cref="Dispose()"/></summary>
		/// <param name="stream">The underlying stream for this bookmark</param>
		/// <param name="newMode"></param>
		public IkSoftStreamModeBookmark(IKSoftStreamModeable stream, FileAccess newMode)
		{
			Contract.Requires(stream != null);
			Contract.Requires(newMode != 0, "New mode is unset!");

			this.mOldMode_ = (this.mStream_ = stream).StreamMode;
			this.mStream_.StreamMode = newMode;

			if (this.mOldMode_ == newMode)
				this.mStream_ = null;
		}

		/// <summary>Returns the StreamMode of the underlying stream to the previous mode</summary>
		public void Dispose()
		{
			if (this.mStream_ != null)
			{
				this.mStream_.StreamMode = this.mOldMode_;
				this.mStream_ = null;
			}
		}
	};
}
