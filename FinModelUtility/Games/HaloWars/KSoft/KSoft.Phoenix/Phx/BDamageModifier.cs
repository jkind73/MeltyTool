using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public sealed class BWeaponModifier
		: IO.ITagElementStringNameStreamable
		, IComparable<BWeaponModifier>
		, IEqualityComparer<BWeaponModifier>
	{
		#region Xml constants
		public static readonly Collections.BTypeValuesParams<BWeaponModifier> KBListParams = new
			Collections.BTypeValuesParams<BWeaponModifier>(db => db.DamageTypes);
		public static readonly XML.BTypeValuesXmlParams<BWeaponModifier> KBListXmlParams = new
			XML.BTypeValuesXmlParams<BWeaponModifier>("DamageModifier", "type");
		#endregion

		#region Rating
		float mRating_ = 1.0f;
		public float Rating
		{
			get { return this.mRating_; }
			set { this.mRating_ = value; }
		}
		#endregion

		#region DamagePercentage
		float mDamagePercentage_ = 1.0f;
		public float DamagePercentage
		{
			get { return this.mDamagePercentage_; }
		}
		#endregion

		#region ReflectDamageFactor
		float mReflectDamageFactor_;
		public float ReflectDamageFactor
		{
			get { return this.mReflectDamageFactor_; }
			set { this.mReflectDamageFactor_ = value; }
		}
		#endregion

		#region Bowlable
		bool mBowlable_;
		public bool Bowlable
		{
			get { return this.mBowlable_; }
			set { this.mBowlable_ = value; }
		}
		#endregion

		#region Rammable
		bool mRammable_;
		public bool Rammable
		{
			get { return this.mRammable_; }
			set { this.mRammable_ = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOpt("rating", ref this.mRating_, PhxPredicates.IsNotOne);
			s.StreamCursor(ref this.mDamagePercentage_);
			s.StreamAttributeOpt("reflectDamageFactor", ref this.mReflectDamageFactor_, Predicates.IsNotZero);
			s.StreamAttributeOpt("bowlable", ref this.mBowlable_, Predicates.IsTrue);
			s.StreamAttributeOpt("rammable", ref this.mRammable_, Predicates.IsTrue);
		}
		#endregion

		#region IComparable<BDamageModifier> Members
		public int CompareTo(BWeaponModifier other)
		{
			if (this.Rating != other.Rating)
				return this.Rating.CompareTo(other.Rating);

			return this.DamagePercentage.CompareTo(other.DamagePercentage);
		}
		#endregion

		#region IEqualityComparer<BDamageModifier> Members
		public bool Equals(BWeaponModifier x, BWeaponModifier y)
		{
			return x.Rating == y.Rating
				&& x.DamagePercentage == y.DamagePercentage;
		}

		public int GetHashCode(BWeaponModifier obj)
		{
			return obj.Rating.GetHashCode() ^ obj.DamagePercentage.GetHashCode();
		}
		#endregion
	};
}