using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	using BCost = Collections.BTypeValuesSingle;
	using BProtoObjectVeterancyList = Collections.BListExplicitIndex<BProtoObjectVeterancy>;

	partial class BDatabaseBase
	{
		HashSet<BCost> mPoolCosts_;
		//HashSet<BPops> mPoolPops;
		HashSet<BProtoObjectVeterancyList> mPoolVeterancies_;

	public bool InternTypeValues<T>(ref Collections.BTypeValuesBase<T> values)
		{
			return false;
		}
	};
}
