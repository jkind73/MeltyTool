using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Phx
{
	[ProtoDataTypeObjectSourceKind(ProtoDataObjectSourceKind.GAME_DATA)]
	public sealed class BGameData
		: IO.ITagElementStringNameStreamable
		, IProtoDataObjectDatabaseProvider
	{
		public ProtoDataObjectDatabase ObjectDatabase { get; private set; }

		#region Xml constants
		const string K_XML_ROOT_ = "GameData";

		public static readonly Engine.XmlFileInfo KXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.DATA,
			FileName = "GameData.xml",
			RootName = K_XML_ROOT_
		};
		public static readonly Engine.ProtoDataXmlFileInfo KProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.GAME_DATA,
			KXmlFileInfo);

		public static readonly Collections.BTypeValuesParams<float> KRatesBListTypeValuesParams = new
			Collections.BTypeValuesParams<float>(db => db.GameData.Rates) { kTypeGetInvalid = PhxUtil.KGetInvalidSingle };
		static readonly XML.BListXmlParams KRatesXmlParams = new XML.BListXmlParams("Rate");
		public static readonly XML.BTypeValuesXmlParams<float> KRatesBListTypeValuesXmlParams = new
			XML.BTypeValuesXmlParams<float>("Rate", "Rate"); // oiy, really? name the 'type' attribute with the same name as the element?
		static readonly XML.BListXmlParams KPlayerStatesXmlParams = new XML.BListXmlParams("PlayerState");
		static readonly XML.BListXmlParams KPopsXmlParams = new XML.BListXmlParams("Pop");
		static readonly XML.BListXmlParams KRefCountsXmlParams = new XML.BListXmlParams("RefCount");
		static readonly XML.BListXmlParams KHudItemsXmlParams = new XML.BListXmlParams("HUDItem");
		static readonly XML.BListXmlParams KFlashableItemsXmlParams = new XML.BListXmlParams
		{
			rootName = "FlashableItems",
			elementName = "Item",
			flags = XML.BCollectionXmlParamsFlags.USE_INNER_TEXT_FOR_DATA,
		};
		static readonly XML.BListXmlParams KUnitFlagsXmlParams = new XML.BListXmlParams("UnitFlag");
		static readonly XML.BListXmlParams KSquadFlagsXmlParams = new XML.BListXmlParams("SquadFlag");

		static readonly Collections.BTypeValuesParams<string> KCodeProtoObjectsParams = new Collections.BTypeValuesParams<string>(db => db.GameProtoObjectTypes);
		static readonly XML.BTypeValuesXmlParams<string> KCodeProtoObjectsXmlParams = new XML.BTypeValuesXmlParams<string>("CodeProtoObject", "Type")
		{
			rootName = "CodeProtoObjects",
		};
		static readonly Collections.BTypeValuesParams<string> KCodeObjectTypesParams = new Collections.BTypeValuesParams<string>(db => db.GameObjectTypes);
		static readonly XML.BTypeValuesXmlParams<string> KCodeObjectTypesXmlParams = new XML.BTypeValuesXmlParams<string>("CodeObjectType", "Type")
		{
			rootName = "CodeObjectTypes",
		};
		#endregion

		public Collections.BListAutoId<BResource> Resources { get; private set; }
		public Collections.BTypeNames Rates { get; private set; }

		#region GoodAgainstGrades
		uint[] mGoodAgainstGrades_ = new uint[(int)ReticleAttackGrade.K_NUMBER_OF];
		public uint[] GoodAgainstGrades { get { return this.mGoodAgainstGrades_; } }
		#endregion

		#region DifficultyModifiers
		float[] mDifficultyModifiers_ = new float[(int)BDifficultyTypeModifier.K_NUMBER_OF];
		public float[] DifficultyModifiers { get { return this.mDifficultyModifiers_; } }
		#endregion

		public Collections.BTypeNames Populations { get; private set; }
		public Collections.BTypeNames RefCounts { get; private set; }
		public Collections.BTypeNames PlayerStates { get; private set; }

		#region GarrisonDamageMultiplier
		float mGarrisonDamageMultiplier_ = 1.0f;
		public float GarrisonDamageMultiplier
		{
			get { return this.mGarrisonDamageMultiplier_; }
			set { this.mGarrisonDamageMultiplier_ = value; }
		}
		#endregion

		#region ConstructionDamageMultiplier
		float mConstructionDamageMultiplier_ = 1.0f;
		public float ConstructionDamageMultiplier
		{
			get { return this.mConstructionDamageMultiplier_; }
			set { this.mConstructionDamageMultiplier_ = value; }
		}
		#endregion

		#region CaptureDecayRate
		float mCaptureDecayRate_;
		public float CaptureDecayRate
		{
			get { return this.mCaptureDecayRate_; }
			set { this.mCaptureDecayRate_ = value; }
		}
		#endregion

		#region SquadLeashLength
		float mSquadLeashLength_;
		public float SquadLeashLength
		{
			get { return this.mSquadLeashLength_; }
			set { this.mSquadLeashLength_ = value; }
		}
		#endregion

		#region SquadAggroLength
		float mSquadAggroLength_;
		public float SquadAggroLength
		{
			get { return this.mSquadAggroLength_; }
			set { this.mSquadAggroLength_ = value; }
		}

		/// <summary>Engine clamps aggro length to leash length</summary>
		public bool SquadAggroLengthIsLessThanLeash { get { return this.SquadAggroLength < this.SquadLeashLength; } }
		#endregion

		#region UnitLeashLength
		float mUnitLeashLength_;
		public float UnitLeashLength
		{
			get { return this.mUnitLeashLength_; }
			set { this.mUnitLeashLength_ = value; }
		}
		#endregion

		#region MaxNumCorpses
		int mMaxNumCorpses_;
		public int MaxNumCorpses
		{
			get { return this.mMaxNumCorpses_; }
			set { this.mMaxNumCorpses_ = value; }
		}
		#endregion

		#region BurningEffectLimits
		int mDefaultBurningEffectLimit_ = 1;
		public int DefaultBurningEffectLimit
		{
			get { return this.mDefaultBurningEffectLimit_; }
			set { this.mDefaultBurningEffectLimit_ = value; }
		}

		public Collections.BListArray<BBurningEffectLimit> BurningEffectLimits { get; private set; }
		#endregion

		#region Fatality
		float mFatalityTransitionScale_;
		public float FatalityTransitionScale
		{
			get { return this.mFatalityTransitionScale_; }
			set { this.mFatalityTransitionScale_ = value; }
		}

		float mFatalityMaxTransitionTime_;
		public float FatalityMaxTransitionTime
		{
			get { return this.mFatalityMaxTransitionTime_; }
			set { this.mFatalityMaxTransitionTime_ = value; }
		}

		float mFatalityPositionOffsetTolerance_;
		public float FatalityPositionOffsetTolerance
		{
			get { return this.mFatalityPositionOffsetTolerance_; }
			set { this.mFatalityPositionOffsetTolerance_ = value; }
		}

		float mFatalityOrientationOffsetTolerance_;
		/// <summary>angle</summary>
		public float FatalityOrientationOffsetTolerance
		{
			get { return this.mFatalityOrientationOffsetTolerance_; }
			set { this.mFatalityOrientationOffsetTolerance_ = value; }
		}

		float mFatalityExclusionRange_;
		public float FatalityExclusionRange
		{
			get { return this.mFatalityExclusionRange_; }
			set { this.mFatalityExclusionRange_ = value; }
		}
		#endregion

		#region GameOverDelay
		float mGameOverDelay_;
		public float GameOverDelay
		{
			get { return this.mGameOverDelay_; }
			set { this.mGameOverDelay_ = value; }
		}
		#endregion

		#region InfantryCorpseDecayTime
		float mInfantryCorpseDecayTime_;
		public float InfantryCorpseDecayTime
		{
			get { return this.mInfantryCorpseDecayTime_; }
			set { this.mInfantryCorpseDecayTime_ = value; }
		}
		#endregion

		#region CorpseSinkingSpacing
		float mCorpseSinkingSpacing_;
		public float CorpseSinkingSpacing
		{
			get { return this.mCorpseSinkingSpacing_; }
			set { this.mCorpseSinkingSpacing_ = value; }
		}
		#endregion

		#region MaxCorpseDisposalCount
		int mMaxCorpseDisposalCount_;
		public int MaxCorpseDisposalCount
		{
			get { return this.mMaxCorpseDisposalCount_; }
			set { this.mMaxCorpseDisposalCount_ = value; }
		}
		#endregion

		#region MaxSquadPathsPerFrame
		uint mMaxSquadPathsPerFrame_ = 10;
		public uint MaxSquadPathsPerFrame
		{
			get { return this.mMaxSquadPathsPerFrame_; }
			set { this.mMaxSquadPathsPerFrame_ = value; }
		}
		#endregion

		#region MaxPlatoonPathsPerFrame
		uint mMaxPlatoonPathsPerFrame_ = 10;
		public uint MaxPlatoonPathsPerFrame
		{
			get { return this.mMaxPlatoonPathsPerFrame_; }
			set { this.mMaxPlatoonPathsPerFrame_ = value; }
		}
		#endregion

		#region ProjectileGravity
		float mProjectileGravity_;
		public float ProjectileGravity
		{
			get { return this.mProjectileGravity_; }
			set { this.mProjectileGravity_ = value; }
		}
		#endregion

		#region ProjectileTumbleRate
		float mProjectileTumbleRate_;
		/// <summary>angle</summary>
		public float ProjectileTumbleRate
		{
			get { return this.mProjectileTumbleRate_; }
			set { this.mProjectileTumbleRate_ = value; }
		}
		#endregion

		#region TrackInterceptDistance
		float mTrackInterceptDistance_;
		public float TrackInterceptDistance
		{
			get { return this.mTrackInterceptDistance_; }
			set { this.mTrackInterceptDistance_ = value; }
		}
		#endregion

		#region StationaryTargetAttackToleranceAngle
		float mStationaryTargetAttackToleranceAngle_;
		public float StationaryTargetAttackToleranceAngle
		{
			get { return this.mStationaryTargetAttackToleranceAngle_; }
			set { this.mStationaryTargetAttackToleranceAngle_ = value; }
		}
		#endregion

		#region MovingTargetAttackToleranceAngle
		float mMovingTargetAttackToleranceAngle_;
		public float MovingTargetAttackToleranceAngle
		{
			get { return this.mMovingTargetAttackToleranceAngle_; }
			set { this.mMovingTargetAttackToleranceAngle_ = value; }
		}
		#endregion

		#region MovingTargetTrackingAttackToleranceAngle
		float mMovingTargetTrackingAttackToleranceAngle_;
		public float MovingTargetTrackingAttackToleranceAngle
		{
			get { return this.mMovingTargetTrackingAttackToleranceAngle_; }
			set { this.mMovingTargetTrackingAttackToleranceAngle_ = value; }
		}
		#endregion

		#region MovingTargetRangeMultiplier
		float mMovingTargetRangeMultiplier_ = 1.0f;
		public float MovingTargetRangeMultiplier
		{
			get { return this.mMovingTargetRangeMultiplier_; }
			set { this.mMovingTargetRangeMultiplier_ = value; }
		}
		#endregion

		#region CloakingDelay
		float mCloakingDelay_;
		public float CloakingDelay
		{
			get { return this.mCloakingDelay_; }
			set { this.mCloakingDelay_ = value; }
		}
		#endregion

		#region ReCloakDelay
		float mReCloakDelay_;
		public float ReCloakDelay
		{
			get { return this.mReCloakDelay_; }
			set { this.mReCloakDelay_ = value; }
		}
		#endregion

		#region CloakDetectFrequency
		float mCloakDetectFrequency_;
		public float CloakDetectFrequency
		{
			get { return this.mCloakDetectFrequency_; }
			set { this.mCloakDetectFrequency_ = value; }
		}
		#endregion

		#region ShieldRegenDelay
		float mShieldRegenDelay_;
		public float ShieldRegenDelay
		{
			get { return this.mShieldRegenDelay_; }
			set { this.mShieldRegenDelay_ = value; }
		}
		#endregion

		#region ShieldRegenTime
		float mShieldRegenTime_;
		public float ShieldRegenTime
		{
			get { return this.mShieldRegenTime_; }
			set { this.mShieldRegenTime_ = value; }
		}
		#endregion

		#region AttackedRevealerLOS
		float mAttackedRevealerLos_;
		public float AttackedRevealerLos
		{
			get { return this.mAttackedRevealerLos_; }
			set { this.mAttackedRevealerLos_ = value; }
		}
		#endregion

		#region AttackedRevealerLifespan
		float mAttackedRevealerLifespan_;
		public float AttackedRevealerLifespan
		{
			get { return this.mAttackedRevealerLifespan_; }
			set { this.mAttackedRevealerLifespan_ = value; }
		}
		#endregion

		#region AttackRevealerLOS
		float mAttackRevealerLos_;
		public float AttackRevealerLos
		{
			get { return this.mAttackRevealerLos_; }
			set { this.mAttackRevealerLos_ = value; }
		}
		#endregion

		#region AttackRevealerLifespan
		float mAttackRevealerLifespan_;
		public float AttackRevealerLifespan
		{
			get { return this.mAttackRevealerLifespan_; }
			set { this.mAttackRevealerLifespan_ = value; }
		}
		#endregion

		#region MinimumRevealerSize
		float mMinimumRevealerSize_;
		public float MinimumRevealerSize
		{
			get { return this.mMinimumRevealerSize_; }
			set { this.mMinimumRevealerSize_ = value; }
		}
		#endregion

		#region AttackRatingMultiplier
		float mAttackRatingMultiplier_ = 20f;
		public float AttackRatingMultiplier
		{
			get { return this.mAttackRatingMultiplier_; }
			set { this.mAttackRatingMultiplier_ = value; }
		}
		#endregion

		#region DefenseRatingMultiplier
		float mDefenseRatingMultiplier_ = 10f;
		public float DefenseRatingMultiplier
		{
			get { return this.mDefenseRatingMultiplier_; }
			set { this.mDefenseRatingMultiplier_ = value; }
		}
		#endregion

		#region GoodAgainstMinAttackGrade
		uint mGoodAgainstMinAttackGrade_ = 3;
		public uint GoodAgainstMinAttackGrade
		{
			get { return this.mGoodAgainstMinAttackGrade_; }
			set { this.mGoodAgainstMinAttackGrade_ = value; }
		}
		#endregion

		#region HeightBonusDamage
		float mHeightBonusDamage_;
		public float HeightBonusDamage
		{
			get { return this.mHeightBonusDamage_; }
			set { this.mHeightBonusDamage_ = value; }
		}
		#endregion

		#region ShieldBarColor
		System.Drawing.Color mShieldBarColor_;
		public System.Drawing.Color ShieldBarColor
		{
			get { return this.mShieldBarColor_; }
			set { this.mShieldBarColor_ = value; }
		}
		#endregion

		#region AmmoBarColor
		System.Drawing.Color mAmmoBarColor_;
		public System.Drawing.Color AmmoBarColor
		{
			get { return this.mAmmoBarColor_; }
			set { this.mAmmoBarColor_ = value; }
		}
		#endregion

		#region OpportunityDistPriFactor
		float mOpportunityDistPriFactor_ = 1.0f;
		public float OpportunityDistPriFactor
		{
			get { return this.mOpportunityDistPriFactor_; }
			set { this.mOpportunityDistPriFactor_ = value; }
		}
		#endregion

		#region OpportunityBeingAttackedPriBonus
		float mOpportunityBeingAttackedPriBonus_;
		public float OpportunityBeingAttackedPriBonus
		{
			get { return this.mOpportunityBeingAttackedPriBonus_; }
			set { this.mOpportunityBeingAttackedPriBonus_ = value; }
		}
		#endregion

		#region ChanceToRocket
		float mChanceToRocket_;
		public float ChanceToRocket
		{
			get { return this.mChanceToRocket_; }
			set { this.mChanceToRocket_ = value; }
		}
		#endregion

		#region MaxDamageBankPctAdjust
		float mMaxDamageBankPctAdjust_;
		public float MaxDamageBankPctAdjust
		{
			get { return this.mMaxDamageBankPctAdjust_; }
			set { this.mMaxDamageBankPctAdjust_ = value; }
		}
		#endregion

		#region DamageBankTimer
		float mDamageBankTimer_;
		public float DamageBankTimer
		{
			get { return this.mDamageBankTimer_; }
			set { this.mDamageBankTimer_ = value; }
		}
		#endregion

		#region BuildingSelfDestructTime
		float mBuildingSelfDestructTime_;
		public float BuildingSelfDestructTime
		{
			get { return this.mBuildingSelfDestructTime_; }
			set { this.mBuildingSelfDestructTime_ = value; }
		}
		#endregion

		#region TributeAmount
		float mTributeAmount_ = 500f;
		public float TributeAmount
		{
			get { return this.mTributeAmount_; }
			set { this.mTributeAmount_ = value; }
		}
		#endregion

		#region TributeCost
		float mTributeCost_;
		public float TributeCost
		{
			get { return this.mTributeCost_; }
			set { this.mTributeCost_ = value; }
		}
		#endregion

		#region UnscSupplyPadBonus
		float mUnscSupplyPadBonus_;
		public float UnscSupplyPadBonus
		{
			get { return this.mUnscSupplyPadBonus_; }
			set { this.mUnscSupplyPadBonus_ = value; }
		}
		#endregion

		#region UnscSupplyPadBreakEvenPoint
		float mUnscSupplyPadBreakEvenPoint_;
		public float UnscSupplyPadBreakEvenPoint
		{
			get { return this.mUnscSupplyPadBreakEvenPoint_; }
			set { this.mUnscSupplyPadBreakEvenPoint_ = value; }
		}
		#endregion

		#region CovSupplyPadBonus
		float mCovSupplyPadBonus_;
		public float CovSupplyPadBonus
		{
			get { return this.mCovSupplyPadBonus_; }
			set { this.mCovSupplyPadBonus_ = value; }
		}
		#endregion

		#region CovSupplyPadBreakEvenPoint
		float mCovSupplyPadBreakEvenPoint_;
		public float CovSupplyPadBreakEvenPoint
		{
			get { return this.mCovSupplyPadBreakEvenPoint_; }
			set { this.mCovSupplyPadBreakEvenPoint_ = value; }
		}
		#endregion

		#region LeaderPowerChargeResourceID
		int mLeaderPowerChargeResourceId_ = TypeExtensions.K_NONE;
		[Meta.ResourceReference]
		public int LeaderPowerChargeResourceId
		{
			get { return this.mLeaderPowerChargeResourceId_; }
			set { this.mLeaderPowerChargeResourceId_ = value; }
		}
		#endregion

		#region LeaderPowerChargeRateID
		int mLeaderPowerChargeRateId_ = TypeExtensions.K_NONE;
		[Meta.RateReference]
		public int LeaderPowerChargeRateId
		{
			get { return this.mLeaderPowerChargeRateId_; }
			set { this.mLeaderPowerChargeRateId_ = value; }
		}
		#endregion

		#region DamageReceivedXPFactor
		float mDamageReceivedXpFactor_;
		public float DamageReceivedXpFactor
		{
			get { return this.mDamageReceivedXpFactor_; }
			set { this.mDamageReceivedXpFactor_ = value; }
		}
		#endregion

		#region AirStrikeLoiterTime
		float mAirStrikeLoiterTime_;
		public float AirStrikeLoiterTime
		{
			get { return this.mAirStrikeLoiterTime_; }
			set { this.mAirStrikeLoiterTime_ = value; }
		}
		#endregion

		#region RecyleRefundRate
		float mRecyleRefundRate_ = 1.0f;
		public float RecyleRefundRate
		{
			get { return this.mRecyleRefundRate_; }
			set { this.mRecyleRefundRate_ = value; }
		}
		#endregion

		#region BaseRebuildTimer
		float mBaseRebuildTimer_;
		public float BaseRebuildTimer
		{
			get { return this.mBaseRebuildTimer_; }
			set { this.mBaseRebuildTimer_ = value; }
		}
		#endregion

		#region ObjectiveArrowRadialOffset
		float mObjectiveArrowRadialOffset_;
		public float ObjectiveArrowRadialOffset
		{
			get { return this.mObjectiveArrowRadialOffset_; }
			set { this.mObjectiveArrowRadialOffset_ = value; }
		}
		#endregion

		#region ObjectiveArrowSwitchOffset
		float mObjectiveArrowSwitchOffset_;
		public float ObjectiveArrowSwitchOffset
		{
			get { return this.mObjectiveArrowSwitchOffset_; }
			set { this.mObjectiveArrowSwitchOffset_ = value; }
		}
		#endregion

		#region ObjectiveArrowYOffset
		float mObjectiveArrowYOffset_;
		public float ObjectiveArrowYOffset
		{
			get { return this.mObjectiveArrowYOffset_; }
			set { this.mObjectiveArrowYOffset_ = value; }
		}
		#endregion

		#region ObjectiveArrowMaxIndex
		byte mObjectiveArrowMaxIndex_;
		public byte ObjectiveArrowMaxIndex
		{
			get { return this.mObjectiveArrowMaxIndex_; }
			set { this.mObjectiveArrowMaxIndex_ = value; }
		}
		#endregion

		#region OverrunMinVel
		float mOverrunMinVel_;
		public float OverrunMinVel
		{
			get { return this.mOverrunMinVel_; }
			set { this.mOverrunMinVel_ = value; }
		}
		#endregion

		#region OverrunJumpForce
		float mOverrunJumpForce_;
		public float OverrunJumpForce
		{
			get { return this.mOverrunJumpForce_; }
			set { this.mOverrunJumpForce_ = value; }
		}
		#endregion

		#region OverrunDistance
		float mOverrunDistance_;
		public float OverrunDistance
		{
			get { return this.mOverrunDistance_; }
			set { this.mOverrunDistance_ = value; }
		}
		#endregion

		#region CoopResourceSplitRate
		float mCoopResourceSplitRate_ = 1.0f;
		public float CoopResourceSplitRate
		{
			get { return this.mCoopResourceSplitRate_; }
			set { this.mCoopResourceSplitRate_ = value; }
		}
		#endregion

		#region Hero globals
		#region HeroDownedLOS
		float mHeroDownedLos_;
		public float HeroDownedLos
		{
			get { return this.mHeroDownedLos_; }
			set { this.mHeroDownedLos_ = value; }
		}
		#endregion

		#region HeroHPRegenTime
		float mHeroHpRegenTime_;
		public float HeroHpRegenTime
		{
			get { return this.mHeroHpRegenTime_; }
			set { this.mHeroHpRegenTime_ = value; }
		}
		#endregion

		#region HeroRevivalDistance
		float mHeroRevivalDistance_;
		public float HeroRevivalDistance
		{
			get { return this.mHeroRevivalDistance_; }
			set { this.mHeroRevivalDistance_ = value; }
		}
		#endregion

		#region HeroPercentHPRevivalThreshhold
		float mHeroPercentHpRevivalThreshhold_;
		public float HeroPercentHpRevivalThreshhold
		{
			get { return this.mHeroPercentHpRevivalThreshhold_; }
			set { this.mHeroPercentHpRevivalThreshhold_ = value; }
		}
		#endregion

		#region MaxDeadHeroTransportDist
		float mMaxDeadHeroTransportDist_;
		public float MaxDeadHeroTransportDist
		{
			get { return this.mMaxDeadHeroTransportDist_; }
			set { this.mMaxDeadHeroTransportDist_ = value; }
		}
		#endregion
		#endregion

		#region Transport
		float mTransportClearRadiusScale_ = 1.0f;
		public float TransportClearRadiusScale
		{
			get { return this.mTransportClearRadiusScale_; }
			set { this.mTransportClearRadiusScale_ = value; }
		}

		float mTransportMaxSearchRadiusScale_ = 1.0f;
		public float TransportMaxSearchRadiusScale
		{
			get { return this.mTransportMaxSearchRadiusScale_; }
			set { this.mTransportMaxSearchRadiusScale_ = value; }
		}

		uint mTransportMaxSearchLocations_ = 1;
		public uint TransportMaxSearchLocations
		{
			get { return this.mTransportMaxSearchLocations_; }
			set { this.mTransportMaxSearchLocations_ = value; }
		}

		uint mTransportBlockTime_;
		public uint TransportBlockTime
		{
			get { return this.mTransportBlockTime_; }
			set { this.mTransportBlockTime_ = value; }
		}

		uint mTransportLoadBlockTime_;
		public uint TransportLoadBlockTime
		{
			get { return this.mTransportLoadBlockTime_; }
			set { this.mTransportLoadBlockTime_ = value; }
		}
		#endregion

		#region Ambient life
		uint mAlMaxWanderFrequency_;
		public uint AlMaxWanderFrequency
		{
			get { return this.mAlMaxWanderFrequency_; }
			set { this.mAlMaxWanderFrequency_ = value; }
		}

		uint mAlPredatorCheckFrequency_;
		public uint AlPredatorCheckFrequency
		{
			get { return this.mAlPredatorCheckFrequency_; }
			set { this.mAlPredatorCheckFrequency_ = value; }
		}

		uint mAlPreyCheckFrequency_;
		public uint AlPreyCheckFrequency
		{
			get { return this.mAlPreyCheckFrequency_; }
			set { this.mAlPreyCheckFrequency_ = value; }
		}

		float mAlOppCheckRadius_;
		public float AlOppCheckRadius
		{
			get { return this.mAlOppCheckRadius_; }
			set { this.mAlOppCheckRadius_ = value; }
		}

		float mAlFleeDistance_;
		public float AlFleeDistance
		{
			get { return this.mAlFleeDistance_; }
			set { this.mAlFleeDistance_ = value; }
		}

		float mAlFleeMovementModifier_;
		public float AlFleeMovementModifier
		{
			get { return this.mAlFleeMovementModifier_; }
			set { this.mAlFleeMovementModifier_ = value; }
		}

		float mAlMinWanderDistance_;
		public float AlMinWanderDistance
		{
			get { return this.mAlMinWanderDistance_; }
			set { this.mAlMinWanderDistance_ = value; }
		}

		float mAlMaxWanderDistance_;
		public float AlMaxWanderDistance
		{
			get { return this.mAlMaxWanderDistance_; }
			set { this.mAlMaxWanderDistance_ = value; }
		}

		float mAlSpawnerCheckFrequency_;
		public float AlSpawnerCheckFrequency
		{
			get { return this.mAlSpawnerCheckFrequency_; }
			set { this.mAlSpawnerCheckFrequency_ = value; }
		}
		#endregion

		#region Transport
		uint mTransportMaxBlockAttempts_ = 1;
		public uint TransportMaxBlockAttempts
		{
			get { return this.mTransportMaxBlockAttempts_; }
			set { this.mTransportMaxBlockAttempts_ = value; }
		}

		float mTransportIncomingHeight_ = 60.0f;
		public float TransportIncomingHeight
		{
			get { return this.mTransportIncomingHeight_; }
			set { this.mTransportIncomingHeight_ = value; }
		}

		float mTransportIncomingOffset_ = 40.0f;
		public float TransportIncomingOffset
		{
			get { return this.mTransportIncomingOffset_; }
			set { this.mTransportIncomingOffset_ = value; }
		}

		float mTransportOutgoingHeight_ = 60.0f;
		public float TransportOutgoingHeight
		{
			get { return this.mTransportOutgoingHeight_; }
			set { this.mTransportOutgoingHeight_ = value; }
		}

		float mTransportOutgoingOffset_ = 40.0f;
		public float TransportOutgoingOffset
		{
			get { return this.mTransportOutgoingOffset_; }
			set { this.mTransportOutgoingOffset_ = value; }
		}

		float mTransportPickupHeight_ = 12.0f;
		public float TransportPickupHeight
		{
			get { return this.mTransportPickupHeight_; }
			set { this.mTransportPickupHeight_ = value; }
		}

		float mTransportDropoffHeight_ = 12.0f;
		public float TransportDropoffHeight
		{
			get { return this.mTransportDropoffHeight_; }
			set { this.mTransportDropoffHeight_ = value; }
		}

		uint mTransportMax_ = 3;
		public uint TransportMax
		{
			get { return this.mTransportMax_; }
			set { this.mTransportMax_ = value; }
		}
		#endregion

		#region HitchOffset
		float mHitchOffset_ = 8.0f;
		public float HitchOffset
		{
			get { return this.mHitchOffset_; }
			set { this.mHitchOffset_ = value; }
		}
		#endregion

		#region Cyro globals
		#region TimeFrozenToThaw
		float mTimeFrozenToThaw_;
		public float TimeFrozenToThaw
		{
			get { return this.mTimeFrozenToThaw_; }
			set { this.mTimeFrozenToThaw_ = value; }
		}
		#endregion

		#region TimeFreezingToThaw
		float mTimeFreezingToThaw_;
		public float TimeFreezingToThaw
		{
			get { return this.mTimeFreezingToThaw_; }
			set { this.mTimeFreezingToThaw_ = value; }
		}
		#endregion

		#region DefaultCryoPoints
		float mDefaultCryoPoints_;
		public float DefaultCryoPoints
		{
			get { return this.mDefaultCryoPoints_; }
			set { this.mDefaultCryoPoints_ = value; }
		}
		#endregion

		#region DefaultThawSpeed
		float mDefaultThawSpeed_;
		public float DefaultThawSpeed
		{
			get { return this.mDefaultThawSpeed_; }
			set { this.mDefaultThawSpeed_ = value; }
		}
		#endregion

		#region FreezingSpeedModifier
		float mFreezingSpeedModifier_;
		public float FreezingSpeedModifier
		{
			get { return this.mFreezingSpeedModifier_; }
			set { this.mFreezingSpeedModifier_ = value; }
		}
		#endregion

		#region FreezingDamageModifier
		float mFreezingDamageModifier_;
		public float FreezingDamageModifier
		{
			get { return this.mFreezingDamageModifier_; }
			set { this.mFreezingDamageModifier_ = value; }
		}
		#endregion

		#region FrozenDamageModifier
		float mFrozenDamageModifier_;
		public float FrozenDamageModifier
		{
			get { return this.mFrozenDamageModifier_; }
			set { this.mFrozenDamageModifier_ = value; }
		}
		#endregion
		#endregion

		#region SmallDotSize
		float mSmallDotSize_;
		public float SmallDotSize
		{
			get { return this.mSmallDotSize_; }
			set { this.mSmallDotSize_ = value; }
		}
		#endregion

		#region MediumDotSize
		float mMediumDotSize_;
		public float MediumDotSize
		{
			get { return this.mMediumDotSize_; }
			set { this.mMediumDotSize_ = value; }
		}
		#endregion

		public Collections.BTypeValuesString CodeProtoObjects { get; private set; }
		public Collections.BTypeValuesString CodeObjectTypes { get; private set; }
		public Collections.BListArray<BInfectionMap> InfectionMap { get; private set; }

		#region Nonsense
		/// <remarks>Engine doesn't process these, but some trigger scripts use these dynamic types, so keep them on record</remarks>
		public Collections.BTypeNames HudItems { get; private set; }
		/// <remarks>Engine doesn't process these, but some trigger scripts use these dynamic types, so keep them on record</remarks>
		public Collections.BTypeNames FlashableItems { get; private set; }
		/// <remarks>Engine doesn't process these, but some trigger scripts use these dynamic types, so keep them on record</remarks>
		public Collections.BTypeNames UnitFlags { get; private set; }
		/// <remarks>Engine doesn't process these, but some trigger scripts use these dynamic types, so keep them on record</remarks>
		public Collections.BTypeNames SquadFlags { get; private set; }
		#endregion

		/// <summary>Get how much it costs, in total, to tribute a resource to another player</summary>
		public float TotalTributeCost { get { return (this.mTributeAmount_ * this.mTributeCost_) + this.mTributeAmount_; } }

		public BGameData()
		{
			this.ObjectDatabase = new ProtoDataObjectDatabase(this, typeof(GameDataObjectKind));

			this.Resources = new Collections.BListAutoId<BResource>();
			this.Rates = new Collections.BTypeNames();
			#region DifficultyModifiers

			this.mDifficultyModifiers_[(int)BDifficultyTypeModifier.NORMAL] = 0.34f;
			this.mDifficultyModifiers_[(int)BDifficultyTypeModifier.HARD] = 0.67f;
			this.mDifficultyModifiers_[(int)BDifficultyTypeModifier.LEGENDARY] = 1.0f;
			this.mDifficultyModifiers_[(int)BDifficultyTypeModifier.DEFAULT] = 0.4f;
			this.mDifficultyModifiers_[(int)BDifficultyTypeModifier.SPCAI_DEFAULT] = 0.5f;
			#endregion

			this.Populations = new Collections.BTypeNames();
			this.RefCounts = new Collections.BTypeNames();
			this.PlayerStates = new Collections.BTypeNames();
			this.BurningEffectLimits = new Collections.BListArray<BBurningEffectLimit>();
			this.CodeProtoObjects = new Collections.BTypeValuesString(KCodeProtoObjectsParams);
			this.CodeObjectTypes = new Collections.BTypeValuesString(KCodeObjectTypesParams);
			this.InfectionMap = new Collections.BListArray<BInfectionMap>();

			#region Nonsense

			this.HudItems = new Collections.BTypeNames();
			this.FlashableItems = new Collections.BTypeNames();
			this.UnitFlags = new Collections.BTypeNames();
			this.SquadFlags = new Collections.BTypeNames();
			#endregion
		}

		#region Database interfaces
		internal Collections.IBTypeNames GetNamesInterface(GameDataObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != GameDataObjectKind.NONE);

			switch (kind)
			{
			case GameDataObjectKind.COST: return this.Resources;
			case GameDataObjectKind.POP:  return this.Populations;
			case GameDataObjectKind.RATE: return this.Rates;

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}

		internal Collections.IHasUndefinedProtoMemberInterface GetMembersInterface(GameDataObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != GameDataObjectKind.NONE);

			switch (kind)
			{
			case GameDataObjectKind.COST: return this.Resources;
			case GameDataObjectKind.POP:  return this.Populations;
			case GameDataObjectKind.RATE: return this.Rates;

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}
		#endregion

		#region ITagElementStreamable<string> Members
		/// <remarks>For streaming directly from gamedata.xml</remarks>
		internal void StreamGameData<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			XML.XmlUtil.Serialize(s, this.Resources, BResource.KBListXmlParams);
			XML.XmlUtil.Serialize(s, this.Rates, KRatesXmlParams);
			#region GoodAgainstReticle
			using (s.EnterCursorBookmark("GoodAgainstReticle"))
			{
				s.StreamElementOpt("NoEffect", ref this.mGoodAgainstGrades_[(int)ReticleAttackGrade.NO_EFFECT], Predicates.IsNotZero);
				s.StreamElementOpt("Weak",     ref this.mGoodAgainstGrades_[(int)ReticleAttackGrade.WEAK], Predicates.IsNotZero);
				s.StreamElementOpt("Fair",     ref this.mGoodAgainstGrades_[(int)ReticleAttackGrade.FAIR], Predicates.IsNotZero);
				s.StreamElementOpt("Good",     ref this.mGoodAgainstGrades_[(int)ReticleAttackGrade.GOOD], Predicates.IsNotZero);
				s.StreamElementOpt("Extreme",  ref this.mGoodAgainstGrades_[(int)ReticleAttackGrade.EXTREME], Predicates.IsNotZero);
			}
			#endregion
			#region DifficultyModifiers
			s.StreamElementOpt("DifficultyEasy",         ref this.mDifficultyModifiers_[(int)BDifficultyTypeModifier.EASY], Predicates.IsNotZero);
			// #NOTE The engine has a typo in it and looks for "DifficultyNormali"
			s.StreamElementOpt("DifficultyNormal",       ref this.mDifficultyModifiers_[(int)BDifficultyTypeModifier.NORMAL], Predicates.IsNotZero);
			s.StreamElementOpt("DifficultyHard",         ref this.mDifficultyModifiers_[(int)BDifficultyTypeModifier.HARD], Predicates.IsNotZero);
			s.StreamElementOpt("DifficultyLegendary",    ref this.mDifficultyModifiers_[(int)BDifficultyTypeModifier.LEGENDARY], Predicates.IsNotZero);
			s.StreamElementOpt("DifficultyDefault",      ref this.mDifficultyModifiers_[(int)BDifficultyTypeModifier.DEFAULT], Predicates.IsNotZero);
			s.StreamElementOpt("DifficultySPCAIDefault", ref this.mDifficultyModifiers_[(int)BDifficultyTypeModifier.SPCAI_DEFAULT], Predicates.IsNotZero);
			#endregion
			XML.XmlUtil.Serialize(s, this.Populations, KPopsXmlParams);
			XML.XmlUtil.Serialize(s, this.RefCounts, KRefCountsXmlParams);
			XML.XmlUtil.Serialize(s, this.PlayerStates, KPlayerStatesXmlParams);
			s.StreamElementOpt("GarrisonDamageMultiplier", ref this.mGarrisonDamageMultiplier_, PhxPredicates.IsNotOne);
			s.StreamElementOpt("ConstructionDamageMultiplier", ref this.mConstructionDamageMultiplier_, PhxPredicates.IsNotOne);
			s.StreamElementOpt("CaptureDecayRate", ref this.mCaptureDecayRate_, Predicates.IsNotZero);
			s.StreamElementOpt("SquadLeashLength", ref this.mSquadLeashLength_, Predicates.IsNotZero);
			s.StreamElementOpt("SquadAggroLength", ref this.mSquadAggroLength_, Predicates.IsNotZero);
			s.StreamElementOpt("UnitLeashLength", ref this.mUnitLeashLength_, Predicates.IsNotZero);
			s.StreamElementOpt("MaxNumCorpses", ref this.mMaxNumCorpses_, Predicates.IsNotZero);
			#region BurningEffectLimits
			using (s.EnterCursorBookmark("BurningEffectLimits"))
			{
				s.StreamAttribute("DefaultLimit", ref this.mDefaultBurningEffectLimit_);
				XML.XmlUtil.Serialize(s, this.BurningEffectLimits, BBurningEffectLimit.KBListXmlParams);
			}
			#endregion
			#region Fatality
			s.StreamElementOpt("FatalityTransitionScale", ref this.mFatalityTransitionScale_, Predicates.IsNotZero);
			s.StreamElementOpt("FatalityMaxTransitionTime", ref this.mFatalityMaxTransitionTime_, Predicates.IsNotZero);
			s.StreamElementOpt("FatalityPositionOffsetTolerance", ref this.mFatalityPositionOffsetTolerance_, Predicates.IsNotZero);
			s.StreamElementOpt("FatalityOrientationOffsetTolerance", ref this.mFatalityOrientationOffsetTolerance_, Predicates.IsNotZero);
			s.StreamElementOpt("FatalityExclusionRange", ref this.mFatalityExclusionRange_, Predicates.IsNotZero);
			#endregion
			s.StreamElementOpt("GameOverDelay", ref this.mGameOverDelay_, Predicates.IsNotZero);
			s.StreamElementOpt("InfantryCorpseDecayTime", ref this.mInfantryCorpseDecayTime_, Predicates.IsNotZero);
			s.StreamElementOpt("CorpseSinkingSpacing", ref this.mCorpseSinkingSpacing_, Predicates.IsNotZero);
			s.StreamElementOpt("MaxCorpseDisposalCount", ref this.mMaxCorpseDisposalCount_, Predicates.IsNotZero);
			s.StreamElementOpt("MaxSquadPathsPerFrame", ref this.mMaxSquadPathsPerFrame_, Predicates.IsNotZero);
			s.StreamElementOpt("MaxPlatoonPathsPerFrame", ref this.mMaxPlatoonPathsPerFrame_, Predicates.IsNotZero);
			s.StreamElementOpt("ProjectileGravity", ref this.mProjectileGravity_, Predicates.IsNotZero);
			s.StreamElementOpt("ProjectileTumbleRate", ref this.mProjectileTumbleRate_, Predicates.IsNotZero);
			s.StreamElementOpt("TrackInterceptDistance", ref this.mTrackInterceptDistance_, Predicates.IsNotZero);
			s.StreamElementOpt("StationaryTargetAttackToleranceAngle", ref this.mStationaryTargetAttackToleranceAngle_, Predicates.IsNotZero);
			s.StreamElementOpt("MovingTargetAttackToleranceAngle", ref this.mMovingTargetAttackToleranceAngle_, Predicates.IsNotZero);
			s.StreamElementOpt("MovingTargetTrackingAttackToleranceAngle", ref this.mMovingTargetTrackingAttackToleranceAngle_, Predicates.IsNotZero);
			s.StreamElementOpt("MovingTargetRangeMultiplier", ref this.mMovingTargetRangeMultiplier_, PhxPredicates.IsNotOne);
			s.StreamElementOpt("CloakingDelay", ref this.mCloakingDelay_, Predicates.IsNotZero);
			s.StreamElementOpt("ReCloakDelay", ref this.mReCloakDelay_, Predicates.IsNotZero);
			s.StreamElementOpt("CloakDetectFrequency", ref this.mCloakDetectFrequency_, Predicates.IsNotZero);
			s.StreamElementOpt("ShieldRegenDelay", ref this.mShieldRegenDelay_, Predicates.IsNotZero);
			s.StreamElementOpt("ShieldRegenTime", ref this.mShieldRegenTime_, Predicates.IsNotZero);
			s.StreamElementOpt("AttackedRevealerLOS", ref this.mAttackedRevealerLos_, Predicates.IsNotZero);
			s.StreamElementOpt("AttackedRevealerLifespan", ref this.mAttackedRevealerLifespan_, Predicates.IsNotZero);
			s.StreamElementOpt("AttackRevealerLOS", ref this.mAttackRevealerLos_, Predicates.IsNotZero);
			s.StreamElementOpt("AttackRevealerLifespan", ref this.mAttackRevealerLifespan_, Predicates.IsNotZero);
			s.StreamElementOpt("MinimumRevealerSize", ref this.mMinimumRevealerSize_, Predicates.IsNotZero);
			s.StreamElementOpt("AttackRatingMultiplier", ref this.mAttackRatingMultiplier_, Predicates.IsNotZero);
			s.StreamElementOpt("DefenseRatingMultiplier", ref this.mDefenseRatingMultiplier_, Predicates.IsNotZero);
			// #NOTE data has this as "GoodAgainstMinAttackRating"
			s.StreamElementOpt("GoodAgainstMinAttackRating", ref this.mGoodAgainstMinAttackGrade_, Predicates.IsNotZero);
			s.StreamElementOpt("HeightBonusDamage", ref this.mHeightBonusDamage_, Predicates.IsNotZero);
			s.StreamIntegerColor("ShieldBarColor", ref this.mShieldBarColor_);
			s.StreamIntegerColor("AmmoBarColor", ref this.mAmmoBarColor_);
			s.StreamElementOpt("OpportunityDistPriFactor", ref this.mOpportunityDistPriFactor_, PhxPredicates.IsNotOne);
			s.StreamElementOpt("OpportunityBeingAttackedPriBonus", ref this.mOpportunityBeingAttackedPriBonus_, Predicates.IsNotZero);
			s.StreamElementOpt("ChanceToRocket", ref this.mChanceToRocket_, Predicates.IsNotZero);
			s.StreamElementOpt("MaxDamageBankPctAdjust", ref this.mMaxDamageBankPctAdjust_, Predicates.IsNotZero);
			s.StreamElementOpt("DamageBankTimer", ref this.mDamageBankTimer_, Predicates.IsNotZero);
			s.StreamElementOpt("BuildingSelfDestructTime", ref this.mBuildingSelfDestructTime_, Predicates.IsNotZero);
			s.StreamElementOpt("TributeAmount", ref this.mTributeAmount_, Predicates.IsNotZero);
			s.StreamElementOpt("TributeCost", ref this.mTributeCost_, Predicates.IsNotZero);
			s.StreamElementOpt("UnscSupplyPadBonus", ref this.mUnscSupplyPadBonus_, Predicates.IsNotZero);
			s.StreamElementOpt("UnscSupplyPadBreakEvenPoint", ref this.mUnscSupplyPadBreakEvenPoint_, Predicates.IsNotZero);
			s.StreamElementOpt("CovSupplyPadBonus", ref this.mCovSupplyPadBonus_, Predicates.IsNotZero);
			s.StreamElementOpt("CovSupplyPadBreakEvenPoint", ref this.mCovSupplyPadBreakEvenPoint_, Predicates.IsNotZero);
			xs.StreamTypeName(s, "LeaderPowerChargeResource", ref this.mLeaderPowerChargeResourceId_, GameDataObjectKind.COST);
			xs.StreamTypeName(s, "LeaderPowerChargeRate", ref this.mLeaderPowerChargeRateId_, GameDataObjectKind.RATE);
			s.StreamElementOpt("DamageReceivedXPFactor", ref this.mDamageReceivedXpFactor_, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("AirStrikeLoiterTime", ref this.mAirStrikeLoiterTime_, Predicates.IsNotZero);
			s.StreamElementOpt("RecyleRefundRate", ref this.mRecyleRefundRate_, PhxPredicates.IsNotOne);
			s.StreamElementOpt("BaseRebuildTimer", ref this.mBaseRebuildTimer_, Predicates.IsNotZero);
			s.StreamElementOpt("ObjectiveArrowRadialOffset", ref this.mObjectiveArrowRadialOffset_, Predicates.IsNotZero);
			s.StreamElementOpt("ObjectiveArrowSwitchOffset", ref this.mObjectiveArrowSwitchOffset_, Predicates.IsNotZero);
			s.StreamElementOpt("ObjectiveArrowYOffset", ref this.mObjectiveArrowYOffset_, Predicates.IsNotZero);
			s.StreamElementOpt("ObjectiveArrowMaxIndex", ref this.mObjectiveArrowMaxIndex_, Predicates.IsNotZero);
			s.StreamElementOpt("OverrunMinVel", ref this.mOverrunMinVel_, Predicates.IsNotZero);
			s.StreamElementOpt("OverrunJumpForce", ref this.mOverrunJumpForce_, Predicates.IsNotZero);
			s.StreamElementOpt("OverrunDistance", ref this.mOverrunDistance_, Predicates.IsNotZero);
			s.StreamElementOpt("CoopResourceSplitRate", ref this.mCoopResourceSplitRate_, PhxPredicates.IsNotOne);
			#region Hero globals
			s.StreamElementOpt("HeroDownedLOS", ref this.mHeroDownedLos_, Predicates.IsNotZero);
			s.StreamElementOpt("HeroHPRegenTime", ref this.mHeroHpRegenTime_, Predicates.IsNotZero);
			s.StreamElementOpt("HeroRevivalDistance", ref this.mHeroRevivalDistance_, Predicates.IsNotZero);
			s.StreamElementOpt("HeroPercentHPRevivalThreshhold", ref this.mHeroPercentHpRevivalThreshhold_, Predicates.IsNotZero);
			s.StreamElementOpt("MaxDeadHeroTransportDist", ref this.mMaxDeadHeroTransportDist_, Predicates.IsNotZero);
			#endregion
			#region Transport
			s.StreamElementOpt("TransportClearRadiusScale", ref this.mTransportClearRadiusScale_, PhxPredicates.IsNotOne);
			s.StreamElementOpt("TransportMaxSearchRadiusScale", ref this.mTransportMaxSearchRadiusScale_, PhxPredicates.IsNotOne);
			s.StreamElementOpt("TransportMaxSearchLocations", ref this.mTransportMaxSearchLocations_, PhxPredicates.IsNotOne);
			s.StreamElementOpt("TransportBlockTime", ref this.mTransportBlockTime_, Predicates.IsNotZero);
			s.StreamElementOpt("TransportLoadBlockTime", ref this.mTransportLoadBlockTime_, Predicates.IsNotZero);
			#endregion
			#region Ambient life
			s.StreamElementOpt("ALMaxWanderFrequency", ref this.mAlMaxWanderFrequency_, Predicates.IsNotZero);
			s.StreamElementOpt("ALPredatorCheckFrequency", ref this.mAlPredatorCheckFrequency_, Predicates.IsNotZero);
			s.StreamElementOpt("ALPreyCheckFrequency", ref this.mAlPreyCheckFrequency_, Predicates.IsNotZero);
			s.StreamElementOpt("ALOppCheckRadius", ref this.mAlOppCheckRadius_, Predicates.IsNotZero);
			s.StreamElementOpt("ALFleeDistance", ref this.mAlFleeDistance_, Predicates.IsNotZero);
			s.StreamElementOpt("ALFleeMovementModifier", ref this.mAlFleeMovementModifier_, Predicates.IsNotZero);
			s.StreamElementOpt("ALMinWanderDistance", ref this.mAlMinWanderDistance_, Predicates.IsNotZero);
			s.StreamElementOpt("ALMaxWanderDistance", ref this.mAlMaxWanderDistance_, Predicates.IsNotZero);
			s.StreamElementOpt("ALSpawnerCheckFrequency", ref this.mAlSpawnerCheckFrequency_, Predicates.IsNotZero);
			#endregion
			#region Transport
			s.StreamElementOpt("TransportMaxBlockAttempts", ref this.mTransportMaxBlockAttempts_, PhxPredicates.IsNotOne);
			s.StreamElementOpt("TransportIncomingHeight", ref this.mTransportIncomingHeight_, Predicates.IsNotZero);
			s.StreamElementOpt("TransportIncomingOffset", ref this.mTransportIncomingOffset_, Predicates.IsNotZero);
			s.StreamElementOpt("TransportOutgoingHeight", ref this.mTransportOutgoingHeight_, Predicates.IsNotZero);
			s.StreamElementOpt("TransportOutgoingOffset", ref this.mTransportOutgoingOffset_, Predicates.IsNotZero);
			s.StreamElementOpt("TransportPickupHeight", ref this.mTransportPickupHeight_, Predicates.IsNotZero);
			s.StreamElementOpt("TransportDropoffHeight", ref this.mTransportDropoffHeight_, Predicates.IsNotZero);
			s.StreamElementOpt("TransportMax", ref this.mTransportMax_, Predicates.IsNotZero);
			#endregion
			s.StreamElementOpt("HitchOffset", ref this.mHitchOffset_, Predicates.IsNotZero);
			#region Cryo globals
			s.StreamElementOpt("TimeFrozenToThaw", ref this.mTimeFrozenToThaw_, Predicates.IsNotZero);
			s.StreamElementOpt("TimeFreezingToThaw", ref this.mTimeFreezingToThaw_, Predicates.IsNotZero);
			s.StreamElementOpt("DefaultCryoPoints", ref this.mDefaultCryoPoints_, Predicates.IsNotZero);
			s.StreamElementOpt("DefaultThawSpeed", ref this.mDefaultThawSpeed_, Predicates.IsNotZero);
			s.StreamElementOpt("FreezingSpeedModifier", ref this.mFreezingSpeedModifier_, Predicates.IsNotZero);
			s.StreamElementOpt("FreezingDamageModifier", ref this.mFreezingDamageModifier_, Predicates.IsNotZero);
			s.StreamElementOpt("FrozenDamageModifier", ref this.mFrozenDamageModifier_, Predicates.IsNotZero);
			#endregion
			using (s.EnterCursorBookmark("Dot"))
			{
				s.StreamAttributeOpt("small", ref this.mSmallDotSize_, Predicates.IsNotZero);
				s.StreamAttributeOpt("medium", ref this.mMediumDotSize_, Predicates.IsNotZero);
			}

			XML.XmlUtil.Serialize(s, this.CodeProtoObjects, KCodeProtoObjectsXmlParams);
			XML.XmlUtil.Serialize(s, this.CodeObjectTypes, KCodeObjectTypesXmlParams);
			XML.XmlUtil.Serialize(s, this.InfectionMap, BInfectionMap.KBListXmlParams);

			#region Nonsense
			XML.XmlUtil.Serialize(s, this.HudItems, KHudItemsXmlParams);
			XML.XmlUtil.Serialize(s, this.FlashableItems, KFlashableItemsXmlParams);
			XML.XmlUtil.Serialize(s, this.UnitFlags, KUnitFlagsXmlParams);
			XML.XmlUtil.Serialize(s, this.SquadFlags, KSquadFlagsXmlParams);
			#endregion
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark(K_XML_ROOT_))
				this.StreamGameData(s);
		}
		#endregion

		#region IProtoDataObjectDatabaseProvider members
		Engine.XmlFileInfo IProtoDataObjectDatabaseProvider.SourceFileReference { get { return KXmlFileInfo; } }

		Collections.IBTypeNames IProtoDataObjectDatabaseProvider.GetNamesInterface(int objectKind)
		{
			var kind = (GameDataObjectKind)objectKind;
			return this.GetNamesInterface(kind);
		}

		Collections.IHasUndefinedProtoMemberInterface IProtoDataObjectDatabaseProvider.GetMembersInterface(int objectKind)
		{
			var kind = (GameDataObjectKind)objectKind;
			return this.GetMembersInterface(kind);
		}
		#endregion
	};

	public sealed class BBurningEffectLimit
		: IO.ITagElementStringNameStreamable
		, IComparable<BBurningEffectLimit>
		, IEquatable<BBurningEffectLimit>
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "BurningEffectLimitEntry",
		};
		#endregion

		#region Limit
		int mLimit_;
		public int Limit
		{
			get { return this.mLimit_; }
			set { this.mLimit_ = value; }
		}
		#endregion

		#region ObjectTypeID
		int mObjectTypeId_ = TypeExtensions.K_NONE;
		[Meta.UnitReference]
		public int ObjectTypeId
		{
			get { return this.mObjectTypeId_; }
			set { this.mObjectTypeId_ = value; }
		}
		#endregion

		public bool IsInvalid { get { return PhxUtil.IsUndefinedReferenceHandleOrNone(this.ObjectTypeId); } }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttribute("Limit", ref this.mLimit_);
			xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mObjectTypeId_, DatabaseObjectKind.UNIT, false, XML.XmlUtil.K_SOURCE_CURSOR);
		}
		#endregion

		#region IComparable Members
		public int CompareTo(BBurningEffectLimit other)
		{
			if (this.Limit != other.Limit)
				this.Limit.CompareTo(other.Limit);

			return this.ObjectTypeId.CompareTo(other.ObjectTypeId);
		}
		#endregion

		#region IEquatable Members
		public bool Equals(BBurningEffectLimit other)
		{
			return this.Limit == other.Limit
				&&
				this.ObjectTypeId == other.ObjectTypeId;
		}
		#endregion
	};
}