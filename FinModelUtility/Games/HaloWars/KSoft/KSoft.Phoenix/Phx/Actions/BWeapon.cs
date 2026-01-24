
namespace KSoft.Phoenix.Phx
{
	public sealed class BWeapon
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "Weapon",
			dataName = "Name",
			flags = XML.BCollectionXmlParamsFlags.USE_ELEMENT_FOR_DATA |
				XML.BCollectionXmlParamsFlags.FORCE_NO_ROOT_ELEMENT_STREAMING,
		};
		#endregion

		#region Properties
		float mDamagePerSecond_ = PhxUtil.K_INVALID_SINGLE;
		public float DamagePerSecond { get { return this.mDamagePerSecond_; } }
		float mDotRate_ = PhxUtil.K_INVALID_SINGLE;
		public float DotRate { get { return this.mDotRate_; } }
		float mDotDuration_ = PhxUtil.K_INVALID_SINGLE;
		public float DotDuration { get { return this.mDotDuration_; } }

		float mAttackRate_ = PhxUtil.K_INVALID_SINGLE;
		public float AttackRate { get { return this.mAttackRate_; } }
		int mProjectileObjectId_ = TypeExtensions.K_NONE;
		public float ProjectileObjectId { get { return this.mProjectileObjectId_; } }

		int mWeaponTypeId_ = TypeExtensions.K_NONE;
		public int WeaponTypeId { get { return this.mWeaponTypeId_; } }
		int mVisualAmmo_ = TypeExtensions.K_NONE;
		public int VisualAmmo { get { return this.mVisualAmmo_; } }
		int mTriggerScriptId_ = TypeExtensions.K_NONE;
		public int TriggerScriptId { get { return this.mTriggerScriptId_; } }

		float mMinRange_ = PhxUtil.K_INVALID_SINGLE;
		public float MinRange { get { return this.mMinRange_; } }
		float mMaxRange_ = PhxUtil.K_INVALID_SINGLE;
		public float MaxRange { get { return this.mMaxRange_; } }

		float mReflectDamageFactor_ = PhxUtil.K_INVALID_SINGLE;
		public float ReflectDamageFactor { get { return this.mReflectDamageFactor_; } }
		float mMovingAccuracy_ = PhxUtil.K_INVALID_SINGLE;
		public float MovingAccuracy { get { return this.mMovingAccuracy_; } }
		float mMaxDeviation_ = PhxUtil.K_INVALID_SINGLE;
		public float MaxDeviation { get { return this.mMaxDeviation_; } }
		float mMovingMaxDeviation_ = PhxUtil.K_INVALID_SINGLE;
		public float MovingMaxDeviation { get { return this.mMovingMaxDeviation_; } }
		float mAccuracyDistanceFactor_ = PhxUtil.K_INVALID_SINGLE;
		public float AccuracyDistanceFactor { get { return this.mAccuracyDistanceFactor_; } }
		float mAccuracyDeviationFactor_ = PhxUtil.K_INVALID_SINGLE;
		public float AccuracyDeviationFactor { get { return this.mAccuracyDeviationFactor_; } }
		float mMaxVelocityLead_ = PhxUtil.K_INVALID_SINGLE;
		public float MaxVelocityLead { get { return this.mMaxVelocityLead_; } }
		float mAirBurstSpan_ = PhxUtil.K_INVALID_SINGLE;
		public float AirBurstSpan { get { return this.mAirBurstSpan_; } }

		public Collections.BTypeValues<BDamageRatingOverride> DamageOverrides { get; private set; } = new Collections.BTypeValues<BDamageRatingOverride>(BDamageRatingOverride.KBListParams);
		public Collections.BListArray<BTargetPriority> TargetPriorities { get; private set; } = new Collections.BListArray<BTargetPriority>();

		bool mStasisSmartTargeting_;
		public bool StasisSmartTargeting { get { return this.mStasisSmartTargeting_; } }
		float mStasisHealToDrainRatio_ = PhxUtil.K_INVALID_SINGLE;
		public float StasisHealToDrainRatio { get { return this.mStasisHealToDrainRatio_; } }

		sbyte mBounces_ = TypeExtensions.K_NONE;
		public sbyte Bounces { get { return this.mBounces_; } }
		float mBounceRange_ = PhxUtil.K_INVALID_SINGLE;
		public float BounceRange { get { return this.mBounceRange_; } }

		float mMaxPullRange_ = PhxUtil.K_INVALID_SINGLE;
		public float MaxPullRange { get { return this.mMaxPullRange_; } }
		#endregion

		#region BListAutoIdObject Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			var xs = s.GetSerializerInterface();

			s.StreamElementOpt("DamagePerSecond", ref this.mDamagePerSecond_, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("DOTrate", ref this.mDotRate_, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("DOTduration", ref this.mDotDuration_, PhxPredicates.IsNotInvalid);

			s.StreamElementOpt("AttackRate", ref this.mAttackRate_, PhxPredicates.IsNotInvalid);
			xs.StreamDbid(s, "Projectile", ref this.mProjectileObjectId_, DatabaseObjectKind.OBJECT);

			xs.StreamDbid(s, "WeaponType", ref this.mWeaponTypeId_, DatabaseObjectKind.WEAPON_TYPE);
			s.StreamElementOpt("VisualAmmo", ref this.mVisualAmmo_, Predicates.IsNotNone);
			//TriggerScript

			s.StreamElementOpt("MinRange", ref this.mMinRange_, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("MaxRange", ref this.mMaxRange_, PhxPredicates.IsNotInvalid);

			s.StreamElementOpt("ReflectDamageFactor", ref this.mReflectDamageFactor_, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("MovingAccuracy", ref this.mMovingAccuracy_, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("MaxDeviation", ref this.mMaxDeviation_, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("MovingMaxDeviation", ref this.mMovingMaxDeviation_, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("AccuracyDistanceFactor", ref this.mAccuracyDistanceFactor_, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("AccuracyDeviationFactor", ref this.mAccuracyDeviationFactor_, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("MaxVelocityLead", ref this.mMaxVelocityLead_, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("AirBurstSpan", ref this.mAirBurstSpan_, PhxPredicates.IsNotInvalid);

			XML.XmlUtil.Serialize(s, this.DamageOverrides, BDamageRatingOverride.KBListXmlParams);
			XML.XmlUtil.Serialize(s, this.TargetPriorities, BTargetPriority.KBListXmlParams);

			using (var bm = s.EnterCursorBookmarkOpt("Stasis", this, o => o.mStasisSmartTargeting_)) if (bm.IsNotNull)
				s.StreamAttribute("SmartTargeting", ref this.mStasisSmartTargeting_);

			s.StreamElementOpt("StasisHealToDrainRatio", ref this.mStasisHealToDrainRatio_, PhxPredicates.IsNotInvalid);

			s.StreamElementOpt("Bounces", ref this.mBounces_, Predicates.IsNotNone);
			s.StreamElementOpt("BounceRange", ref this.mBounceRange_, PhxPredicates.IsNotInvalid);

			s.StreamElementOpt("MaxPullRange", ref this.mMaxPullRange_, PhxPredicates.IsNotInvalid);
		}
		#endregion
	};
}
