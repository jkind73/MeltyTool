using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoObjectVeterancy
		: IO.ITagElementStringNameStreamable
		, IComparable<BProtoObjectVeterancy>
		, IEqualityComparer<BProtoObjectVeterancy>
	{
		sealed class EqualityComparer : IEqualityComparer<BProtoObjectVeterancy>
		{
			#region IEqualityComparer<BProtoObjectVeterancy> Members
			public bool Equals(BProtoObjectVeterancy x, BProtoObjectVeterancy y)
			{
				return x.Xp == y.Xp
					&& x.Damage == y.Damage
					&& x.Velocity == y.Velocity
					&& x.Accuracy == y.Accuracy
					&& x.WorkRate == y.WorkRate
					&& x.WeaponRange == y.WeaponRange
					&& x.DamageTaken == y.DamageTaken;
			}

			public int GetHashCode(BProtoObjectVeterancy obj)
			{
				return obj.Xp.GetHashCode()
					^ obj.Damage.GetHashCode()
					^ obj.Velocity.GetHashCode()
					^ obj.Accuracy.GetHashCode()
					^ obj.WorkRate.GetHashCode()
					^ obj.WeaponRange.GetHashCode()
					^ obj.DamageTaken.GetHashCode();
			}
			#endregion
		};
		private static EqualityComparer gEqualityComparer_;
		public static IEqualityComparer<BProtoObjectVeterancy> EqualityComparer { get {
			if (gEqualityComparer_ == null)
				gEqualityComparer_ = new EqualityComparer();

			return gEqualityComparer_;
		} }

		#region Constants
		static readonly BProtoObjectVeterancy KInvalid = new BProtoObjectVeterancy(),
			KDefaultLevel1 = new BProtoObjectVeterancy()
			{
				mDamage_ = 1.15f, mVelocity_ = 1, mAccuracy_ = 1.6f, mWorkRate_ = 1.2f, mWeaponRange_ = 1f, mDamageTaken_ = 0.87f
			},
			KDefaultLevel2 = new BProtoObjectVeterancy()
			{
				mDamage_ = 1.15f, mVelocity_ = 1, mAccuracy_ = 1.7f, mWorkRate_ = 1.2f, mWeaponRange_ = 1f, mDamageTaken_ = 0.80f
			},
			KDefaultLevel3 = new BProtoObjectVeterancy()
			{
				mDamage_ = 1.15f, mVelocity_ = 1, mAccuracy_ = 1.8f, mWorkRate_ = 1.2f, mWeaponRange_ = 1f, mDamageTaken_ = 0.74f
			},
			KDefaultLevel4 = new BProtoObjectVeterancy()
			{
				mDamage_ = 2.00f, mVelocity_ = 1, mAccuracy_ = 1.1f, mWorkRate_ = 2.0f, mWeaponRange_ = 1f, mDamageTaken_ = 0.50f
			},
			KDefaultLevel5 = new BProtoObjectVeterancy()
			{
				mDamage_ = 2.00f, mVelocity_ = 1, mAccuracy_ = 1.2f, mWorkRate_ = 2.0f, mWeaponRange_ = 1f, mDamageTaken_ = 0.50f
			};

		public static IEnumerable<BProtoObjectVeterancy> GetLevelDefaults()
		{
			yield return KDefaultLevel1;
			yield return KDefaultLevel2;
			yield return KDefaultLevel3;
			yield return KDefaultLevel4;
			yield return KDefaultLevel5;
		}
		#endregion

		#region Xml constants
		public static readonly Collections.BListExplicitIndexParams<BProtoObjectVeterancy> KBListExplicitIndexParams = new
			Collections.BListExplicitIndexParams<BProtoObjectVeterancy>(5)
			{
				// We use a zero'd instance as the invalid format
				// Game considers Vets with XP = 0 as 'null' basically
				kTypeGetInvalid = () => KInvalid
			};
		public static readonly XML.BListExplicitIndexXmlParams<BProtoObjectVeterancy> KBListExplicitIndexXmlParams = new
			XML.BListExplicitIndexXmlParams<BProtoObjectVeterancy>("Veterancy", "Level");
		#endregion

		#region Properties
		float mXp_;
		public float Xp { get { return this.mXp_; } }
		float mDamage_ = 1.0f;
		public float Damage { get { return this.mDamage_; } }
		float mVelocity_ = 1.0f;
		public float Velocity { get { return this.mVelocity_; } }
		float mAccuracy_ = 1.0f;
		public float Accuracy { get { return this.mAccuracy_; } }
		float mWorkRate_ = 1.0f;
		public float WorkRate { get { return this.mWorkRate_; } }
		float mWeaponRange_ = 1.0f;
		public float WeaponRange { get { return this.mWeaponRange_; } }
		float mDamageTaken_ = 1.0f;
		public float DamageTaken { get { return this.mDamageTaken_; } }
		#endregion

		public bool IsInvalid { get { return ReferenceEquals(this, KInvalid); } }
		public bool IsIgnored { get { return this.mXp_ == 0.0f; } }

		#region IComparable Members
		int IComparable<BProtoObjectVeterancy>.CompareTo(BProtoObjectVeterancy other)
		{
			if (this.Xp != other.Xp)
				return this.Xp.CompareTo(other.Xp);

			if (this.Damage != other.Damage)
				return this.Damage.CompareTo(other.Damage);

			if (this.Velocity != other.Velocity)
				return this.Velocity.CompareTo(other.Velocity);

			if (this.Accuracy != other.Accuracy)
				return this.Accuracy.CompareTo(other.Accuracy);

			if (this.WorkRate != other.WorkRate)
				return this.WorkRate.CompareTo(other.WorkRate);

			if (this.WeaponRange != other.WeaponRange)
				return this.WeaponRange.CompareTo(other.WeaponRange);

			if (this.DamageTaken != other.DamageTaken)
				return this.DamageTaken.CompareTo(other.DamageTaken);

			return 0;
		}
		#endregion

		#region IEqualityComparer<BProtoObjectVeterancy> Members
		public bool Equals(BProtoObjectVeterancy x, BProtoObjectVeterancy y)
		{
			return EqualityComparer.Equals(x, y);
		}

		public int GetHashCode(BProtoObjectVeterancy obj)
		{
			return EqualityComparer.GetHashCode(obj);
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOpt("XP", ref this.mXp_, Predicates.IsNotZero);
			s.StreamAttributeOpt("Damage", ref this.mDamage_, PhxPredicates.IsNotOne);
			s.StreamAttributeOpt("Velocity", ref this.mVelocity_, PhxPredicates.IsNotOne);
			s.StreamAttributeOpt("Accuracy", ref this.mAccuracy_, PhxPredicates.IsNotOne);
			s.StreamAttributeOpt("WorkRate", ref this.mWorkRate_, PhxPredicates.IsNotOne);
			s.StreamAttributeOpt("WeaponRange", ref this.mWeaponRange_, PhxPredicates.IsNotOne);
			s.StreamAttributeOpt("DamageTaken", ref this.mDamageTaken_, PhxPredicates.IsNotOne);
		}
		#endregion
	};
}