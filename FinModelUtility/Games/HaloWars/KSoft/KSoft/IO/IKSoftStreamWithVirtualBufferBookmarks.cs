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
	/// <summary>Forces the stream to seek to the end of the virtual buffer when disposed</summary>
	[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct IkSoftStreamWithVirtualBufferCleanup : IDisposable
	{
		IKSoftStreamWithVirtualBuffer mStream_;
		readonly long mBufferEnd_;

		public IkSoftStreamWithVirtualBufferCleanup(IKSoftStreamWithVirtualBuffer stream)
		{
			Contract.Requires(stream != null);
			Contract.Requires(stream.VirtualBufferStart > 0 && stream.VirtualBufferLength > 0);
			this.mStream_ = stream;
			this.mBufferEnd_ = stream.VirtualBufferStart + stream.VirtualBufferLength;
		}

		/// <summary>
		/// If the stream position is still inside the virtual buffer, seeks to the VirtualBuffer 'end'.
		/// Sets the VirtualBuffer properties of the underlying stream to 0.
		/// </summary>
		public void Dispose()
		{
			if (this.mStream_ != null)
			{
				long leftovers = this.mBufferEnd_ - this.mStream_.BaseStream.Position;
				if (leftovers > 0)
					this.mStream_.BaseStream.Seek(leftovers, SeekOrigin.Current);

				this.mStream_.VirtualBufferStart = this.mStream_.VirtualBufferLength = 0;
				this.mStream_ = null;
			}
		}
	};
	/// <summary>Temporarily bookmarks a stream's VirtualBuffer properties</summary>
	[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct IkSoftStreamWithVirtualBufferBookmark : IDisposable
	{
		IKSoftStreamWithVirtualBuffer mStream_;
		readonly long mOldStart_, mOldLength_;

		public IkSoftStreamWithVirtualBufferBookmark(IKSoftStreamWithVirtualBuffer stream)
		{
			Contract.Requires(stream != null);
			this.mStream_ = stream;
			this.mOldStart_ = stream.VirtualBufferStart;
			this.mOldLength_ = stream.VirtualBufferLength;
		}

		/// <summary>Restores the VirtualBuffer properties of the underlying stream to their previous values</summary>
		public void Dispose()
		{
			if (this.mStream_ != null)
			{
				this.mStream_.VirtualBufferStart = this.mOldStart_;
				this.mStream_.VirtualBufferLength = this.mOldLength_;
				this.mStream_ = null;
			}
		}
	};
	/// <summary>
	/// Temporarily bookmarks a stream's VirtualBuffer properties and sets up a new virtual buffer concept
	/// </summary>
	/// <see cref="IkSoftStreamWithVirtualBufferBookmark"/>
	/// <see cref="IkSoftStreamWithVirtualBufferCleanup"/>
	[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct IkSoftStreamWithVirtualBufferAndBookmark : IDisposable
	{
		IkSoftStreamWithVirtualBufferBookmark mBookmark_;
		IkSoftStreamWithVirtualBufferCleanup mCleanup_;
		bool mDisposed_;

		public IkSoftStreamWithVirtualBufferAndBookmark(IKSoftStreamWithVirtualBuffer stream, long bufferLength)
		{
			Contract.Requires(stream != null);
			this.mBookmark_ = stream.EnterVirtualBufferBookmark();
			this.mCleanup_ = stream.EnterVirtualBuffer(bufferLength);
			this.mDisposed_ = false;
		}

		public void Dispose()
		{
			if (!this.mDisposed_)
			{
				this.mCleanup_.Dispose();
				this.mBookmark_.Dispose();
				this.mDisposed_ = true;
			}
		}
	};
}
