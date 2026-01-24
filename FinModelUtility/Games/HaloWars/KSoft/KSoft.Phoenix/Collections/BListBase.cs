using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace KSoft.Collections
{
	public interface IBList
	{
		BListParams Params { get; }

		int Count { get; }

		void Clear();

		object GetObject(int id);
		object UnderlyingObjectsCollection { get; }

		bool IsEmpty { get; }

		void Sort();
	};

	/// <summary>Our base interface for lists of comparable objects</summary>
	/// <typeparam name="T">Comparable object's type</typeparam>
	public abstract class BListBase<T>
		: IBList
		, IEqualityComparer<BListBase<T>>
		, IEnumerable<T>
	{
		#region kValueEqualityComparer
		private static IEqualityComparer<T> gValueEqualityComparer_;
		protected static IEqualityComparer<T> KValueEqualityComparer { get {
			if (gValueEqualityComparer_ == null)
				gValueEqualityComparer_ = EqualityComparer<T>.Default;

			return gValueEqualityComparer_;
		} }
		#endregion

		#region kEqualityComparer
		protected sealed class EqualityComparer
			: IEqualityComparer<BListBase<T>>
		{
			#region IEqualityComparer<BListBase<T>> Members
			public bool Equals(BListBase<T> x, BListBase<T> y)
			{
				bool equals = x.Count == y.Count;
				if (equals)
				{
					var comparer = KValueEqualityComparer;
					for (int i = 0; i < x.Count && equals; i++)
						equals &= comparer.Equals(x[i], y[i]);
				}

				return equals;
			}

			public int GetHashCode(BListBase<T> obj)
			{
				int hash = 0;
				var comparer = KValueEqualityComparer;
				foreach (var o in obj)
					hash ^= comparer.GetHashCode(o);

				return hash;
			}
			#endregion
		};
		private static EqualityComparer gEqualityComparer_;
		protected static EqualityComparer KEqualityComparer { get {
			if (gEqualityComparer_ == null)
				gEqualityComparer_ = new EqualityComparer();

			return gEqualityComparer_;
		} }
		#endregion

		protected ObservableCollection<T> mList;
		protected List<T> RawList { get {
			var list = ObjectModel.Util.GetUnderlyingItemsAsList(this.mList);
			return list;
		} }

		/// <summary>Parameters that dictate the functionality of this list</summary>
		public BListParams Params { get; private set; }

		protected BListBase(int capacity = BCollectionParams.K_DEFAULT_CAPACITY)
		{
			this.mList = new ObservableCollection<T>(/*capacity*/);
			this.Capacity = capacity;
		}
		protected BListBase(BListParams @params)
			: this(@params != null ? @params.initialCapacity : BCollectionParams.K_DEFAULT_CAPACITY)
		{
			this.Params = @params;
		}

		#region List interface
		public int Count { get { return this.mList.Count; } }

		internal int Capacity
		{
			get { return this.RawList.Capacity; }
			set { this.RawList.Capacity = value; }
		}

		public virtual T this[int index]
		{
			get { return this.mList[index]; }
			set { this.mList[index] = value; }
		}

		internal void AddItem(T item)
		{
			this.mList.Add(item);
		}

		public virtual void Clear()
		{
			if (this.mList != null)
				this.mList.Clear();
		}

		#region IEnumerable<T> Members
		public List<T>.Enumerator GetEnumerator()
		{
			return this.RawList.GetEnumerator();
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return this.mList.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.mList.GetEnumerator();
		}
		#endregion
		#endregion

		public virtual object GetObject(int id)
		{
			return this[id];
		}

		object IBList.UnderlyingObjectsCollection { get { return this.mList; } }

		public bool IsEmpty { get { return this.Count == 0; } }
		internal void OptimizeStorage()
		{
			//if (Count == 0)
			//	mList = null;
			this.RawList.TrimExcess();
		}

		#region IEqualityComparer<BListBase<T>> Members
		public bool Equals(BListBase<T> x, BListBase<T> y)
		{
			return KEqualityComparer.Equals(x, y);
		}

		public int GetHashCode(BListBase<T> obj)
		{
			return KEqualityComparer.GetHashCode(obj);
		}
		#endregion

		public void Sort()
		{
			this.mList.Sort();
		}
		public void Sort(IComparer<T> comparer)
		{
			this.mList.Sort(comparer);
		}
		public void Sort(int index, int count, IComparer<T> comparer)
		{
			this.RawList.Sort(index, count, comparer);
		}
		public void Sort(Comparison<T> comparison)
		{
			this.mList.Sort(comparison);
		}
	};
}