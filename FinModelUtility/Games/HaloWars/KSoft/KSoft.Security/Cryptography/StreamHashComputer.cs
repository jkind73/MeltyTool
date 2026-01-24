using System;
using System.IO;
using System.Security.Cryptography;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Security.Cryptography
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct StreamHashComputer<T>
		where T : HashAlgorithm
	{
		/// <summary>Max size of the scratch buffer when we don't use a user specified preallocated buffer</summary>
		const int K_MAX_SCRATCH_BUFFER_SIZE_ = 0x1000;

		private readonly T mAlgo_;
		private readonly Stream mInputStream_;
		private byte[] mScratchBuffer_;
		private long mStartOffset_;
		private long mCount_;
		private readonly bool mRestorePosition_;

		public Stream InputStream { get { return this.mInputStream_; } }
		public long StartOffset { get { return this.mStartOffset_; } }
		public long Count { get { return this.mCount_; } }
		/// <summary>
		/// Does the input stream's current position get treated as the starting offset?
		/// </summary>
		public bool StartOffsetIsStreamPosition { get { return this.mStartOffset_.IsNone(); } }

		public StreamHashComputer(T algo, Stream inputStream
			, bool restorePosition = false
			, byte[] preallocatedBuffer = null)
		{
			Contract.Requires<ArgumentNullException>(inputStream != null);
			Contract.Requires<ArgumentException>(inputStream.CanSeek);
			Contract.Requires<ArgumentException>(preallocatedBuffer == null || preallocatedBuffer.Length > 0);

			this.mAlgo_ = algo;
			this.mInputStream_ = inputStream;
			this.mScratchBuffer_ = preallocatedBuffer;
			this.mStartOffset_ = TypeExtensions.K_NONE;
			this.mCount_ = TypeExtensions.K_NONE;
			this.mRestorePosition_ = restorePosition;

			this.mAlgo_.Initialize();
		}

		public void SetRangeAtCurrentOffset(long count)
		{
			Contract.Requires<ArgumentOutOfRangeException>(count >= 0);

			this.SetRangeAndOffset(TypeExtensions.K_NONE, count);
		}
		public void SetRangeAndOffset(long offset, long count)
		{
			Contract.Requires<ArgumentOutOfRangeException>(offset.IsNoneOrPositive());
			Contract.Requires<ArgumentOutOfRangeException>(count >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(offset.IsNone() || (offset+count) <= this.InputStream.Length);

			this.mStartOffset_ = offset;
			this.mCount_ = count;
		}

		public T Compute()
		{
			Contract.Requires<InvalidOperationException>(this.StartOffset.IsNoneOrPositive(),
				"You need to call SetRange before calling this");
			Contract.Requires<InvalidOperationException>(this.Count >= 0,
				"You need to call SetRange before calling this");

			#region prologue

			this.mAlgo_.Initialize();

			int bufferSize;
			byte[] buffer;

			bool usesPreallocatedBuffer = this.mScratchBuffer_ != null;
			if (!usesPreallocatedBuffer)
			{
				bufferSize = Math.Min((int) this.Count, K_MAX_SCRATCH_BUFFER_SIZE_);
				this.mScratchBuffer_ = new byte[bufferSize];
			}

			buffer = this.mScratchBuffer_;
			bufferSize = this.mScratchBuffer_.Length;

			long origPos = this.mInputStream_.Position;
			if (!this.StartOffsetIsStreamPosition && this.StartOffset != origPos)
				this.mInputStream_.Seek(this.StartOffset, SeekOrigin.Begin);
			#endregion

			for (long bytesRemaining = this.Count; bytesRemaining > 0;)
			{
				long numBytesToRead = Math.Min(bytesRemaining, bufferSize);
				int numBytesRead = 0;
				do
				{
					int n = this.mInputStream_.Read(buffer, numBytesRead, (int)numBytesToRead);
					if (n == 0)
						break;

					numBytesRead += n;
					numBytesToRead -= n;
				} while (numBytesToRead > 0);

				if (numBytesRead > 0)
					this.mAlgo_.TransformBlock(buffer, 0, numBytesRead, null, 0);
				else
					break;

				bytesRemaining -= numBytesRead;
			}

			this.mAlgo_.TransformFinalBlock(buffer, 0, 0); // yes, 0 bytes, all bytes should have been taken care of already

			#region epilogue
			if (this.mRestorePosition_)
				this.mInputStream_.Seek(origPos, SeekOrigin.Begin);

			if (!usesPreallocatedBuffer)
			{
				this.mScratchBuffer_ = null;
			}
			#endregion

			return this.mAlgo_;
		}
	};
}
