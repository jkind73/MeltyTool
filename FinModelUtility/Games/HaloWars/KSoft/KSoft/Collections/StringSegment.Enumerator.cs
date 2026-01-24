using System;
using System.Collections.Generic;

namespace KSoft.Collections
{
	partial struct StringSegment
	{
		[Serializable]
		public struct Enumerator
			: IEnumerator<char>
		{
			readonly string mData_;
			readonly int mStart_, mEnd_;
			int mCurrent_;

			public Enumerator(StringSegment segment)
			{
				this.mData_ = segment.mData_;
				this.mStart_ = segment.mOffset_;
				this.mEnd_ = this.mStart_ + segment.mCount_;
				this.mCurrent_ = this.mStart_ - 1;
			}
			public void Dispose() { }

			#region IEnumerator<char> Members
			public char Current { get {
				if (this.mCurrent_ < this.mStart_)	throw new InvalidOperationException("Enumeration has not started");
				if (this.mCurrent_ >= this.mEnd_)	throw new InvalidOperationException("Enumeration already finished");

				return this.mData_[this.mCurrent_];
			} }
			object System.Collections.IEnumerator.Current { get { return this.Current; } }

			public bool MoveNext()
			{
				if (this.mCurrent_ < this.mEnd_)
				{
					++this.mCurrent_;
					return this.mCurrent_ < this.mEnd_;
				}
				return false;
			}

			public void Reset()
			{
				this.mCurrent_ = this.mStart_ - 1;
			}
			#endregion
		};
	};
}