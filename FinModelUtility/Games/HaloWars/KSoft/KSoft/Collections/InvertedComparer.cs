using System;
using System.Collections;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Collections
{
	public sealed class InvertedComparer : IComparer
	{
		readonly IComparer mComparer_;

		public InvertedComparer(IComparer comparer)
		{
			Contract.Requires<ArgumentNullException>(comparer != null);
			this.mComparer_ = comparer;
		}

		#region IComparer Members
		public int Compare(object x, object y)
		{
			return -this.mComparer_.Compare(x, y);
		}
		#endregion
	};

	public sealed class InvertedComparer<T> : IComparer<T>
	{
		readonly IComparer<T> mComparer_;

		public InvertedComparer(IComparer<T> comparer)
		{
			Contract.Requires<ArgumentNullException>(comparer != null);
			this.mComparer_ = comparer;
		}

		#region IComparer<T> Members
		public int Compare(T x, T y)
		{
			return -this.mComparer_.Compare(x, y);
		}
		#endregion
	};
}