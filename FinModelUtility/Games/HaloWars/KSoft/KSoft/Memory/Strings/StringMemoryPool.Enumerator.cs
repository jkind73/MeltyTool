using System;
using System.Collections.Generic;

namespace KSoft.Memory.Strings
{
	partial class StringMemoryPool
	{
		/// <summary>
		/// Enumerates the strings in a <see cref="StringMemoryPool"/> in key-value pairs
		/// with the <b>address</b> being the <i>key</i> and the <b>string</b> being the <i>value</i>
		/// </summary>
		protected struct KeyValueEnumerator : IEnumerator<KeyValuePair<Values.PtrHandle, string>>
		{
			const int K_BLANK_INDEX_STATE_ = -2;

			StringMemoryPool mPool_;
			int mCurrentIndex_;

			public KeyValueEnumerator(StringMemoryPool pool)
			{
				this.mPool_ = pool;
				this.mCurrentIndex_ = K_BLANK_INDEX_STATE_;
				this.mCurrent_ = new KeyValuePair<Values.PtrHandle, string>(Values.PtrHandle.Null32, null);
			}

			#region IEnumerator<T> Members
			KeyValuePair<Values.PtrHandle, string> mCurrent_;
			/// <summary>Get the current element in the enumeration</summary>
			public KeyValuePair<Values.PtrHandle, string> Current { get { return this.mCurrent_; } }
			#endregion

			#region IDisposable Members
			void IDisposable.Dispose() { }
			#endregion

			#region IEnumerator Members
			/// <summary>Get the current element in the enumeration</summary>
			object System.Collections.IEnumerator.Current { get { return this.mCurrent_; } }

			/// <summary>Advances the enumerator to the next address\string pair</summary>
			/// <returns></returns>
			public bool MoveNext()
			{
				// for supporting state Resets
				if (this.mCurrentIndex_ == K_BLANK_INDEX_STATE_)
					this.mCurrentIndex_ = 0;

				if (this.mCurrentIndex_ >= 0 && this.mCurrentIndex_ < this.mPool_.Count)
				{
					this.mCurrent_ = new KeyValuePair<Values.PtrHandle, string>(this.mPool_.mReferences_[this.mCurrentIndex_], this.mPool_.mPool_[this.mCurrentIndex_]);

					this.mCurrentIndex_++;
				}
				// when we've past the end of the pool
				else
					this.mCurrentIndex_ = TypeExtensions.K_NONE;

				return this.mCurrentIndex_ >= 0 && this.mCurrentIndex_ < this.mPool_.Count;
			}

			public void Reset() {
				this.mCurrentIndex_ = K_BLANK_INDEX_STATE_; }
			#endregion
		};
	};
}