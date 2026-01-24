using System;

namespace KSoft.Collections
{
	/// <summary>Various flags for <see cref="BCollectionParams"/></summary>
	[Flags]
	public enum BCollectionParamsFlags
	{
		TO_LOWER_DATA_NAMES,

		K_NUMBER_OF
	};

	public abstract class BCollectionParams
	{
		public const int K_DEFAULT_CAPACITY = 16;

		/// <summary>For fine tuning the BDictionary initialization, to avoid reallocations</summary>
		public /*readonly*/ int initialCapacity = K_DEFAULT_CAPACITY;

		#region Flags
		/// <summary>BCollectionParamsFlags</summary>
		public BitVector32 flags;

		public bool ToLowerDataNames
		{
			get { return this.flags.Test(BCollectionParamsFlags.TO_LOWER_DATA_NAMES); }
			set { this.flags.Set(BCollectionParamsFlags.TO_LOWER_DATA_NAMES, value); }
		}
		#endregion

		protected BCollectionParams() {}
	};
}