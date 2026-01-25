using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public struct BPopulation
		: IO.ITagElementStringNameStreamable
		, IComparable<BPopulation>
		, IEqualityComparer<BPopulation>
	{
		sealed class EqualityComparer_ : IEqualityComparer<BPopulation>
		{
			#region IEqualityComparer<BPopulation> Members
			public bool Equals(BPopulation x, BPopulation y)
			{
				return x.Max == y.Max && x.Count == y.Count;
			}

			public int GetHashCode(BPopulation obj)
			{
				return obj.Max.GetHashCode() ^ obj.Count.GetHashCode();
			}
			#endregion
		};
		private static EqualityComparer_ gEqualityComparer_;

        public static IEqualityComparer<BPopulation> EqualityComparer {
          get {
            if (gEqualityComparer_ == null)
              gEqualityComparer_ = new EqualityComparer_();

            return gEqualityComparer_;
          }
        }

		#region Xml constants
		public static readonly Collections.BTypeValuesParams<BPopulation> KBListParams = new
			Collections.BTypeValuesParams<BPopulation>(db => db.GameData.Populations)
			{
				kTypeGetInvalid = () => KInvalid
			};
		public static readonly XML.BTypeValuesXmlParams<BPopulation> KBListXmlParams = new
			XML.BTypeValuesXmlParams<BPopulation>("Pop", "Type");

		public static readonly Collections.BTypeValuesParams<float> KBListParamsSingle = new
			Collections.BTypeValuesParams<float>(db => db.GameData.Populations)
			{
				kTypeGetInvalid = PhxUtil.KGetInvalidSingle
			};
		public static readonly XML.BTypeValuesXmlParams<float> KBListXmlParamsSingle = new
			XML.BTypeValuesXmlParams<float>("Pop", "Type");
		public static readonly XML.BTypeValuesXmlParams<float> KBListXmlParamsSingleLowerCase = new
			XML.BTypeValuesXmlParams<float>("Pop", "Type".ToLowerInvariant());
		public static readonly XML.BTypeValuesXmlParams<float> KBListXmlParamsSingleCapAddition = new
			XML.BTypeValuesXmlParams<float>("PopCapAddition", "Type");
		#endregion

		private static BPopulation KInvalid { get { return new BPopulation(PhxUtil.K_INVALID_SINGLE, PhxUtil.K_INVALID_SINGLE); } }

		float mMax_;
		public float Max { get { return this.mMax_; } }

		float mCount_;
		public float Count { get { return this.mCount_; } }

		BPopulation(float max, float count) {
			this.mMax_ = max;
			this.mCount_ = count; }

		#region IComparable<T> Members
		int IComparable<BPopulation>.CompareTo(BPopulation other)
		{
			if (this.Max == other.Max)
				return this.Count.CompareTo(other.Count);
			else
				return this.Max.CompareTo(other.Max);
		}
		#endregion

		#region IEqualityComparer<BPopulation> Members
		public bool Equals(BPopulation x, BPopulation y)
		{
			return EqualityComparer.Equals(x, y);
		}

		public int GetHashCode(BPopulation obj)
		{
			return EqualityComparer.GetHashCode(obj);
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("Max", ref this.mMax_);
			s.StreamCursor(ref this.mCount_);
		}
		#endregion
	};
}