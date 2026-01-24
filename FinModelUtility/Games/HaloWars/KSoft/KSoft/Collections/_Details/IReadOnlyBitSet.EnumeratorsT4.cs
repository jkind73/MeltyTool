using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace KSoft.Collections
{
	static partial class ReadOnlyBitSetEnumerators
	{
		[Serializable]
		partial struct StateEnumerator
			: IEnumerator< bool >
		{
			readonly IReadOnlyBitSet mSet_;
			readonly int mLastIndex_;
			readonly int mVersion_;
			int mBitIndex_;
			bool mCurrent_;

			StateEnumerator(IReadOnlyBitSet bitset,
				[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
				bool dummy)
				: this()
			{
				this.mSet_ = bitset;
				this.mLastIndex_ = bitset.Length - 1;
				this.mVersion_ = bitset.Version;
				this.mBitIndex_ = TypeExtensions.K_NONE;
			}

			void VerifyVersion()
			{
				if (this.mVersion_ != this.mSet_.Version)
					throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}

			public bool Current { get {
				if (this.mBitIndex_.IsNone())			throw new InvalidOperationException("Enumeration has not started");
				if (this.mBitIndex_ > this.mLastIndex_)		throw new InvalidOperationException("Enumeration already finished");

				return this.mCurrent_;
			} }
			object System.Collections.IEnumerator.Current { get => this.Current; }

			public void Reset()
			{
				this.VerifyVersion();
				this.mBitIndex_ = TypeExtensions.K_NONE;
			}

			public void Dispose()	{ }
		};

		[Serializable]
		partial struct StateFilterEnumerator
			: IEnumerator< int >
		{
			readonly IReadOnlyBitSet mSet_;
			readonly int mLastIndex_;
			readonly int mVersion_;
			int mBitIndex_;
			int mCurrent_;
			// defined here to avoid: CS0282: There is no defined ordering between fields in multiple declarations of partial class or struct
			readonly bool mStateFilter_;
			readonly int mStartBitIndex_;

			StateFilterEnumerator(IReadOnlyBitSet bitset,
				[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
				bool dummy)
				: this()
			{
				this.mSet_ = bitset;
				this.mLastIndex_ = bitset.Length - 1;
				this.mVersion_ = bitset.Version;
				this.mBitIndex_ = TypeExtensions.K_NONE;
			}

			void VerifyVersion()
			{
				if (this.mVersion_ != this.mSet_.Version)
					throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}

			public int Current { get {
				if (this.mBitIndex_.IsNone())			throw new InvalidOperationException("Enumeration has not started");
				if (this.mBitIndex_ > this.mLastIndex_)		throw new InvalidOperationException("Enumeration already finished");

				return this.mCurrent_;
			} }
			object System.Collections.IEnumerator.Current { get => this.Current; }

			public void Reset()
			{
				this.VerifyVersion();
				this.mBitIndex_ = TypeExtensions.K_NONE;
			}

			public void Dispose()	{ }
		};

	};
}
