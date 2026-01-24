//SQUAD_NEEDS_ToLowerDataNames

using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.Phx
{
	/* Deprecated fields:
	 * - MinimapIcon: This is in ProtoObject now.
	*/

	public sealed class BProtoSquad
		: DatabaseIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams("Squad")
		{
			dataName = K_XML_ATTR_NAME,
			flags = 0
#if SQUAD_NEEDS_ToLowerDataNames
				| XML.BCollectionXmlParamsFlags.ToLowerDataNames |
#endif
				| XML.BCollectionXmlParamsFlags.REQUIRES_DATA_NAME_PRELOADING
				| XML.BCollectionXmlParamsFlags.SUPPORTS_UPDATING
		};
		public static readonly Collections.BListAutoIdParams KBListParams
#if SQUAD_NEEDS_ToLowerDataNames
			= new Collections.BListAutoIdParams()
		{
			ToLowerDataNames = kBListXmlParams.ToLowerDataNames,
		};
#else
			= null;
#endif

		public static readonly Engine.XmlFileInfo KXmlFileInfo = new Engine.XmlFileInfo
		{
			Location = Engine.ContentStorage.GAME,
			Directory = Engine.GameDirectory.DATA,
			FileName = "Squads.xml",
			RootName = KBListXmlParams.rootName
		};
		public static readonly Engine.XmlFileInfo KXmlFileInfoUpdate = new Engine.XmlFileInfo
		{
			Location = Engine.ContentStorage.UPDATE,
			Directory = Engine.GameDirectory.DATA,
			FileName = "Squads_Update.xml",
			RootName = KBListXmlParams.rootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo KProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.PROTO_DATA,
			KXmlFileInfo,
			KXmlFileInfoUpdate);

		static readonly Collections.CodeEnum<BProtoSquadFlags> KFlagsProtoEnum = new Collections.CodeEnum<BProtoSquadFlags>();
		static readonly Collections.BBitSetParams KFlagsParams = new Collections.BBitSetParams(() => KFlagsProtoEnum);
		#endregion

		#region FormationType
		BProtoSquadFormationType mFormationType_ = BProtoSquadFormationType.STANDARD;
		public BProtoSquadFormationType FormationType
		{
			get { return this.mFormationType_; }
			set { this.mFormationType_ = value; }
		}
		#endregion
		bool mUpdate_;
		public bool Update { get { return this.mUpdate_; } }

		#region PortraitIcon
		string mPortraitIcon_;
		public string PortraitIcon
		{
			get { return this.mPortraitIcon_; }
			set { this.mPortraitIcon_ = value; }
		}
		#endregion
		#region AltIcon
		string mAltIcon_;
		public string AltIcon
		{
			get { return this.mAltIcon_; }
			set { this.mAltIcon_ = value; }
		}
		#endregion
		#region Stance
		BSquadStance mStance_ = BSquadStance.DEFENSIVE;
		public BSquadStance Stance
		{
			get { return this.mStance_; }
			set { this.mStance_ = value; }
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
		#region Selection
		static BVector CDefaultSelectionRadius { get { return new BVector(1.0f, 0.0f, 1.0f, 0.0f); } }

		BVector mSelectionRadius_ = CDefaultSelectionRadius;
		public BVector SelectionRadius
		{
			get { return this.mSelectionRadius_; }
			set { this.mSelectionRadius_ = value; }
		}

		BVector mSelectionOffset_;
		public BVector SelectionOffset
		{
			get { return this.mSelectionOffset_; }
			set { this.mSelectionOffset_ = value; }
		}

		bool mSelectionConformToTerrain_;
		public bool SelectionConformToTerrain
		{
			get { return this.mSelectionConformToTerrain_; }
			set { this.mSelectionConformToTerrain_ = value; }
		}

		bool mSelectionAllowOrientation_ = true;
		public bool SelectionAllowOrientation
		{
			get { return this.mSelectionAllowOrientation_; }
			set { this.mSelectionAllowOrientation_ = value; }
		}

		bool HasSelectionData { get {
			return this.SelectionRadius != CDefaultSelectionRadius
				|| PhxPredicates.IsNotZero(this.SelectionOffset)
				||
				this.SelectionConformToTerrain
				||
				this.SelectionAllowOrientation==false;
		} }
		#endregion
		#region HPBar

		int mHpBarId_ = TypeExtensions.K_NONE;
		[Meta.BProtoHpBarReference]
		public int HpBarId
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
			return this.HpBarId.IsNotNone()
				|| PhxPredicates.IsNotZero(this.HpBarSize)
				|| PhxPredicates.IsNotZero(this.HpBarOffset);
		} }
		#endregion
		#region VeterancyBar
		static BVector CDefaultVeterancyBarSize { get { return new BVector(1.0f, 1.0f, 0.0f, 0.0f); } }

		int mVeterancyBarId_ = TypeExtensions.K_NONE;
		[Meta.BProtoVeterancyBarReference]
		public int VeterancyBarId
		{
			get { return this.mVeterancyBarId_; }
			set { this.mVeterancyBarId_ = value; }
		}

		int mVeterancyCenteredBarId_ = TypeExtensions.K_NONE;
		[Meta.BProtoVeterancyBarReference]
		public int VeterancyCenteredBarId
		{
			get { return this.mVeterancyCenteredBarId_; }
			set { this.mVeterancyCenteredBarId_ = value; }
		}

		BVector mVeterancyBarSize_ = CDefaultVeterancyBarSize;
		public BVector VeterancyBarSize
		{
			get { return this.mVeterancyBarSize_; }
			set { this.mVeterancyBarSize_ = value; }
		}

		BVector mVeterancyBarOffset_;
		public BVector VeterancyBarOffset
		{
			get { return this.mVeterancyBarOffset_; }
			set { this.mVeterancyBarOffset_ = value; }
		}

		bool HasVeterancyBarData { get {
			return this.VeterancyBarId.IsNotNone()
				||
				this.VeterancyCenteredBarId.IsNotNone()
				||
				this.VeterancyBarSize != CDefaultVeterancyBarSize
				|| PhxPredicates.IsNotZero(this.VeterancyBarOffset);
		} }
		#endregion
		#region AbilityRecoveryBar
		static BVector CDefaultAbilityRecoveryBarSize { get { return new BVector(1.0f, 1.0f, 0.0f, 0.0f); } }

		int mAbilityRecoveryBarId_ = TypeExtensions.K_NONE;
		[Meta.BProtoPieProgressReference]
		public int AbilityRecoveryBarId
		{
			get { return this.mAbilityRecoveryBarId_; }
			set { this.mAbilityRecoveryBarId_ = value; }
		}

		int mAbilityRecoveryCenteredBarId_ = TypeExtensions.K_NONE;
		[Meta.BProtoPieProgressReference]
		public int AbilityRecoveryCenteredBarId
		{
			get { return this.mAbilityRecoveryCenteredBarId_; }
			set { this.mAbilityRecoveryCenteredBarId_ = value; }
		}

		BVector mAbilityRecoveryBarSize_ = CDefaultAbilityRecoveryBarSize;
		public BVector AbilityRecoveryBarSize
		{
			get { return this.mAbilityRecoveryBarSize_; }
			set { this.mAbilityRecoveryBarSize_ = value; }
		}

		BVector mAbilityRecoveryBarOffset_;
		public BVector AbilityRecoveryBarOffset
		{
			get { return this.mAbilityRecoveryBarOffset_; }
			set { this.mAbilityRecoveryBarOffset_ = value; }
		}

		bool HasAbilityRecoveryBarData { get {
			return this.AbilityRecoveryBarId.IsNotNone()
				||
				this.AbilityRecoveryCenteredBarId.IsNotNone()
				||
				this.AbilityRecoveryBarSize != CDefaultAbilityRecoveryBarSize
				|| PhxPredicates.IsNotZero(this.AbilityRecoveryBarOffset);
		} }
		#endregion
		#region BobbleHeadID
		int mBobbleHeadId_ = TypeExtensions.K_NONE;
		[Meta.BProtoBobbleHeadReference]
		public int BobbleHeadId
		{
			get { return this.mBobbleHeadId_; }
			set { this.mBobbleHeadId_ = value; }
		}
		#endregion
		#region BuildingStrengthDisplayID
		int mBuildingStrengthDisplayId_ = TypeExtensions.K_NONE;
		[Meta.BProtoBuildingStrengthReference]
		public int BuildingStrengthDisplayId
		{
			get { return this.mBuildingStrengthDisplayId_; }
			set { this.mBuildingStrengthDisplayId_ = value; }
		}
		#endregion
		#region CryoPoints
		float mCryoPoints_ = PhxUtil.K_INVALID_SINGLE;
		/// <remarks>Actually defaults to the default value in GameData</remarks>
		public float CryoPoints
		{
			get { return this.mCryoPoints_; }
			set { this.mCryoPoints_ = value; }
		}
		#endregion
		#region DazeResist
		float mDazeResist_ = 1.0f;
		public float DazeResist
		{
			get { return this.mDazeResist_; }
			set { this.mDazeResist_ = value; }
		}
		#endregion
		#region Birth

		BSquadBirthType mBirthType_ = BSquadBirthType.INVALID;
		public BSquadBirthType BirthType
		{
			get { return this.mBirthType_; }
			set { this.mBirthType_ = value; }
		}

		string mBirthBone_;
		public string BirthBone
		{
			get { return this.mBirthBone_; }
			set { this.mBirthBone_ = value; }
		}

		string mBirthEndBone_;
		public string BirthEndBone
		{
			get { return this.mBirthEndBone_; }
			set { this.mBirthEndBone_ = value; }
		}

		string mBirthAnim0_;
		[Meta.BAnimTypeReference]
		public string BirthAnim0
		{
			get { return this.mBirthAnim0_; }
			set { this.mBirthAnim0_ = value; }
		}

		string mBirthAnim1_;
		[Meta.BAnimTypeReference]
		public string BirthAnim1
		{
			get { return this.mBirthAnim1_; }
			set { this.mBirthAnim1_ = value; }
		}

		string mBirthAnim2_;
		[Meta.BAnimTypeReference]
		public string BirthAnim2
		{
			get { return this.mBirthAnim2_; }
			set { this.mBirthAnim2_ = value; }
		}

		string mBirthAnim3_;
		[Meta.BAnimTypeReference]
		public string BirthAnim3
		{
			get { return this.mBirthAnim3_; }
			set { this.mBirthAnim3_ = value; }
		}

		string mBirthTrainerAnim_;
		[Meta.BAnimTypeReference]
		public string BirthTrainerAnim
		{
			get { return this.mBirthTrainerAnim_; }
			set { this.mBirthTrainerAnim_ = value; }
		}

		bool HasBirthData { get {
			return this.BirthType != BSquadBirthType.INVALID
				||
				this.BirthBone.IsNotNullOrEmpty()
				||
				this.BirthEndBone.IsNotNullOrEmpty()
				||
				this.BirthAnim0.IsNotNullOrEmpty()
				||
				this.BirthAnim1.IsNotNullOrEmpty()
				||
				this.BirthAnim2.IsNotNullOrEmpty()
				||
				this.BirthAnim3.IsNotNullOrEmpty()
				||
				this.BirthTrainerAnim.IsNotNullOrEmpty();
		} }
		#endregion
		public Collections.BListArray<BProtoSquadUnit> Units { get; private set; }
		#region SubSelectSort
		int mSubSelectSort_ = int.MaxValue;
		public int SubSelectSort
		{
			get { return this.mSubSelectSort_; }
			set { this.mSubSelectSort_ = value; }
		}
		#endregion
		#region TurnRadius

		float mTurnRadiusMin_ = PhxUtil.K_INVALID_SINGLE;
		public float TurnRadiusMin
		{
			get { return this.mTurnRadiusMin_; }
			set { this.mTurnRadiusMin_ = value; }
		}

		float mTurnRadiusMax_ = PhxUtil.K_INVALID_SINGLE;
		public float TurnRadiusMax
		{
			get { return this.mTurnRadiusMax_; }
			set { this.mTurnRadiusMax_ = value; }
		}

		bool HasTurnRadiusData { get {
			return PhxPredicates.IsNotInvalid(this.TurnRadiusMin)
				|| PhxPredicates.IsNotInvalid(this.TurnRadiusMax);
		} }
		#endregion
		#region LeashDistance
		float mLeashDistance_;
		public float LeashDistance
		{
			get { return this.mLeashDistance_; }
			set { this.mLeashDistance_ = value; }
		}
		#endregion
		#region LeashDeadzone
		const float C_DEFAULT_LEASH_DEADZONE_ = 10.0f;

		float mLeashDeadzone_ = C_DEFAULT_LEASH_DEADZONE_;
		public float LeashDeadzone
		{
			get { return this.mLeashDeadzone_; }
			set { this.mLeashDeadzone_ = value; }
		}
		#endregion
		#region LeashRecallDelay
		const int C_DEFAULT_LEASH_RECALL_DELAY_ = 2 * 1000;

		int mLeashRecallDelay_ = C_DEFAULT_LEASH_RECALL_DELAY_;
		/// <remarks>in milliseconds</remarks>
		public int LeashRecallDelay
		{
			get { return this.mLeashRecallDelay_; }
			set { this.mLeashRecallDelay_ = value; }
		}
		#endregion
		#region AggroDistance
		float mAggroDistance_;
		public float AggroDistance
		{
			get { return this.mAggroDistance_; }
			set { this.mAggroDistance_ = value; }
		}
		#endregion
		#region MinimapScale
		float mMinimapScale_;
		public float MinimapScale
		{
			get { return this.mMinimapScale_; }
			set { this.mMinimapScale_ = value; }
		}
		#endregion
		public Collections.BBitSet Flags { get; private set; }
		#region Level
		int mLevel_;
		public int Level
		{
			get { return this.mLevel_; }
			set { this.mLevel_ = value; }
		}
		#endregion
		#region TechLevel
		int mTechLevel_;
		public int TechLevel
		{
			get { return this.mTechLevel_; }
			set { this.mTechLevel_ = value; }
		}
		#endregion
		public Collections.BListArray<BProtoSquadSound> Sounds { get; private set; }
		#region RecoveringEffectID
		int mRecoveringEffectId_ = TypeExtensions.K_NONE;
		[Meta.BProtoObjectReference]
		public int RecoveringEffectId
		{
			get { return this.mRecoveringEffectId_; }
			set { this.mRecoveringEffectId_ = value; }
		}
		#endregion
		#region CanAttackWhileMoving
		bool mCanAttackWhileMoving_ = true;
		public bool CanAttackWhileMoving
		{
			get { return this.mCanAttackWhileMoving_; }
			set { this.mCanAttackWhileMoving_ = value; }
		}
		#endregion

		/// <summary>Is this Squad just made up of a single Unit?</summary>
		public bool SquadIsUnit { get {
			return this.Units.Count == 1 && this.Units[0].Count == 1;
		}}

		public BProtoSquad() : base(BResource.KBListTypeValuesParams, BResource.KBListTypeValuesXmlParamsCostLowercaseType)
		{
			var textData = this.CreateDatabaseObjectUserInterfaceTextData();
			textData.HasDisplayNameId = true;
			textData.HasRolloverTextId = true;
			textData.HasStatsNameId = true;
			textData.HasPrereqTextId = true;
			textData.HasRoleTextId = true;

			this.Units = new Collections.BListArray<BProtoSquadUnit>();

			this.Flags = new Collections.BBitSet(KFlagsParams);

			this.Sounds = new Collections.BListArray<BProtoSquadSound>();
		}

		#region IXmlElementStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);
			var xs = s.GetSerializerInterface();

			s.StreamAttributeEnumOpt("formationType", ref this.mFormationType_, e => e != BProtoSquadFormationType.STANDARD);
			s.StreamAttributeOpt("update", ref this.mUpdate_, Predicates.IsTrue);

			s.StreamElementOpt("PortraitIcon", ref this.mPortraitIcon_, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("AltIcon", ref this.mAltIcon_, Predicates.IsNotNullOrEmpty);
			s.StreamElementEnumOpt("Stance", ref this.mStance_, e => e != BSquadStance.DEFENSIVE);
			s.StreamElementOpt("TrainAnim", ref this.mTrainAnim_, Predicates.IsNotNullOrEmpty);
			#region Selection
			using (var bm = s.EnterCursorBookmarkOpt("Selection", this, v => v.HasSelectionData)) if (bm.IsNotNull)
			{
				// #NOTE assumes cDefaultSelectionRadius's XZ values are 1.0
				s.StreamElementOpt("RadiusX", ref this.mSelectionRadius_.X, PhxPredicates.IsNotOne);
				s.StreamElementOpt("RadiusZ", ref this.mSelectionRadius_.Z, PhxPredicates.IsNotOne);
				s.StreamElementOpt("YOffset", ref this.mSelectionOffset_.Y, Predicates.IsNotZero);
				s.StreamElementOpt("ZOffset", ref this.mSelectionOffset_.Z, Predicates.IsNotZero);
				s.StreamElementOpt("ConformToTerrain", ref this.mSelectionConformToTerrain_, Predicates.IsTrue);
				s.StreamElementOpt("AllowOrientation", ref this.mSelectionAllowOrientation_, Predicates.IsFalse);
			}
			#endregion
			#region HPBar
			using (var bm = s.EnterCursorBookmarkOpt("HPBar", this, v => v.HasHpBarData)) if (bm.IsNotNull)
			{
				xs.StreamHpBarName(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mHpBarId_, HpBarDataObjectKind.HP_BAR, isOptional: false, xmlSource: XML.XmlUtil.K_SOURCE_CURSOR);
				s.StreamAttributeOpt("sizeX", ref this.mHpBarSize_.X, Predicates.IsNotZero);
				s.StreamAttributeOpt("sizeY", ref this.mHpBarSize_.Y, Predicates.IsNotZero);
				s.StreamBVector("offset", ref this.mHpBarOffset_, xmlSource: XML.XmlUtil.K_SOURCE_ATTR);
			}
			#endregion
			#region VeterancyBar
			using (var bm = s.EnterCursorBookmarkOpt("VeterancyBar", this, v => v.HasVeterancyBarData)) if (bm.IsNotNull)
			{
				xs.StreamHpBarName(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mVeterancyBarId_, HpBarDataObjectKind.VETERANCY_BAR, isOptional: false, xmlSource: XML.XmlUtil.K_SOURCE_CURSOR);
				xs.StreamHpBarName(s, "Centered", ref this.mVeterancyCenteredBarId_, HpBarDataObjectKind.VETERANCY_BAR, isOptional: false, xmlSource: XML.XmlUtil.K_SOURCE_ATTR);
				// #NOTE assumes cDefaultVeterancyBarSize's XY values are 1.0
				s.StreamAttributeOpt("sizeX", ref this.mVeterancyBarSize_.X, PhxPredicates.IsNotOne);
				s.StreamAttributeOpt("sizeY", ref this.mVeterancyBarSize_.Y, PhxPredicates.IsNotOne);
				s.StreamAttributeOpt("offsetX", ref this.mVeterancyBarOffset_.X, Predicates.IsNotZero);
				s.StreamAttributeOpt("offsetY", ref this.mVeterancyBarOffset_.Y, Predicates.IsNotZero);
			}
			#endregion
			#region AbilityRecoveryBar
			using (var bm = s.EnterCursorBookmarkOpt("AbilityRecoveryBar", this, v => v.HasAbilityRecoveryBarData)) if (bm.IsNotNull)
			{
				xs.StreamHpBarName(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mAbilityRecoveryBarId_, HpBarDataObjectKind.PIE_PROGRESS, isOptional: false, xmlSource: XML.XmlUtil.K_SOURCE_CURSOR);
				xs.StreamHpBarName(s, "Centered", ref this.mAbilityRecoveryCenteredBarId_, HpBarDataObjectKind.PIE_PROGRESS, isOptional: false, xmlSource: XML.XmlUtil.K_SOURCE_ATTR);
				// #NOTE assumes cDefaultAbilityRecoveryBarSize's XY values are 1.0
				s.StreamAttributeOpt("sizeX", ref this.mAbilityRecoveryBarSize_.X, PhxPredicates.IsNotOne);
				s.StreamAttributeOpt("sizeY", ref this.mAbilityRecoveryBarSize_.Y, PhxPredicates.IsNotOne);
				s.StreamAttributeOpt("offsetX", ref this.mAbilityRecoveryBarOffset_.X, Predicates.IsNotZero);
				s.StreamAttributeOpt("offsetY", ref this.mAbilityRecoveryBarOffset_.Y, Predicates.IsNotZero);
			}
			#endregion
			xs.StreamHpBarName(s, "BobbleHead", ref this.mBobbleHeadId_, HpBarDataObjectKind.BOBBLE_HEAD);
			xs.StreamHpBarName(s, "BuildingStrengthDisplay", ref this.mBuildingStrengthDisplayId_, HpBarDataObjectKind.BUILDING_STRENGTH);
			s.StreamElementOpt("CryoPoints", ref this.mCryoPoints_, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("DazeResist", ref this.mDazeResist_, PhxPredicates.IsNotOne);
			#region Birth
			using (var bm = s.EnterCursorBookmarkOpt("Birth", this, v => v.HasBirthData)) if (bm.IsNotNull)
			{
				s.StreamCursorEnum(ref this.mBirthType_);
				s.StreamAttributeOpt("spawnpoint", ref this.mBirthBone_, Predicates.IsNotNullOrEmpty);
				s.StreamAttributeOpt("endPoint", ref this.mBirthEndBone_, Predicates.IsNotNullOrEmpty);
				s.StreamAttributeOpt("anim0", ref this.mBirthAnim0_, Predicates.IsNotNullOrEmpty);
				s.StreamAttributeOpt("anim1", ref this.mBirthAnim1_, Predicates.IsNotNullOrEmpty);
				s.StreamAttributeOpt("anim2", ref this.mBirthAnim2_, Predicates.IsNotNullOrEmpty);
				s.StreamAttributeOpt("anim3", ref this.mBirthAnim3_, Predicates.IsNotNullOrEmpty);
				s.StreamAttributeOpt("trainerAnim", ref this.mBirthTrainerAnim_, Predicates.IsNotNullOrEmpty);
			}
			#endregion
			XML.XmlUtil.Serialize(s, this.Units, BProtoSquadUnit.KBListXmlParams);
			s.StreamElementOpt("SubSelectSort", ref this.mSubSelectSort_, v => v != int.MaxValue);
			#region TurnRadius
			using (var bm = s.EnterCursorBookmarkOpt("TurnRadius", this, v => v.HasTurnRadiusData)) if (bm.IsNotNull)
			{
				s.StreamAttributeOpt("min", ref this.mTurnRadiusMin_, PhxPredicates.IsNotInvalid);
				s.StreamAttributeOpt("max", ref this.mTurnRadiusMax_, PhxPredicates.IsNotInvalid);
			}
			#endregion
			s.StreamElementOpt("LeashDistance", ref this.mLeashDistance_, Predicates.IsNotZero);
			s.StreamElementOpt("LeashDeadzone", ref this.mLeashDeadzone_, v => v != C_DEFAULT_LEASH_DEADZONE_);
			s.StreamElementOpt("LeashRecallDelay", ref this.mLeashRecallDelay_, v => v != C_DEFAULT_LEASH_RECALL_DELAY_);
			s.StreamElementOpt("AggroDistance", ref this.mAggroDistance_, Predicates.IsNotZero);
			s.StreamElementOpt("MinimapScale", ref this.mMinimapScale_, Predicates.IsNotZero);
			XML.XmlUtil.Serialize(s, this.Flags, XML.BBitSetXmlParams.KFlagsSansRoot);
			s.StreamElementOpt("Level", ref this.mLevel_, Predicates.IsNotZero);
			s.StreamElementOpt("TechLevel", ref this.mTechLevel_, Predicates.IsNotZero);
			XML.XmlUtil.Serialize(s, this.Sounds, BProtoSquadSound.KBListXmlParams);
			xs.StreamDbid(s, "RecoveringEffect", ref this.mRecoveringEffectId_, DatabaseObjectKind.OBJECT);
			s.StreamElementOpt("CanAttackWhileMoving", ref this.mCanAttackWhileMoving_, Predicates.IsFalse);
		}
		#endregion
	};
}