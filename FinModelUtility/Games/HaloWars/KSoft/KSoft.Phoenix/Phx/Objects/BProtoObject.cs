using System.Collections.Generic;

using BProtoObjectID = System.Int32;
using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.Phx
{
	/* Deprecated fields:
	 * - TrackInterceptDistance: This was made a global and moved to GameData.
	 * - FlashUI: these are no longer defined in this proto.
	 * - UIVisual: Not sure what happened with this. There's just the Visual field now though.
	 * - DazeResist: This was moved to ProtoSquad.
	 *
	 * #NOTE
	 * - "fx_impact_effect_01" - Hitpoints field with value "20000000" gets written as "2E+07" with TagElementTextStream's ToString("r") impl. This *should* get parsed correctly.
	 * - "cpgn_scn07_scarabBoss_02" - DeathFadeDelayTime value, 99999999, gets rounded up to 1E+08 when we serialize it
	*/

	public sealed class BProtoObject
		: DatabaseIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams("Object")
		{
			dataName = K_XML_ATTR_NAME,
			flags = 0
				| XML.BCollectionXmlParamsFlags.TO_LOWER_DATA_NAMES
				| XML.BCollectionXmlParamsFlags.REQUIRES_DATA_NAME_PRELOADING
				| XML.BCollectionXmlParamsFlags.SUPPORTS_UPDATING
		};
		public static readonly Collections.BListAutoIdParams KBListParams = new Collections.BListAutoIdParams()
		{
			ToLowerDataNames = KBListXmlParams.ToLowerDataNames,
		};

		public static readonly Engine.XmlFileInfo KXmlFileInfo = new Engine.XmlFileInfo
		{
			Location = Engine.ContentStorage.GAME,
			Directory = Engine.GameDirectory.DATA,
			FileName = "Objects.xml",
			RootName = KBListXmlParams.rootName
		};
		public static readonly Engine.XmlFileInfo KXmlFileInfoUpdate = new Engine.XmlFileInfo
		{
			Location = Engine.ContentStorage.UPDATE,
			Directory = Engine.GameDirectory.DATA,
			FileName = "Objects_Update.xml",
			RootName = KBListXmlParams.rootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo KProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.PROTO_DATA,
			KXmlFileInfo,
			KXmlFileInfoUpdate);

		static readonly Collections.CodeEnum<BProtoObjectFlags> KFlagsProtoEnum = new Collections.CodeEnum<BProtoObjectFlags>();
		static readonly Collections.BBitSetParams KFlagsParams = new Collections.BBitSetParams(() => KFlagsProtoEnum);

		static readonly Collections.BBitSetParams KObjectTypesParams = new Collections.BBitSetParams(db => db.ObjectTypes);
		static readonly XML.BBitSetXmlParams KObjectTypesXmlParams = new XML.BBitSetXmlParams("ObjectType");
	internal const string K_XML_ELEMENT_ATTACK_GRADE_DPS = "AttackGradeDPS";
		internal const string K_XML_ELEMENT_REVERSE_SPEED = "ReverseSpeed";
		#endregion

		#region Unused poop
		int mUnusedIs_ = TypeExtensions.K_NONE;
		public int UnusedIs { get { return this.mUnusedIs_; } }
		int mUnusedId_ = TypeExtensions.K_NONE;
		public int UnusedId { get { return this.mUnusedId_; } }
		#endregion
		bool mUpdate_;
		public bool Update { get { return this.mUpdate_; } }

		#region MovementType
		BProtoObjectMovementType mMovementType_ = BProtoObjectMovementType.NONE;
		public BProtoObjectMovementType MovementType
		{
			get { return this.mMovementType_; }
			set { this.mMovementType_ = value; }
		}
		#endregion

		public Collections.BListArray<		BHardpoint> Hardpoints { get; private set; }
			= new Collections.BListArray<	BHardpoint>();
		public List<string> SingleBoneIKs { get; private set; }
			= [];
		public Collections.BListArray<		BGroundIkNode> GroundIKs { get; private set; }
			= new Collections.BListArray<	BGroundIkNode>();
		public Collections.BListArray<		BSweetSpotIkNode> SweetSpotIKs { get; private set; }
			= new Collections.BListArray<	BSweetSpotIkNode>();

		#region ObstructionRadius
		BVector mObstructionRadius_;
		public BVector ObstructionRadius
		{
			get { return this.mObstructionRadius_; }
			set { this.mObstructionRadius_ = value; }
		}
		#endregion

		#region TerrainFlatten
		// Automatic terrain flattening for building placement

		BVector mTerrainFlattennMin0_;
		public BVector TerrainFlattenMin0
		{
			get { return this.mTerrainFlattennMin0_; }
			set { this.mTerrainFlattennMin0_ = value; }
		}
		BVector mTerrainFlattennMax0_;
		public BVector TerrainFlattenMax0
		{
			get { return this.mTerrainFlattennMax0_; }
			set { this.mTerrainFlattennMax0_ = value; }
		}

		BVector mTerrainFlattennMin1_;
		public BVector TerrainFlattenMin1
		{
			get { return this.mTerrainFlattennMin1_; }
			set { this.mTerrainFlattennMin1_ = value; }
		}
		BVector mTerrainFlattennMax1_;
		public BVector TerrainFlattenMax1
		{
			get { return this.mTerrainFlattennMax1_; }
			set { this.mTerrainFlattennMax1_ = value; }
		}
		#endregion
		#region Parking lot
		// Parking lot position for building placement

		BVector mParkingLotMin_;
		public BVector ParkingLotMin
		{
			get { return this.mParkingLotMin_; }
			set { this.mParkingLotMin_ = value; }
		}
		BVector mParkingLotMax_;
		public BVector ParkingLotMax
		{
			get { return this.mParkingLotMax_; }
			set { this.mParkingLotMax_ = value; }
		}
		#endregion
		#region TerrainHeightTolerance
		const float C_DEFAULT_TERRAIN_HEIGHT_TOLERANCE_ = 10.0f;

		float mTerrainHeightTolerance_ = C_DEFAULT_TERRAIN_HEIGHT_TOLERANCE_;
		public float TerrainHeightTolerance
		{
			get { return this.mTerrainHeightTolerance_; }
			set { this.mTerrainHeightTolerance_ = value; }
		}
		#endregion
		#region Physics
		string mPhysicsInfo_;
		[Meta.PhysicsInfoReference]
		public string PhysicsInfo
		{
			get { return this.mPhysicsInfo_; }
			set { this.mPhysicsInfo_ = value; }
		}

		string mPhysicsReplacementInfo_;
		[Meta.PhysicsInfoReference]
		public string PhysicsReplacementInfo
		{
			get { return this.mPhysicsReplacementInfo_; }
			set { this.mPhysicsReplacementInfo_ = value; }
		}

		float mVelocity_;
		public float Velocity
		{
			get { return this.mVelocity_; }
			set { this.mVelocity_ = value; }
		}

		float mMaxVelocity_;
		public float MaxVelocity
		{
			get { return this.mMaxVelocity_; }
			set { this.mMaxVelocity_ = value; }
		}

		const float C_DEFAULT_REVERSE_SPEED_ = 1.0f;
		float mReverseSpeed_ = C_DEFAULT_REVERSE_SPEED_;
		public float ReverseSpeed
		{
			get { return this.mReverseSpeed_; }
			set { this.mReverseSpeed_ = value; }
		}

		float mAcceleration_;
		public float Acceleration
		{
			get { return this.mAcceleration_; }
			set { this.mAcceleration_ = value; }
		}

		float mTrackingDelay_;
		// in seconds
		public float TrackingDelay
		{
			get { return this.mTrackingDelay_; }
			set { this.mTrackingDelay_ = value; }
		}

		float mStartingVelocity_;
		public float StartingVelocity
		{
			get { return this.mStartingVelocity_; }
			set { this.mStartingVelocity_ = value; }
		}
		#endregion
		#region Fuel
		float mFuel_;
		public float Fuel
		{
			get { return this.mFuel_; }
			set { this.mFuel_ = value; }
		}
		#endregion
		#region Perturbance

		float mPerturbanceChance_;
		public float PerturbanceChance
		{
			get { return this.mPerturbanceChance_; }
			set { this.mPerturbanceChance_ = value; }
		}

		float mPerturbanceVelocity_;
		public float PerturbanceVelocity
		{
			get { return this.mPerturbanceVelocity_; }
			set { this.mPerturbanceVelocity_ = value; }
		}

		float mPerturbanceMinTime_;
		public float PerturbanceMinTime
		{
			get { return this.mPerturbanceMinTime_; }
			set { this.mPerturbanceMinTime_ = value; }
		}

		float mPerturbanceMaxTime_;
		public float PerturbanceMaxTime
		{
			get { return this.mPerturbanceMaxTime_; }
			set { this.mPerturbanceMaxTime_ = value; }
		}

		float mPerturbInitialVelocity_;
		public float PerturbInitialVelocity
		{
			get { return this.mPerturbInitialVelocity_; }
			set { this.mPerturbInitialVelocity_ = value; }
		}

		float mInitialPerturbanceMinTime_;
		public float InitialPerturbanceMinTime
		{
			get { return this.mInitialPerturbanceMinTime_; }
			set { this.mInitialPerturbanceMinTime_ = value; }
		}

		float mInitialPerturbanceMaxTime_;
		public float InitialPerturbanceMaxTime
		{
			get { return this.mInitialPerturbanceMaxTime_; }
			set { this.mInitialPerturbanceMaxTime_ = value; }
		}

		bool HasInitialPerturbanceData { get {
			return this.PerturbInitialVelocity != 0.0
				||
				this.InitialPerturbanceMinTime > 0.0
				||
				this.InitialPerturbanceMaxTime > 0.0;
		} }
		#endregion
		#region ActiveScan
		float mActiveScanChance_;
		public float ActiveScanChance
		{
			get { return this.mActiveScanChance_; }
			set { this.mActiveScanChance_ = value; }
		}

		float mActiveScanRadiusScale_;
		public float ActiveScanRadiusScale
		{
			get { return this.mActiveScanRadiusScale_; }
			set { this.mActiveScanRadiusScale_ = value; }
		}

		bool HasActiveScanData { get {
			return this.ActiveScanChance > 0.0
				||
				this.ActiveScanRadiusScale > 0.0;
		} }
		#endregion
		#region TurnRate
		float mTurnRate_;
		public float TurnRate
		{
			get { return this.mTurnRate_; }
			set { this.mTurnRate_ = value; }
		}
		#endregion
		#region Hitpoints
		float mHitpoints_;
		public float Hitpoints
		{
			get { return this.mHitpoints_; }
			set { this.mHitpoints_ = value; }
		}
		#endregion
		#region Shieldpoints
		float mShieldpoints_;
		public float Shieldpoints
		{
			get { return this.mShieldpoints_; }
			set { this.mShieldpoints_ = value; }
		}
		#endregion
		#region LOS
		float mLos_;
		public float Los
		{
			get { return this.mLos_; }
			set { this.mLos_ = value; }
		}
		#endregion
		#region Pick and Select

		float mPickRadius_;
		public float PickRadius
		{
			get { return this.mPickRadius_; }
			set { this.mPickRadius_ = value; }
		}

		float mPickOffset_;
		public float PickOffset
		{
			get { return this.mPickOffset_; }
			set { this.mPickOffset_ = value; }
		}

		BPickPriority mPickPriority_;
		public BPickPriority PickPriority
		{
			get { return this.mPickPriority_; }
			set { this.mPickPriority_ = value; }
		}

		BProtoObjectSelectType mSelectType_;
		public BProtoObjectSelectType SelectType
		{
			get { return this.mSelectType_; }
			set { this.mSelectType_ = value; }
		}

		BGotoType mGotoType_;
		public BGotoType GotoType
		{
			get { return this.mGotoType_; }
			set { this.mGotoType_ = value; }
		}

		BVector mSelectedRadius_;
		public BVector SelectedRadius
		{
			get { return this.mSelectedRadius_; }
			set { this.mSelectedRadius_ = value; }
		}

		#endregion
		#region RepairPoints
		float mRepairPoints_;
		[Meta.UnusedData]
		public float RepairPoints
		{
			get { return this.mRepairPoints_; }
			set { this.mRepairPoints_ = value; }
		}
		#endregion
		#region ClassType
		BProtoObjectClassType mClassType_ = BProtoObjectClassType.OBJECT;
		public BProtoObjectClassType ClassType
		{
			get { return this.mClassType_; }
			set { this.mClassType_ = value; }
		}
		#endregion
		#region TrainerType
		int mTrainerType_ = TypeExtensions.K_NONE;
		public int TrainerType
		{
			get { return this.mTrainerType_; }
			set { this.mTrainerType_ = value; }
		}

		bool mTrainerApplyFormation_;
		public bool TrainerApplyFormation
		{
			get { return this.mTrainerApplyFormation_; }
			set { this.TrainerApplyFormation = value; }
		}

		bool HasTrainerTypeData { get {
			return this.TrainerType.IsNotNone()
				||
				this.TrainerApplyFormation;
		} }
		#endregion
		#region AutoLockDown
		BAutoLockDown mAutoLockDown_;
		public BAutoLockDown AutoLockDown
		{
			get { return this.mAutoLockDown_; }
			set { this.mAutoLockDown_ = value; }
		}
		#endregion
		#region CostEscalation
		float mCostEscalation_ = 1.0f;
		/// <summary>see: UNSC reactors</summary>
		// Also, CostEscalationObject and Flag.LinearCostEscalation
		public float CostEscalation
		{
			get { return this.mCostEscalation_; }
			set { this.mCostEscalation_ = value; }
		}
		public bool HasCostEscalation { get { return this.CostEscalation > 0.0f; } }
		#endregion
		[Meta.BProtoObjectReference]
		public List<	BProtoObjectID> CostEscalationObjects { get; private set; }
			 = [];
		public Collections.BListArray<		BProtoObjectCaptureCost> CaptureCosts { get; private set; }
			= new Collections.BListArray<	BProtoObjectCaptureCost>();
		#region Bounty
		float mBounty_;
		/// <summary>Vet XP contribution value</summary>
		public float Bounty
		{
			get { return this.mBounty_; }
			set { this.mBounty_ = value; }
		}
		#endregion
		#region AIAssetValueAdjust
		float mAiAssetValueAdjust_;
		public float AiAssetValueAdjust
		{
			get { return this.mAiAssetValueAdjust_; }
			set { this.mAiAssetValueAdjust_ = value; }
		}
		#endregion
		#region CombatValue
		float mCombatValue_;
		/// <summary>Score value</summary>
		public float CombatValue
		{
			get { return this.mCombatValue_; }
			set { this.mCombatValue_ = value; }
		}
		#endregion
		#region ResourceAmount
		float mResourceAmount_;
		public float ResourceAmount
		{
			get { return this.mResourceAmount_; }
			set { this.mResourceAmount_ = value; }
		}
		#endregion
		#region PlacementRules
		string mPlacementRules_;
		/// <summary>PlacementRules file name (sans extension)</summary>
		public string PlacementRules
		{
			get { return this.mPlacementRules_; }
			set { this.mPlacementRules_ = value; }
		}
		#endregion
		#region DeathFadeTime
		float mDeathFadeTime_ = 1.0f;
		public float DeathFadeTime
		{
			get { return this.mDeathFadeTime_; }
			set { this.mDeathFadeTime_ = value; }
		}
		#endregion
		#region DeathFadeDelayTime
		float mDeathFadeDelayTime_;
		public float DeathFadeDelayTime
		{
			get { return this.mDeathFadeDelayTime_; }
			set { this.mDeathFadeDelayTime_ = value; }
		}
		#endregion
		#region TrainAnim
		string mTrainAnim_;
		[Meta.BAnimTypeReference]
		public string TrainAnim
		{
			get { return this.mTrainAnim_; }
			set { this.mTrainAnim_ = value; }
		}
		#endregion
		/// <remarks>Engine actually uses a fixed array that maps a BSquadMode to an AnimType</remarks>
		public Collections.BListArray<		BProtoObjectSquadModeAnim> SquadModeAnims { get; private set; }
			= new Collections.BListArray<	BProtoObjectSquadModeAnim>();
		#region RallyPoint
		BRallyPointType mRallyPoint_ = BRallyPointType.INVALID;
		public BRallyPointType RallyPoint
		{
			get { return this.mRallyPoint_; }
			set { this.mRallyPoint_ = value; }
		}
		#endregion
		#region MaxProjectileHeight
		float mMaxProjectileHeight_;
		public float MaxProjectileHeight
		{
			get { return this.mMaxProjectileHeight_; }
			set { this.mMaxProjectileHeight_ = value; }
		}
		#endregion
		#region GroundIKTilt

		float mGroundIkTiltFactor_;
		public float GroundIkTiltFactor
		{
			get { return this.mGroundIkTiltFactor_; }
			set { this.mGroundIkTiltFactor_ = value; }
		}

		string mGroundIkTiltBoneName_;
		public string GroundIkTiltBoneName
		{
			get { return this.mGroundIkTiltBoneName_; }
			set { this.mGroundIkTiltBoneName_ = value; }
		}

		bool HasGroundIkTiltData { get {
			return this.GroundIkTiltFactor > 0.0
				||
				this.GroundIkTiltBoneName.IsNotNullOrEmpty();
		} }
		#endregion
		#region DeathReplacement
		int mDeathReplacementId_ = TypeExtensions.K_NONE;
		[Meta.BProtoObjectReference]
		public int DeathReplacementId
		{
			get { return this.mDeathReplacementId_; }
			set { this.mDeathReplacementId_ = value; }
		}
		#endregion
		#region DeathSpawnSquad

		int mDeathSpawnSquadId_ = TypeExtensions.K_NONE;
		[Meta.BProtoSquadReference]
		public int DeathSpawnSquadId
		{
			get { return this.mDeathSpawnSquadId_; }
			set { this.mDeathSpawnSquadId_ = value; }
		}

		bool mDeathSpawnSquadCheckPosition_;
		public bool DeathSpawnSquadCheckPosition
		{
			get { return this.mDeathSpawnSquadCheckPosition_; }
			set { this.mDeathSpawnSquadCheckPosition_ = value; }
		}

		int mDeathSpawnSquadMaxPopCount_;
		public int DeathSpawnSquadMaxPopCount
		{
			get { return this.mDeathSpawnSquadMaxPopCount_; }
			set { this.mDeathSpawnSquadMaxPopCount_ = value; }
		}

		bool HasDeathSpawnSquadData { get {
			return this.mDeathSpawnSquadId_.IsNotNone()
				||
				this.mDeathSpawnSquadCheckPosition_
				||
				this.mDeathSpawnSquadMaxPopCount_ > 0;
		} }
		#endregion
		#region SurfaceType
		int mSurfaceType_ = TerrainTileType.C_UNDEFINED_INDEX;
		[Meta.TerrainTileTypeReference]
		public int SurfaceType
		{
			get { return this.mSurfaceType_; }
			set { this.mSurfaceType_ = value; }
		}
		#endregion
		#region Visual
		string mVisual_;
		[Meta.VisualReference]
		public string Visual
		{
			get { return this.mVisual_; }
			set { this.mVisual_ = value; }
		}
		#endregion
		#region CorpseDeath
		string mCorpseDeath_;
		[Meta.VisualReference]
		public string CorpseDeath
		{
			get { return this.mCorpseDeath_; }
			set { this.mCorpseDeath_ = value; }
		}
		#endregion
		#region AbilityCommandID
		int mAbilityCommandId_ = TypeExtensions.K_NONE;
		[Meta.BAbilityReference]
		public int AbilityCommandId
		{
			get { return this.mAbilityCommandId_; }
			set { this.mAbilityCommandId_ = value; }
		}
		#endregion
		#region PowerID
		int mPowerId_ = TypeExtensions.K_NONE;
		[Meta.BProtoPowerReference]
		public int PowerId
		{
			get { return this.mPowerId_; }
			set { this.mPowerId_ = value; }
		}
		#endregion
		[Meta.TriggerScriptReference]
		public List<string> AbilityTriggerScripts { get; private set; }
			= [];
		public Collections.BListExplicitIndex<		BProtoObjectVeterancy> Veterancy { get; private set; }
			= new Collections.BListExplicitIndex<	BProtoObjectVeterancy>(BProtoObjectVeterancy.KBListExplicitIndexParams);
		public Collections.BTypeValuesSingle AddResource { get; private set; }
			= new Collections.BTypeValuesSingle(BResource.KBListTypeValuesParams);
		#region ExistSoundBoneName
		string mExistSoundBoneName_;
		public string ExistSoundBoneName
		{
			get { return this.mExistSoundBoneName_; }
			set { this.mExistSoundBoneName_ = value; }
		}
		#endregion
		#region GathererLimit
		int mGathererLimit_ = -1;
		public int GathererLimit
		{
			get { return this.mGathererLimit_; }
			set { this.mGathererLimit_ = value; }
		}
		#endregion
		#region BlockMovementObjectID
		int mBlockMovementObjectId_ = TypeExtensions.K_NONE;
		[Meta.BProtoObjectReference]
		public int BlockMovementObjectId
		{
			get { return this.mBlockMovementObjectId_; }
			set { this.mBlockMovementObjectId_ = value; }
		}
		#endregion
		#region Lifespan
		float mLifespan_;
		public float Lifespan
		{
			get { return this.mLifespan_; }
			set { this.mLifespan_ = value; }
		}
		#endregion
		#region AmmoMax
		float mAmmoMax_;
		public float AmmoMax
		{
			get { return this.mAmmoMax_; }
			set { this.mAmmoMax_ = value; }
		}
		#endregion
		#region AmmoRegenRate
		float mAmmoRegenRate_;
		public float AmmoRegenRate
		{
			get { return this.mAmmoRegenRate_; }
			set { this.mAmmoRegenRate_ = value; }
		}
		#endregion
		#region NumConversions
		int mNumConversions_;
		public int NumConversions
		{
			get { return this.mNumConversions_; }
			set { this.mNumConversions_ = value; }
		}
		#endregion
		#region NumStasisFieldsToStop
		int mNumStasisFieldsToStop_ = 1;
		public int NumStasisFieldsToStop
		{
			get { return this.mNumStasisFieldsToStop_; }
			set { this.mNumStasisFieldsToStop_ = value; }
		}
		#endregion
		public Collections.BBitSet Flags { get; private set; }
			= new Collections.BBitSet(KFlagsParams);
		public Collections.BBitSet ObjectTypes { get; private set; }
			 = new Collections.BBitSet(KObjectTypesParams);
		public Collections.BListArray<		BProtoObjectDamageType> DamageTypes { get; private set; }
			= new Collections.BListArray<	BProtoObjectDamageType>();
		public Collections.BListArray<		BProtoObjectSound> Sounds { get; private set; }
			= new Collections.BListArray<	BProtoObjectSound>();
		public BTerrainImpactDecalHandle ImpactDecal { get; set; }
		#region ExtendedSoundBank
		string mExtendedSoundBank_;
		public string ExtendedSoundBank
		{
			get { return this.mExtendedSoundBank_; }
			set { this.mExtendedSoundBank_ = value; }
		}
		#endregion
		#region PortraitIcon
		string mPortraitIcon_;
		public string PortraitIcon
		{
			get { return this.mPortraitIcon_; }
			set { this.mPortraitIcon_ = value; }
		}
		#endregion
		#region Minimap

		string mMinimapIcon_;
		public string MinimapIcon
		{
			get { return this.mMinimapIcon_; }
			set { this.mMinimapIcon_ = value; }
		}

		float mMiniMapIconSize_ = 1.0f;
		public float MiniMapIconSize
		{
			get { return this.mMiniMapIconSize_; }
			set { this.mMiniMapIconSize_ = value; }
		}

		static BVector CDefaultMinimapColor { get { return new BVector(1.0f, 1.0f, 1.0f, 0.0f); } }
		BVector mMinimapColor_ = CDefaultMinimapColor;
		public BVector MinimapColor
		{
			get { return this.mMinimapColor_; }
			set { this.mMinimapColor_ = value; }
		}

		bool HasMiniMapIconData { get {
			return this.MinimapIcon.IsNotNullOrEmpty()
				||
				this.MiniMapIconSize != 1.0f;
		} }
		bool HasMinimapColorData { get { return this.MinimapColor != CDefaultMinimapColor; } }
		#endregion
		public Collections.BListArray<		BProtoObjectCommand> Commands { get; private set; }
			= new Collections.BListArray<	BProtoObjectCommand>();
		public Collections.BListArray<		BProtoObjectTrainLimit> TrainLimits { get; private set; }
			= new Collections.BListArray<	BProtoObjectTrainLimit>();
		#region GatherLink

		int mGatherLinkObjectType_ = TypeExtensions.K_NONE;
		[Meta.ObjectTypeReference]
		public int GatherLinkObjectType
		{
			get { return this.mGatherLinkObjectType_; }
			set { this.mGatherLinkObjectType_ = value; }
		}

		float mGatherLinkRadius_;
		public float GatherLinkRadius
		{
			get { return this.mGatherLinkRadius_; }
			set { this.mGatherLinkRadius_ = value; }
		}

		int mGatherLinkTarget_ = TypeExtensions.K_NONE;
		[Meta.ObjectTypeReference]
		public int GatherLinkTarget
		{
			get { return this.mGatherLinkTarget_; }
			set { this.mGatherLinkTarget_ = value; }
		}

		bool mGatherLinkSelf_;
		public bool GatherLinkSelf
		{
			get { return this.mGatherLinkSelf_; }
			set { this.mGatherLinkSelf_ = value; }
		}

		bool HasGatherLinkData { get {
			return this.GatherLinkObjectType.IsNotNone()
				||
				this.GatherLinkRadius > 0.0f
				||
				this.GatherLinkTarget.IsNotNone()
				||
				this.GatherLinkSelf;
		} }
		#endregion
		public Collections.BListArray<		BProtoObjectChildObject> ChildObjects { get; private set; }
			= new Collections.BListArray<	BProtoObjectChildObject>();
		public Collections.BTypeValuesSingle Populations { get; private set; }
			= new Collections.BTypeValuesSingle(BPopulation.KBListParamsSingle);
		public Collections.BTypeValuesSingle PopulationsCapAddition { get; private set; }
			= new Collections.BTypeValuesSingle(BPopulation.KBListParamsSingle);
		#region Tactics
		int mTactics_ = TypeExtensions.K_NONE;
		[Meta.BTacticDataReference]
		public int Tactics
		{
			get { return this.mTactics_; }
			set { this.mTactics_ = value; }
		}
		#endregion
		#region FlightLevel
		const float C_DEFAULT_FLIGHT_LEVEL_ = 10.0f;

		float mFlightLevel_ = C_DEFAULT_FLIGHT_LEVEL_;
		/// <summary>relative Y displacement of the object</summary>
		public float FlightLevel
		{
			get { return this.mFlightLevel_; }
			set { this.mFlightLevel_ = value; }
		}
		#endregion
		#region ExitFromDirection
		int mExitFromDirection_ = (int)BProtoObjectExitDirection.FROM_FRONT;
		public int ExitFromDirection
		{
			get { return this.mExitFromDirection_; }
			set { this.mExitFromDirection_ = value; }
		}
		#endregion
		#region HPBar

		// #TODO this needs to be an actual ID
		string mHpBarId_;
		public string HpBarId
		{
			get { return this.mHpBarId_; }
			set { this.mHpBarId_ = value; }
		}

		BVector mHpBarSize_;
		public BVector HpBarSize
		{
			get { return this.mHpBarSize_; }
			set { this.mHpBarSize_ = value; }
		}

		BVector mHpBarOffset_;
		public BVector HpBarOffset
		{
			get { return this.mHpBarOffset_; }
			set { this.mHpBarOffset_ = value; }
		}

		bool HasHpBarData { get {
			return this.mHpBarId_.IsNotNullOrEmpty()
				|| PhxPredicates.IsNotZero(this.HpBarSize)
				|| PhxPredicates.IsNotZero(this.HpBarOffset);
		} }
		#endregion
		public Collections.BListArray<		BHitZone> HitZones { get; private set; }
			= new Collections.BListArray<	BHitZone>();
		#region BeamHead
		int mBeamHead_ = TypeExtensions.K_NONE;
		[Meta.ObjectTypeReference]
		public int BeamHead
		{
			get { return this.mBeamHead_; }
			set { this.mBeamHead_ = value; }
		}
		#endregion
		#region BeamTail
		int mBeamTail_ = TypeExtensions.K_NONE;
		[Meta.ObjectTypeReference]
		public int BeamTail
		{
			get { return this.mBeamTail_; }
			set { this.mBeamTail_ = value; }
		}
		#endregion
		#region Level
		int mLevel_;
		public int Level
		{
			get { return this.mLevel_; }
			set { this.mLevel_ = value; }
		}
		#endregion
		#region LevelUpEffect
		int mLevelUpEffect_ = TypeExtensions.K_NONE;
		[Meta.BProtoObjectReference]
		public int LevelUpEffect
		{
			get { return this.mLevelUpEffect_; }
			set { this.mLevelUpEffect_ = value; }
		}
		#endregion
		#region RecoveringEffect
		int mRecoveringEffect_ = TypeExtensions.K_NONE;
		[Meta.BProtoObjectReference]
		public int RecoveringEffect
		{
			get { return this.mRecoveringEffect_; }
			set { this.mRecoveringEffect_ = value; }
		}
		#endregion
		#region AutoTrainOnBuilt
		int mAutoTrainOnBuilt_ = TypeExtensions.K_NONE;
		[Meta.BProtoSquadReference]
		public int AutoTrainOnBuilt
		{
			get { return this.mAutoTrainOnBuilt_; }
			set { this.mAutoTrainOnBuilt_ = value; }
		}
		#endregion
		#region Socket

		int mSocketId_ = TypeExtensions.K_NONE;
		[Meta.ObjectTypeReference]
		public int SocketId
		{
			get { return this.mSocketId_; }
			set { this.mSocketId_ = value; }
		}

		BPlayerScope mSocketPlayerScope_ = BPlayerScope.PLAYER;
		public BPlayerScope SocketPlayerScope
		{
			get { return this.mSocketPlayerScope_; }
			set { this.mSocketPlayerScope_ = value; }
		}

		bool mAutoSocket_;
		public bool AutoSocket
		{
			get { return this.mAutoSocket_; }
			set { this.mAutoSocket_ = value; }
		}

		bool HasSocketData { get {
			return this.SocketId.IsNotNone()
				||
				this.SocketPlayerScope != BPlayerScope.PLAYER
				||
				this.AutoSocket;
		} }
		#endregion
		#region Rate

		int mRateId_ = TypeExtensions.K_NONE;
		[Meta.RateReference]
		public int RateId
		{
			get { return this.mRateId_; }
			set { this.mRateId_ = value; }
		}

		float mRateAmount_;
		public float RateAmount
		{
			get { return this.mRateAmount_; }
			set { this.mRateAmount_ = value; }
		}

		bool HasRateData { get {
			return this.RateId.IsNotNone()
				||
				this.RateAmount > 0.0f;
		} }
		#endregion
		#region MaxContained
		int mMaxContained_;
		public int MaxContained
		{
			get { return this.mMaxContained_; }
			set { this.mMaxContained_ = value; }
		}
		#endregion
		#region MaxFlameEffects
		int mMaxFlameEffects_ = TypeExtensions.K_NONE;
		public int MaxFlameEffects
		{
			get { return this.mMaxFlameEffects_; }
			set { this.mMaxFlameEffects_ = value; }
		}
		#endregion
		[Meta.ObjectTypeReference]
		public List<	BProtoObjectID> Contains { get; private set; }
			= [];
		#region GarrisonSquadMode
		BSquadMode mGarrisonSquadMode_ = BSquadMode.INVALID;
		public BSquadMode GarrisonSquadMode
		{
			get { return this.mGarrisonSquadMode_; }
			set { this.mGarrisonSquadMode_ = value; }
		}
		#endregion
		#region BuildStatsObjectID
		int mBuildStatsObjectId_ = TypeExtensions.K_NONE;
		[Meta.BProtoObjectReference]
		public int BuildStatsObjectId
		{
			get { return this.mBuildStatsObjectId_; }
			set { this.mBuildStatsObjectId_ = value; }
		}
		#endregion
		#region SubSelectSort
		int mSubSelectSort_ = int.MaxValue;
		public int SubSelectSort
		{
			get { return this.mSubSelectSort_; }
			set { this.mSubSelectSort_ = value; }
		}
		#endregion
		#region AttackGradeDPS
		float mAttackGradeDps_;
		public float AttackGradeDps
		{
			get { return this.mAttackGradeDps_; }
			set { this.mAttackGradeDps_ = value; }
		}
		#endregion
		#region RamDodgeFactor
		float mRamDodgeFactor_;
		public float RamDodgeFactor
		{
			get { return this.mRamDodgeFactor_; }
			set { this.mRamDodgeFactor_ = value; }
		}
		#endregion
		public BRumbleEvent HoveringRumble { get; set; }
		#region VisualDisplayPriority
		BVisualDisplayPriority mVisualDisplayPriority_ = BVisualDisplayPriority.NORMAL;
		public BVisualDisplayPriority VisualDisplayPriority
		{
			get { return this.mVisualDisplayPriority_; }
			set { this.mVisualDisplayPriority_ = value; }
		}
		#endregion
		#region ChildObjectDamageTakenScalar
		float mChildObjectDamageTakenScalar_;
		public float ChildObjectDamageTakenScalar
		{
			get { return this.mChildObjectDamageTakenScalar_; }
			set { this.mChildObjectDamageTakenScalar_ = value; }
		}
		#endregion
		#region TrueLOSHeight
		const float C_DEFAULT_TRUE_LOS_HEIGHT_ = 3.0f;

		float mTrueLosHeight_ = C_DEFAULT_TRUE_LOS_HEIGHT_;
		public float TrueLosHeight
		{
			get { return this.mTrueLosHeight_; }
			set { this.mTrueLosHeight_ = value; }
		}
		#endregion
		#region GarrisonTime
		float mGarrisonTime_;
		public float GarrisonTime
		{
			get { return this.mGarrisonTime_; }
			set { this.mGarrisonTime_ = value; }
		}
		#endregion
		#region BuildRotation
		float mBuildRotation_;
		public float BuildRotation
		{
			get { return this.mBuildRotation_; }
			set { this.mBuildRotation_ = value; }
		}
		#endregion
		#region BuildOffset
		BVector mBuildOffset_;
		public BVector BuildOffset
		{
			get { return this.mBuildOffset_; }
			set { this.mBuildOffset_ = value; }
		}
		#endregion
		#region AutoParkingLot

		int mAutoParkingLotObjectId_ = TypeExtensions.K_NONE;
		[Meta.BProtoObjectReference]
		public int AutoParkingLotObjectId
		{
			get { return this.mAutoParkingLotObjectId_; }
			set { this.mAutoParkingLotObjectId_ = value; }
		}

		float mAutoParkingLotRotation_;
		public float AutoParkingLotRotation
		{
			get { return this.mAutoParkingLotRotation_; }
			set { this.mAutoParkingLotRotation_ = value; }
		}

		BVector mAutoParkingLotOffset_;
		public BVector AutoParkingLotOffset
		{
			get { return this.mAutoParkingLotOffset_; }
			set { this.mAutoParkingLotOffset_ = value; }
		}

		bool HasAutoParkingLotData { get {
			return this.AutoParkingLotObjectId.IsNotNone()
				||
				this.AutoParkingLotRotation != 0.0
				|| PhxPredicates.IsNotZero(this.AutoParkingLotOffset);
		} }
		#endregion
		#region BuildingStrengthID
		// #TODO this needs to be an actual ID

		string mBuildingStrengthId_;
		public string BuildingStrengthId
		{
			get { return this.mBuildingStrengthId_; }
			set { this.mBuildingStrengthId_ = value; }
		}
		#endregion
		#region ShieldType
		int mShieldType_ = TypeExtensions.K_NONE;
		[Meta.ObjectTypeReference]
		public int ShieldType
		{
			get { return this.mShieldType_; }
			set { this.mShieldType_ = value; }
		}
		#endregion
		#region RevealRadius
		float mRevealRadius_;
		public float RevealRadius
		{
			get { return this.mRevealRadius_; }
			set { this.mRevealRadius_ = value; }
		}
		#endregion
		#region TargetBeam
		int mTargetBeam_ = TypeExtensions.K_NONE;
		[Meta.BProtoObjectReference]
		public int TargetBeam
		{
			get { return this.mTargetBeam_; }
			set { this.mTargetBeam_ = value; }
		}
		#endregion
		#region KillBeam
		int mKillBeam_ = TypeExtensions.K_NONE;
		[Meta.BProtoObjectReference]
		public int KillBeam
		{
			get { return this.mKillBeam_; }
			set { this.mKillBeam_ = value; }
		}
		#endregion
		#region MinimapIconName (EDITOR ONLY)
		string mMinimapIconName_;
		public string MinimapIconName
		{
			get { return this.mMinimapIconName_; }
			set { this.mMinimapIconName_ = value; }
		}
		#endregion

		public BProtoObject() : base(BResource.KBListTypeValuesParams, BResource.KBListTypeValuesXmlParamsCost)
		{
			var textData = this.CreateDatabaseObjectUserInterfaceTextData();
			textData.HasDisplayNameId = true;
			textData.HasRolloverTextId = true;
			textData.HasStatsNameId = true;
			textData.HasGaiaRolloverTextId = true;
			textData.HasEnemyRolloverTextId = true;
			textData.HasPrereqTextId = true;
			textData.HasRoleTextId = true;
		}

		#region ITagElementStreamable<string> Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);
			var xs = s.GetSerializerInterface();

			s.StreamAttributeOpt("is", ref this.mUnusedIs_, Predicates.IsNotNone);
			s.StreamAttributeOpt("id", ref this.mUnusedId_, Predicates.IsNotNone);
			s.StreamAttributeOpt("update", ref this.mUpdate_, Predicates.IsTrue);
			s.StreamElementEnumOpt("MovementType", ref this.mMovementType_, e => e != BProtoObjectMovementType.NONE);
			XML.XmlUtil.Serialize(s, this.Hardpoints, BHardpoint.KBListXmlParams);
			s.StreamElements("SingleBoneIK", this.SingleBoneIKs, xs, XML.BXmlSerializerInterface.StreamStringValue, dummy => (string)null);
			XML.XmlUtil.Serialize(s, this.GroundIKs, BGroundIkNode.KBListXmlParams);
			XML.XmlUtil.Serialize(s, this.SweetSpotIKs, BSweetSpotIkNode.KBListXmlParams);
			#region ObstructionRadius
			s.StreamElementOpt("ObstructionRadiusX", ref this.mObstructionRadius_.X, Predicates.IsNotZero);
			s.StreamElementOpt("ObstructionRadiusY", ref this.mObstructionRadius_.Y, Predicates.IsNotZero);
			s.StreamElementOpt("ObstructionRadiusZ", ref this.mObstructionRadius_.Z, Predicates.IsNotZero);
			#endregion
			#region TerrainFlatten
			s.StreamElementOpt("FlattenMinX0", ref this.mTerrainFlattennMin0_.X, Predicates.IsNotZero);
			s.StreamElementOpt("FlattenMaxX0", ref this.mTerrainFlattennMax0_.X, Predicates.IsNotZero);
			s.StreamElementOpt("FlattenMinZ0", ref this.mTerrainFlattennMin0_.Z, Predicates.IsNotZero);
			s.StreamElementOpt("FlattenMaxZ0", ref this.mTerrainFlattennMax0_.Z, Predicates.IsNotZero);

			s.StreamElementOpt("FlattenMinX1", ref this.mTerrainFlattennMin1_.X, Predicates.IsNotZero);
			s.StreamElementOpt("FlattenMaxX1", ref this.mTerrainFlattennMax1_.X, Predicates.IsNotZero);
			s.StreamElementOpt("FlattenMinZ1", ref this.mTerrainFlattennMin1_.Z, Predicates.IsNotZero);
			s.StreamElementOpt("FlattenMaxZ1", ref this.mTerrainFlattennMax1_.Z, Predicates.IsNotZero);
			#endregion
			#region Parking lot
			s.StreamElementOpt("ParkingMinX", ref this.mParkingLotMin_.X, Predicates.IsNotZero);
			s.StreamElementOpt("ParkingMaxX", ref this.mParkingLotMax_.X, Predicates.IsNotZero);
			s.StreamElementOpt("ParkingMinZ", ref this.mParkingLotMin_.Z, Predicates.IsNotZero);
			s.StreamElementOpt("ParkingMaxZ", ref this.mParkingLotMax_.Z, Predicates.IsNotZero);
			#endregion
			s.StreamElementOpt("TerrainHeightTolerance", ref this.mTerrainHeightTolerance_, f => f != C_DEFAULT_TERRAIN_HEIGHT_TOLERANCE_);
			#region Physics
			s.StreamElementOpt("PhysicsInfo", ref this.mPhysicsInfo_, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("PhysicsReplacementInfo", ref this.mPhysicsReplacementInfo_, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("Velocity", ref this.mVelocity_, Predicates.IsNotZero);
			s.StreamElementOpt("MaxVelocity", ref this.mMaxVelocity_, Predicates.IsNotZero);
			s.StreamElementOpt(K_XML_ELEMENT_REVERSE_SPEED, ref this.mReverseSpeed_, f => f != C_DEFAULT_REVERSE_SPEED_);
			s.StreamElementOpt("Acceleration", ref this.mAcceleration_, Predicates.IsNotZero);
			s.StreamElementOpt("TrackingDelay", ref this.mTrackingDelay_, Predicates.IsNotZero);
			s.StreamElementOpt("StartingVelocity", ref this.mStartingVelocity_, Predicates.IsNotZero);
			#endregion
			s.StreamElementOpt("Fuel", ref this.mFuel_, Predicates.IsNotZero);
			#region Perturbance
			s.StreamElementOpt("PerturbanceChance", ref this.mPerturbanceChance_, Predicates.IsNotZero);
			s.StreamElementOpt("PerturbanceVelocity", ref this.mPerturbanceVelocity_, Predicates.IsNotZero);
			s.StreamElementOpt("PerturbanceMinTime", ref this.mPerturbanceMinTime_, Predicates.IsNotZero);
			s.StreamElementOpt("PerturbanceMaxTime", ref this.mPerturbanceMaxTime_, Predicates.IsNotZero);
			using (var bm = s.EnterCursorBookmarkOpt("PerturbInitialVelocity", this, v => v.HasInitialPerturbanceData)) if (bm.IsNotNull)
			{
				s.StreamCursor(ref this.mPerturbInitialVelocity_);
				s.StreamAttributeOpt("minTime", ref this.mInitialPerturbanceMinTime_, Predicates.IsNotZero);
				s.StreamAttributeOpt("maxTime", ref this.mInitialPerturbanceMaxTime_, Predicates.IsNotZero);
			}
			#endregion
			#region ActiveScan
			using (var bm = s.EnterCursorBookmarkOpt("ActiveScanChance", this, v => v.HasActiveScanData)) if (bm.IsNotNull)
			{
				s.StreamCursor(ref this.mActiveScanChance_);
				s.StreamAttributeOpt("radiusScale", ref this.mActiveScanRadiusScale_, Predicates.IsNotZero);
			}
			#endregion
			s.StreamElementOpt("TurnRate", ref this.mTurnRate_, Predicates.IsNotZero);
			s.StreamElementOpt("Hitpoints", ref this.mHitpoints_, Predicates.IsNotZero);
			#region Shieldpoints
			{
				bool streamedShieldpoints = s.StreamElementOpt("Shieldpoints", ref this.mShieldpoints_, Predicates.IsNotZero);
				// #HACK fucking deal with original HW game data that was hand edited, but only when reading
				if (s.IsReading && !streamedShieldpoints)
					s.StreamElementOpt("ShieldPoints", ref this.mShieldpoints_, Predicates.IsNotZero);
			}
			#endregion
			s.StreamElementOpt("LOS", ref this.mLos_, Predicates.IsNotZero);
			#region Pick and Select
			s.StreamElementOpt("PickRadius", ref this.mPickRadius_, Predicates.IsNotZero);
			s.StreamElementOpt("PickOffset", ref this.mPickOffset_, Predicates.IsNotZero);
			s.StreamElementEnumOpt("PickPriority", ref this.mPickPriority_, e => e != BPickPriority.NONE);
			s.StreamElementEnumOpt("SelectType", ref this.mSelectType_, e => e != BProtoObjectSelectType.NONE);
			s.StreamElementEnumOpt("GotoType", ref this.mGotoType_, e => e != BGotoType.NONE);
			s.StreamElementOpt("SelectedRadiusX", ref this.mSelectedRadius_.X, Predicates.IsNotZero);
			s.StreamElementOpt("SelectedRadiusZ", ref this.mSelectedRadius_.Z, Predicates.IsNotZero);
			#endregion
			s.StreamElementOpt("RepairPoints", ref this.mRepairPoints_, Predicates.IsNotZero);
			s.StreamElementEnumOpt("ObjectClass", ref this.mClassType_, x => x != BProtoObjectClassType.OBJECT);
			#region TrainerType
			using (var bm = s.EnterCursorBookmarkOpt("TrainerType", this, v => v.HasTrainerTypeData)) if (bm.IsNotNull)
			{
				s.StreamCursor(ref this.mTrainerType_);
				s.StreamAttributeOpt("ApplyFormation", ref this.mTrainerApplyFormation_, Predicates.IsTrue);
			}
			#endregion
			s.StreamElementEnumOpt("AutoLockDown", ref this.mAutoLockDown_, e => e != BAutoLockDown.NONE);
			s.StreamElementOpt("CostEscalation", ref this.mCostEscalation_, PhxPredicates.IsNotOne);
			s.StreamElements("CostEscalationObject", this.CostEscalationObjects, xs, XML.BXmlSerializerInterface.StreamObjectId);
			XML.XmlUtil.Serialize(s, this.CaptureCosts, BProtoObjectCaptureCost.KBListXmlParams);
			s.StreamElementOpt("Bounty", ref this.mBounty_, Predicates.IsNotZero);
			s.StreamElementOpt("AIAssetValueAdjust", ref this.mAiAssetValueAdjust_, Predicates.IsNotZero);
			s.StreamElementOpt("CombatValue", ref this.mCombatValue_, Predicates.IsNotZero);
			s.StreamElementOpt("ResourceAmount", ref this.mResourceAmount_, Predicates.IsNotZero);
			s.StreamElementOpt("PlacementRules", ref this.mPlacementRules_, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("DeathFadeTime", ref this.mDeathFadeTime_, PhxPredicates.IsNotOne);
			s.StreamElementOpt("DeathFadeDelayTime", ref this.mDeathFadeDelayTime_, Predicates.IsNotZero);
			s.StreamElementOpt("TrainAnim", ref this.mTrainAnim_, Predicates.IsNotNullOrEmpty);
			XML.XmlUtil.Serialize(s, this.SquadModeAnims, BProtoObjectSquadModeAnim.KBListXmlParams);
			s.StreamElementEnumOpt("RallyPoint", ref this.mRallyPoint_, x => x != BRallyPointType.INVALID);
			s.StreamElementOpt("MaxProjectileHeight", ref this.mMaxProjectileHeight_, Predicates.IsNotZero);
			#region GroundIKTilt
			using (var bm = s.EnterCursorBookmarkOpt("GroundIKTilt", this, v => v.HasGroundIkTiltData)) if (bm.IsNotNull)
			{
				s.StreamCursor(ref this.mGroundIkTiltBoneName_);
				s.StreamAttributeOpt("factor", ref this.mGroundIkTiltFactor_, Predicates.IsNotZero);
			}
			#endregion
			xs.StreamDbid(s, "DeathReplacement", ref this.mDeathReplacementId_, DatabaseObjectKind.OBJECT);
			#region DeathSpawnSquad
			using (var bm = s.EnterCursorBookmarkOpt("DeathSpawnSquad", this, v => v.HasDeathSpawnSquadData)) if (bm.IsNotNull)
			{
				xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mDeathSpawnSquadId_, DatabaseObjectKind.SQUAD, xmlSource: XML.XmlUtil.K_SOURCE_CURSOR);

				// #NOTE engine streams this as CheckPos, but it is also case insensitive
				const string kCheckPosName = "checkPos";
				// #NOTE the engine interprets the presence of this attribute as true
				if (s.IsReading)
				{
					this.mDeathSpawnSquadCheckPosition_ = s.AttributeExists(kCheckPosName);
				}
				else if (s.IsWriting)
				{
					if (this.mDeathSpawnSquadCheckPosition_)
						s.WriteAttribute(kCheckPosName, true);
				}

				s.StreamAttributeOpt("MaxPopCount", ref this.mDeathSpawnSquadMaxPopCount_, Predicates.IsNotZero);
			}
			#endregion
			xs.StreamDbid(s, "SurfaceType", ref this.mSurfaceType_, DatabaseObjectKind.TERRAIN_TILE_TYPE);
			s.StreamElementOpt("Visual", ref this.mVisual_, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("CorpseDeath", ref this.mCorpseDeath_, Predicates.IsNotNullOrEmpty);
			xs.StreamDbid(s, "AbilityCommand", ref this.mAbilityCommandId_, DatabaseObjectKind.ABILITY);
			xs.StreamDbid(s, "Power", ref this.mPowerId_, DatabaseObjectKind.POWER);
			s.StreamElements("Ability", this.AbilityTriggerScripts, xs, XML.BXmlSerializerInterface.StreamStringValue, dummy => (string)null);
			XML.XmlUtil.Serialize(s, this.Veterancy, BProtoObjectVeterancy.KBListExplicitIndexXmlParams);
			XML.XmlUtil.Serialize(s, this.AddResource, BResource.KBListTypeValuesXmlParamsAddResource, "Amount");
			#region ExistSound
			using (var bm = s.EnterCursorBookmarkOpt("ExistSound", this.mExistSoundBoneName_, Predicates.IsNotNullOrEmpty)) if (bm.IsNotNull)
			{
				s.StreamAttribute("bone", ref this.mExistSoundBoneName_);
			}
			#endregion
			s.StreamElementOpt("GathererLimit", ref this.mGathererLimit_, Predicates.IsNotNone);
			xs.StreamDbid(s, "BlockMovementObject", ref this.mBlockMovementObjectId_, DatabaseObjectKind.OBJECT);
			s.StreamElementOpt("Lifespan", ref this.mLifespan_, Predicates.IsNotZero);
			s.StreamElementOpt("AmmoMax", ref this.mAmmoMax_, Predicates.IsNotZero);
			s.StreamElementOpt("AmmoRegenRate", ref this.mAmmoRegenRate_, Predicates.IsNotZero);
			s.StreamElementOpt("NumConversions", ref this.mNumConversions_, Predicates.IsNotZero);
			s.StreamElementOpt("NumStasisFieldsToStop", ref this.mNumStasisFieldsToStop_, PhxPredicates.IsNotOne);
			XML.XmlUtil.Serialize(s, this.Flags, XML.BBitSetXmlParams.KFlagsSansRoot);
			XML.XmlUtil.Serialize(s, this.ObjectTypes, KObjectTypesXmlParams);
			XML.XmlUtil.Serialize(s, this.DamageTypes, BProtoObjectDamageType.KBListXmlParams);
			XML.XmlUtil.Serialize(s, this.Sounds, BProtoObjectSound.KBListXmlParams);
			#region ImpactDecal
			using (var bm = s.EnterCursorBookmarkOpt(BTerrainImpactDecalHandle.KBListXmlParams.elementName, this.ImpactDecal, Predicates.IsNotNull)) if (bm.IsNotNull)
			{
				if (s.IsReading)
					this.ImpactDecal = new BTerrainImpactDecalHandle();

				this.ImpactDecal.Serialize(s);
			}
			#endregion
			s.StreamElementOpt("ExtendedSoundBank", ref this.mExtendedSoundBank_, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("PortraitIcon", ref this.mPortraitIcon_, Predicates.IsNotNullOrEmpty);
			#region Minimap
			using (var bm = s.EnterCursorBookmarkOpt("MinimapIcon", this, v => v.HasMiniMapIconData)) if (bm.IsNotNull)
			{
				s.StreamCursor(ref this.mMinimapIcon_);
				s.StreamAttributeOpt("size", ref this.mMiniMapIconSize_, PhxPredicates.IsNotOne);
			}
			using (var bm = s.EnterCursorBookmarkOpt("MinimapColor", this, v => v.HasMinimapColorData)) if (bm.IsNotNull)
			{
				// #NOTE we use IsNotZero here instead of IsNotOne (for cDefaultMinimapColor)
				// because when loading the game defaults the temp rgb values to 0 and then sets
				// the final game data to those values (so excluding red would mean it is zero).
				// #NOTE the engine parses these names in lowercase, but actual data uses uppercase
				s.StreamAttributeOpt("Red", ref this.mMinimapColor_.X, Predicates.IsNotZero);
				s.StreamAttributeOpt("Green", ref this.mMinimapColor_.Y, Predicates.IsNotZero);
				s.StreamAttributeOpt("Blue", ref this.mMinimapColor_.Z, Predicates.IsNotZero);
			}
			#endregion
			XML.XmlUtil.Serialize(s, this.Commands, BProtoObjectCommand.KBListXmlParams);
			XML.XmlUtil.Serialize(s, this.TrainLimits, BProtoObjectTrainLimit.KBListXmlParams);
			#region GatherLink
			using (var bm = s.EnterCursorBookmarkOpt("GatherLink", this, v => v.HasGatherLinkData)) if (bm.IsNotNull)
			{
				xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mGatherLinkObjectType_, DatabaseObjectKind.OBJECT_TYPE, isOptional: false, xmlSource: XML.XmlUtil.K_SOURCE_CURSOR);
				s.StreamAttributeOpt("Radius", ref this.mGatherLinkRadius_, Predicates.IsNotZero);
				xs.StreamDbid(s, "Target", ref this.mGatherLinkObjectType_, DatabaseObjectKind.OBJECT_TYPE, isOptional: false, xmlSource: XML.XmlUtil.K_SOURCE_ATTR);
				s.StreamAttributeOpt("Self", ref this.mGatherLinkSelf_, Predicates.IsTrue);
			}
			#endregion
			XML.XmlUtil.Serialize(s, this.ChildObjects, BProtoObjectChildObject.KBListXmlParams);
			XML.XmlUtil.Serialize(s, this.Populations, BPopulation.KBListXmlParamsSingle);
			XML.XmlUtil.Serialize(s, this.PopulationsCapAddition, BPopulation.KBListXmlParamsSingleCapAddition);
			xs.StreamTactic(s, "Tactics", ref this.mTactics_);
			s.StreamElementOpt("FlightLevel", ref this.mFlightLevel_, f => f != C_DEFAULT_FLIGHT_LEVEL_);
			s.StreamElementOpt("ExitFromDirection", ref this.mExitFromDirection_, Predicates.IsNotZero);
			#region HPBar
			using (var bm = s.EnterCursorBookmarkOpt("HPBar", this, v => v.HasHpBarData)) if (bm.IsNotNull)
			{
				s.StreamCursor(ref this.mHpBarId_);
				s.StreamAttributeOpt("sizeX", ref this.mHpBarSize_.X, Predicates.IsNotZero);
				s.StreamAttributeOpt("sizeY", ref this.mHpBarSize_.Y, Predicates.IsNotZero);
				s.StreamBVector("offset", ref this.mHpBarOffset_, xmlSource: XML.XmlUtil.K_SOURCE_ATTR);
			}
			#endregion
			XML.XmlUtil.Serialize(s, this.HitZones, BHitZone.KBListXmlParams);
			xs.StreamDbid(s, "BeamHead", ref this.mBeamHead_, DatabaseObjectKind.UNIT);
			xs.StreamDbid(s, "BeamTail", ref this.mBeamTail_, DatabaseObjectKind.UNIT);
			s.StreamElementOpt("Level", ref this.mLevel_, Predicates.IsNotZero);
			xs.StreamDbid(s, "LevelUpEffect", ref this.mLevelUpEffect_, DatabaseObjectKind.OBJECT);
			xs.StreamDbid(s, "RecoveringEffect", ref this.mRecoveringEffect_, DatabaseObjectKind.OBJECT);
			xs.StreamDbid(s, "AutoTrainOnBuilt", ref this.mAutoTrainOnBuilt_, DatabaseObjectKind.SQUAD);
			#region Socket
			using (var bm = s.EnterCursorBookmarkOpt("Socket", this, v => v.HasSocketData)) if (bm.IsNotNull)
			{
				xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mSocketId_, DatabaseObjectKind.OBJECT_TYPE, isOptional: false, xmlSource: XML.XmlUtil.K_SOURCE_CURSOR);
				// #NOTE engine reads this Player in lower case, but actual uses pascal case
				s.StreamAttributeEnumOpt("Player", ref this.mSocketPlayerScope_, e => e != BPlayerScope.PLAYER);
				s.StreamAttributeOpt("AutoSocket", ref this.mAutoSocket_, Predicates.IsTrue);
			}
			#endregion
			#region Rate
			using (var bm = s.EnterCursorBookmarkOpt(BGameData.KRatesBListTypeValuesXmlParams.elementName, this, v => v.HasRateData)) if (bm.IsNotNull)
			{
				// #NOTE engine reads Rate as lower case, but actual data is in pascal case
				xs.StreamTypeName(s, BGameData.KRatesBListTypeValuesXmlParams.dataName, ref this.mRateId_, GameDataObjectKind.RATE, isOptional: false, xmlSource: XML.XmlUtil.K_SOURCE_ATTR);
				s.StreamCursor(ref this.mRateAmount_);
			}
			#endregion
			s.StreamElementOpt("MaxContained", ref this.mMaxContained_, Predicates.IsNotZero);
			s.StreamElementOpt("MaxFlameEffects", ref this.mMaxFlameEffects_, Predicates.IsNotNone);
			s.StreamElements("Contain", this.Contains, xs, XML.BXmlSerializerInterface.StreamObjectType);
			s.StreamElementEnumOpt("GarrisonSquadMode", ref this.mGarrisonSquadMode_, e => e != BSquadMode.INVALID);
			xs.StreamDbid(s, "BuildStatsObject", ref this.mBuildStatsObjectId_, DatabaseObjectKind.OBJECT);
			s.StreamElementOpt("SubSelectSort", ref this.mSubSelectSort_, v => v != int.MaxValue);
			s.StreamElementOpt(K_XML_ELEMENT_ATTACK_GRADE_DPS, ref this.mAttackGradeDps_, Predicates.IsNotZero);
			s.StreamElementOpt("RamDodgeFactor", ref this.mRamDodgeFactor_, Predicates.IsNotZero);
			#region HoveringRumble
			using (var bm = s.EnterCursorBookmarkOpt("HoveringRumble", this.HoveringRumble, Predicates.IsNotNull)) if (bm.IsNotNull)
			{
				if (s.IsReading)
					this.HoveringRumble = new BRumbleEvent();

				this.HoveringRumble.Serialize(s);
			}
			#endregion
			s.StreamElementEnumOpt("VisualDisplayPriority", ref this.mVisualDisplayPriority_, e => e != BVisualDisplayPriority.NORMAL);
			s.StreamElementOpt("ChildObjectDamageTakenScalar", ref this.mChildObjectDamageTakenScalar_, Predicates.IsNotZero);
			s.StreamElementOpt("TrueLOSHeight", ref this.mTrueLosHeight_, f => f != C_DEFAULT_TRUE_LOS_HEIGHT_);
			s.StreamElementOpt("GarrisonTime", ref this.mGarrisonTime_, Predicates.IsNotZero);
			s.StreamElementOpt("BuildRotation", ref this.mBuildRotation_, Predicates.IsNotZero);
			s.StreamBVector("BuildOffset", ref this.mBuildOffset_);
			#region AutoParkingLot
			using (var bm = s.EnterCursorBookmarkOpt("AutoParkingLot", this, v => v.HasAutoParkingLotData)) if (bm.IsNotNull)
			{
				xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mAutoParkingLotObjectId_, DatabaseObjectKind.OBJECT, isOptional: false, xmlSource: XML.XmlUtil.K_SOURCE_CURSOR);
				s.StreamAttributeOpt("Rotation", ref this.mAutoParkingLotRotation_, Predicates.IsNotZero);
				s.StreamBVector("Offset", ref this.mAutoParkingLotOffset_, xmlSource: XML.XmlUtil.K_SOURCE_ATTR);
			}
			#endregion
			s.StreamElementOpt("BuildingStrengthDisplay", ref this.mBuildingStrengthId_, Predicates.IsNotNullOrEmpty);
			xs.StreamDbid(s, "ShieldType", ref this.mShieldType_, DatabaseObjectKind.UNIT);
			s.StreamElementOpt("RevealRadius", ref this.mRevealRadius_, Predicates.IsNotZero);
			xs.StreamDbid(s, "TargetBeam", ref this.mTargetBeam_, DatabaseObjectKind.OBJECT);
			xs.StreamDbid(s, "KillBeam", ref this.mKillBeam_, DatabaseObjectKind.OBJECT);
			s.StreamElementOpt("MinimapIconName", ref this.mMinimapIconName_, Predicates.IsNotNullOrEmpty);

			if (s.IsReading)
			{
				this.PostDeserialize();
			}
		}

		private void PostDeserialize()
		{
			if (sortCommandsAfterReading)
			{
				this.SortCommands();
			}

			this.SquadModeAnims.Sort();
		}
		#endregion

		public static bool sortCommandsAfterReading = false;
		private void SortCommands()
		{
			this.Commands.Sort(CompareCommands);
		}

		private static int CompareCommands(BProtoObjectCommand x, BProtoObjectCommand y)
		{
			if (x.Position != y.Position)
				return x.Position.CompareTo(y.Position);

			if (x.CommandType != y.CommandType)
				return ((int)x.CommandType).CompareTo((int)y.CommandType);

			// assuming Proto upgrades are defined after earlier Protos
			return x.Id.CompareTo(y.Id);
		}
	};
}
