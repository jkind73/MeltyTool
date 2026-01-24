using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	// TODO: change to a struct?
	public sealed class BDamageRatingOverride
		: IO.ITagElementStringNameStreamable
		, IEqualityComparer<BDamageRatingOverride>
	{
		#region Xml constants
		public static readonly Collections.BTypeValuesParams<BDamageRatingOverride> KBListParams = new
			Collections.BTypeValuesParams<BDamageRatingOverride>(db => db.DamageTypes);
		public static readonly XML.BTypeValuesXmlParams<BDamageRatingOverride> KBListXmlParams = new
			XML.BTypeValuesXmlParams<BDamageRatingOverride>("DamageRatingOverride", "type");
		#endregion

		float mRating_ = PhxUtil.K_INVALID_SINGLE;
		public float Rating { get { return this.mRating_; } }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamCursor(ref this.mRating_);
		}
		#endregion

		#region IEqualityComparer<BDamageRatingOverride> Members
		public bool Equals(BDamageRatingOverride x, BDamageRatingOverride y)
		{
			return x.Rating == y.Rating;
		}

		public int GetHashCode(BDamageRatingOverride obj)
		{
			return obj.Rating.GetHashCode();
		}
		#endregion
	};
}