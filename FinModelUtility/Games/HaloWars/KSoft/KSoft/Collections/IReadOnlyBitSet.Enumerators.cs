using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Collections
{
	public static partial class ReadOnlyBitSetEnumerators
	{
		public partial struct StateEnumerator
		{
			public StateEnumerator(IReadOnlyBitSet bitset)
				: this(bitset, false)
			{
				Contract.Requires<ArgumentNullException>(bitset != null);
			}

			public bool MoveNext()
			{
				if (this.mBitIndex_ < this.mLastIndex_)
				{
					this.mCurrent_ = this.mSet_.Get(++this.mBitIndex_);
					return true;
				}

				this.mBitIndex_ = this.mSet_.Length;
				return false;
			}
		};

		public partial struct StateFilterEnumerator
		{
			public StateFilterEnumerator(IReadOnlyBitSet bitset, bool stateFilter, int startBitIndex = 0)
				: this(bitset, false)
			{
				Contract.Requires<ArgumentNullException>(bitset != null);
				Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(startBitIndex < bitset.Length || bitset.Length == 0);

				this.mStateFilter_ = stateFilter;
				this.mStartBitIndex_ = startBitIndex-1;
			}

			public bool MoveNext()
			{
				if (this.mBitIndex_.IsNone())
					this.mBitIndex_ = this.mStartBitIndex_;

				if (this.mBitIndex_ < this.mLastIndex_)
				{
					this.mCurrent_ = this.mSet_.NextBitIndex(++this.mBitIndex_, this.mStateFilter_);

					if (this.mCurrent_ >= 0)
					{
						this.mBitIndex_ = this.mCurrent_;
						return true;
					}
				}

				this.mBitIndex_ = this.mSet_.Length;
				return false;
			}
		};
	};
}