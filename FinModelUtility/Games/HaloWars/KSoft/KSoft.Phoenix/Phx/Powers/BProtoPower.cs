using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoPower
		: DatabaseNamedObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams("Power")
		{
			dataName = K_XML_ATTR_NAME,
			flags = XML.BCollectionXmlParamsFlags.REQUIRES_DATA_NAME_PRELOADING
		};
		public static readonly Engine.XmlFileInfo KXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.DATA,
			FileName = "Powers.xml",
			RootName = KBListXmlParams.rootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo KProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.PROTO_DATA,
			KXmlFileInfo);

		static readonly Collections.CodeEnum<BPowerFlags> KFlagsProtoEnum = new Collections.CodeEnum<BPowerFlags>();
		static readonly Collections.BBitSetParams KFlagsParams = new Collections.BBitSetParams(() => KFlagsProtoEnum);

		static readonly Collections.CodeEnum<BPowerToggableFlags> KFlags2ProtoEnum = new Collections.CodeEnum<BPowerToggableFlags>();
		static readonly Collections.BBitSetParams KFlags2Params = new Collections.BBitSetParams(() => KFlags2ProtoEnum)
		{
			kGetMemberDefaultValue = (id) =>
			{
				switch (id)
				{
				case (int)BPowerToggableFlags.CAMERA_ENABLE_USER_SCROLL:
				case (int)BPowerToggableFlags.CAMERA_ENABLE_USER_YAW:
				case (int)BPowerToggableFlags.CAMERA_ENABLE_USER_ZOOM:
				case (int)BPowerToggableFlags.CAMERA_ENABLE_AUTO_ZOOM_INSTANT:
				case (int)BPowerToggableFlags.CAMERA_ENABLE_AUTO_ZOOM:
				case (int)BPowerToggableFlags.SHOW_IN_POWER_MENU:
					return true;

				case (int)BPowerToggableFlags.SHOW_TRANSPORT_ARROWS:
				default:
					return false;
				}
			}
		};
		#endregion

		public Collections.BTypeValuesSingle Cost { get; private set; }
		public Collections.BListArray<BPowerDynamicCost> DynamicCosts { get; private set; }
		public Collections.BListArray<BPowerTargetEffectiveness> TargetEffectiveness { get; private set; }
		public Collections.BTypeValuesSingle Populations { get; private set; }
		#region UIRadius
		float mUiRadius_;
		public float UiRadius
		{
			get { return this.mUiRadius_; }
			set { this.mUiRadius_ = value; }
		}
		#endregion
		#region PowerType
		BPowerType mPowerType_ = BPowerType.INVALID;
		public BPowerType PowerType
		{
			get { return this.mPowerType_; }
			set { this.mPowerType_ = value; }
		}
		#endregion
		#region AutoRecharge
		int mAutoRecharge_;
		/// <remarks>In milliseconds</remarks>
		public int AutoRecharge
		{
			get { return this.mAutoRecharge_; }
			set { this.mAutoRecharge_ = value; }
		}
		#endregion
		#region UseLimit
		int mUseLimit_;
		public int UseLimit
		{
			get { return this.mUseLimit_; }
			set { this.mUseLimit_ = value; }
		}
		#endregion
		public Collections.BBitSet Flags { get; private set; }
		public Collections.BBitSet Flags2 { get; private set; }
		#region IconTextureName
		string mIconTextureName_;
		[Meta.TextureReference]
		public string IconTextureName
		{
			get { return this.mIconTextureName_; }
			set { this.mIconTextureName_ = value; }
		}
		#endregion
		public List<int> IconLocations { get; private set; }
		[Meta.BProtoTechReference]
		public List<int> TechPrereqs { get; private set; }
		#region ActionType
		BActionType mActionType_ = BActionType.INVALID;
		public BActionType ActionType
		{
			get { return this.mActionType_; }
			set { this.mActionType_ = value; }
		}
		#endregion
		#region MinigameType
		BMinigameType mMinigameType_;
		public BMinigameType MinigameType
		{
			get { return this.mMinigameType_; }
			set { this.mMinigameType_ = value; }
		}
		#endregion
		#region CameraZoomMin
		float mCameraZoomMin_;
		public float CameraZoomMin
		{
			get { return this.mCameraZoomMin_; }
			set { this.mCameraZoomMin_ = value; }
		}
		#endregion
		#region CameraZoomMax
		float mCameraZoomMax_;
		public float CameraZoomMax
		{
			get { return this.mCameraZoomMax_; }
			set { this.mCameraZoomMax_ = value; }
		}
		#endregion
		#region CameraPitchMin
		float mCameraPitchMin_;
		public float CameraPitchMin
		{
			get { return this.mCameraPitchMin_; }
			set { this.mCameraPitchMin_ = value; }
		}
		#endregion
		#region CameraPitchMax
		float mCameraPitchMax_;
		public float CameraPitchMax
		{
			get { return this.mCameraPitchMax_; }
			set { this.mCameraPitchMax_ = value; }
		}
		#endregion
		#region CameraEffectIn
		string mCameraEffectIn_;
		[Meta.CameraEffectReference]
		public string CameraEffectIn
		{
			get { return this.mCameraEffectIn_; }
			set { this.mCameraEffectIn_ = value; }
		}
		#endregion
		#region CameraEffectOut
		string mCameraEffectOut_;
		[Meta.CameraEffectReference]
		public string CameraEffectOut
		{
			get { return this.mCameraEffectOut_; }
			set { this.mCameraEffectOut_ = value; }
		}
		#endregion
		#region MinDistanceToSquad
		float mMinDistanceToSquad_ = PhxUtil.K_INVALID_SINGLE;
		public float MinDistanceToSquad
		{
			get { return this.mMinDistanceToSquad_; }
			set { this.mMinDistanceToSquad_ = value; }
		}
		#endregion
		#region MaxDistanceToSquad
		float mMaxDistanceToSquad_ = PhxUtil.K_INVALID_SINGLE;
		public float MaxDistanceToSquad
		{
			get { return this.mMaxDistanceToSquad_; }
			set { this.mMaxDistanceToSquad_ = value; }
		}
		#endregion
		#region ShowTargetHighlight

		int mShowTargetHighlightObjectType_ = TypeExtensions.K_NONE;
		[Meta.ObjectTypeReference]
		public int ShowTargetHighlightObjectType
		{
			get { return this.mShowTargetHighlightObjectType_; }
			set { this.mShowTargetHighlightObjectType_ = value; }
		}

		BRelationType mShowTargetHighlightRelation_ = BRelationType.ANY;
		public BRelationType ShowTargetHighlightRelation
		{
			get { return this.mShowTargetHighlightRelation_; }
			set { this.mShowTargetHighlightRelation_ = value; }
		}

		bool HasShowTargetHighlightData { get {
			return this.mShowTargetHighlightObjectType_.IsNotNone()
				||
				this.mShowTargetHighlightRelation_ != BRelationType.ANY;
		} }
		#endregion
		public List<int> ChildObjectIDs { get; private set; }
		#region BaseDataLevel
		BProtoPowerDataLevel mBaseDataLevel_;
		public BProtoPowerDataLevel BaseDataLevel
		{
			get { return this.mBaseDataLevel_; }
			set { this.mBaseDataLevel_ = value; }
		}
		#endregion
		public Collections.BListExplicitIndex<BProtoPowerDataLevel> LevelData { get; private set; }
		#region TriggerScript
		string mTriggerScript_;
		[Meta.TriggerScriptReference]
		public string TriggerScript
		{
			get { return this.mTriggerScript_; }
			set { this.mTriggerScript_ = value; }
		}
		#endregion
		#region CommandTriggerScript
		string mCommandTriggerScript_;
		[Meta.TriggerScriptReference]
		public string CommandTriggerScript
		{
			get { return this.mCommandTriggerScript_; }
			set { this.mCommandTriggerScript_ = value; }
		}
		#endregion

		public BProtoPower()
		{
			var textData = this.CreateDatabaseObjectUserInterfaceTextData();
			textData.HasDisplayNameId = true;
			textData.HasRolloverTextId = true;
			textData.HasPrereqTextId = true;
			textData.HasChooseTextId = true;

			this.Cost = new Collections.BTypeValuesSingle(BResource.KBListTypeValuesParams);
			this.DynamicCosts = new Collections.BListArray<BPowerDynamicCost>();
			this.TargetEffectiveness = new Collections.BListArray<BPowerTargetEffectiveness>();
			this.Populations = new Collections.BTypeValuesSingle(BPopulation.KBListParamsSingle);

			this.Flags = new Collections.BBitSet(KFlagsParams);
			this.Flags2 = new Collections.BBitSet(KFlags2Params);

			this.IconLocations = [];
			this.TechPrereqs = [];

			this.ChildObjectIDs = [];
			this.LevelData = new Collections.BListExplicitIndex<BProtoPowerDataLevel>(BProtoPowerDataLevel.KBListExplicitIndexParams);
		}

		#region ITagElementStreamable<string> Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			var xs = s.GetSerializerInterface();

			using (s.EnterCursorBookmark("Attributes"))
			{
				base.Serialize(s);

				using (var bm = s.EnterCursorBookmarkOpt("Cost", this.Cost, x => x.HasNonZeroItems)) if (bm.IsNotNull)
					XML.XmlUtil.SerializeCostHack(s, this.Cost);
				XML.XmlUtil.Serialize(s, this.DynamicCosts, BPowerDynamicCost.KBListXmlParams);
				XML.XmlUtil.Serialize(s, this.TargetEffectiveness, BPowerTargetEffectiveness.KBListXmlParams);
				XML.XmlUtil.Serialize(s, this.Populations, BPopulation.KBListXmlParamsSingleLowerCase);
				s.StreamElementOpt("UIRadius", ref this.mUiRadius_, Predicates.IsNotZero);
				s.StreamElementEnumOpt("PowerType", ref this.mPowerType_, e => e != BPowerType.INVALID);
				s.StreamElementOpt("AutoRecharge", ref this.mAutoRecharge_, Predicates.IsNotZero);
				s.StreamElementOpt("UseLimit", ref this.mUseLimit_, Predicates.IsNotZero);
				XML.XmlUtil.Serialize(s, this.Flags, XML.BBitSetXmlParams.KFlagsAreElementNamesThatMeanTrue);
				XML.XmlUtil.Serialize(s, this.Flags2, XML.BBitSetXmlParams.KFlagsAreElementNamesThatMeanTrue);
				s.StreamElementOpt("Icon", ref this.mIconTextureName_, Predicates.IsNotNullOrEmpty);
				s.StreamElements("IconLocation", this.IconLocations, xs, StreamIconLocation);
				s.StreamElements("TechPrereq", this.TechPrereqs, xs, XML.BXmlSerializerInterface.StreamTechId);
				s.StreamElementEnumOpt("Action", ref this.mActionType_, BProtoAction.KNotInvalidActionType);
				s.StreamElementEnumOpt("Minigame", ref this.mMinigameType_, e => e != BMinigameType.NONE);
				s.StreamElementOpt("CameraZoomMin", ref this.mCameraZoomMin_, Predicates.IsNotZero);
				s.StreamElementOpt("CameraZoomMax", ref this.mCameraZoomMax_, Predicates.IsNotZero);
				s.StreamElementOpt("CameraPitchMin", ref this.mCameraPitchMin_, Predicates.IsNotZero);
				s.StreamElementOpt("CameraPitchMax", ref this.mCameraPitchMax_, Predicates.IsNotZero);
				s.StreamElementOpt("CameraEffectIn", ref this.mCameraEffectIn_, Predicates.IsNotNullOrEmpty);
				s.StreamElementOpt("CameraEffectOut", ref this.mCameraEffectOut_, Predicates.IsNotNullOrEmpty);
				s.StreamElementOpt("MinDistanceToSquad", ref this.mMinDistanceToSquad_, PhxPredicates.IsNotInvalid);
				s.StreamElementOpt("MaxDistanceToSquad", ref this.mMaxDistanceToSquad_, PhxPredicates.IsNotInvalid);
				using (var bm = s.EnterCursorBookmarkOpt("ShowTargetHighlight", this, x => x.HasShowTargetHighlightData)) if (bm.IsNotNull)
				{
					xs.StreamDbid(s, "ObjectType", ref this.mShowTargetHighlightObjectType_, DatabaseObjectKind.OBJECT_TYPE, xmlSource: XML.XmlUtil.K_SOURCE_ATTR);
					s.StreamAttributeEnumOpt("Relation", ref this.mShowTargetHighlightRelation_, e => e != BRelationType.ANY);
				}
				using (var bm = s.EnterCursorBookmarkOpt("ChildObjects", this.ChildObjectIDs, Predicates.HasItems)) if (bm.IsNotNull)
				{
					s.StreamElements("Object", this.ChildObjectIDs, xs, XML.BXmlSerializerInterface.StreamObjectId);
				}
				using (var bm = s.EnterCursorBookmarkOpt("BaseDataLevel", this, x => x.BaseDataLevel != null)) if (bm.IsNotNull)
				{
					if (s.IsReading)
						this.mBaseDataLevel_ = new BProtoPowerDataLevel();

					this.BaseDataLevel.Serialize(s);
				}
				XML.XmlUtil.Serialize(s, this.LevelData, BProtoPowerDataLevel.KBListExplicitIndexXmlParams);
			}
			s.StreamElementOpt("TriggerScript", ref this.mTriggerScript_, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("CommandTriggerScript", ref this.mCommandTriggerScript_, Predicates.IsNotNullOrEmpty);
		}

		static void StreamIconLocation<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs,
			ref int value)
			where TDoc : class
			where TCursor : class
		{
			s.StreamCursor(ref value);
		}
		#endregion
	};

	public sealed class BPowerDynamicCost
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "DynamicCost",
		};
		#endregion

		#region ObjectType
		int mObjectType_ = TypeExtensions.K_NONE;
		[Meta.ObjectTypeReference]
		public int ObjectType
		{
			get { return this.mObjectType_; }
			set { this.mObjectType_ = value; }
		}
		#endregion

		#region Multiplier
		float mMultiplier_ = 1.0f;
		public float Multiplier
		{
			get { return this.mMultiplier_; }
			set { this.mMultiplier_ = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDbid(s, "ObjectType", ref this.mObjectType_, DatabaseObjectKind.OBJECT_TYPE, isOptional: false, xmlSource: XML.XmlUtil.K_SOURCE_ATTR);
			s.StreamCursor(ref this.mMultiplier_);
		}
		#endregion
	};

	public sealed class BPowerTargetEffectiveness
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "TargetEffectiveness",
		};
		#endregion

		#region ObjectType
		int mObjectType_ = TypeExtensions.K_NONE;
		[Meta.ObjectTypeReference]
		public int ObjectType
		{
			get { return this.mObjectType_; }
			set { this.mObjectType_ = value; }
		}
		#endregion

		#region Effectiveness
		int mEffectiveness_;
		public int Effectiveness
		{
			get { return this.mEffectiveness_; }
			set { this.mEffectiveness_ = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDbid(s, "ObjectType", ref this.mObjectType_, DatabaseObjectKind.OBJECT_TYPE, isOptional: false, xmlSource: XML.XmlUtil.K_SOURCE_ATTR);
			s.StreamCursor(ref this.mEffectiveness_);
		}
		#endregion
	};

	public sealed class BProtoPowerDataLevel
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly Collections.BListExplicitIndexParams<BProtoPowerDataLevel> KBListExplicitIndexParams = new
			Collections.BListExplicitIndexParams<BProtoPowerDataLevel>()
		{
			kComparer = new ComparerForDataCount(),//PhxUtil.CreateDummyComparerAlwaysNonZero<BProtoPowerDataLevel>(),
			kTypeGetInvalid = () => new BProtoPowerDataLevel()
		};
		public static readonly XML.BListExplicitIndexXmlParams<BProtoPowerDataLevel> KBListExplicitIndexXmlParams = new
			XML.BListExplicitIndexXmlParams<BProtoPowerDataLevel>("DataLevel", "level")
		{
			indexBase = 0
		};

		private sealed class ComparerForDataCount
			: IComparer<BProtoPowerDataLevel>
		{
			public int Compare(BProtoPowerDataLevel x, BProtoPowerDataLevel y)
			{
				if (x == null || y == null)
					return -1;

				if (x.Data.Count == 0 && y.Data.Count == 0)
					return 0;

				return -1;
			}
		};
		#endregion

		public Collections.BListArray<BProtoPowerData> Data { get; private set; } = new Collections.BListArray<BProtoPowerData>();

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			XML.XmlUtil.Serialize(s, this.Data, BProtoPowerData.KBListXmlParams);
		}
		#endregion
	};

	public sealed class BProtoPowerData
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "Data",
		};
		#endregion

		#region Name
		string mName_;
		public string Name
		{
			get { return this.mName_; }
			set { this.mName_ = value; }
		}
		#endregion

		#region DataType
		ProtoPowerDataType mDataType_ = ProtoPowerDataType.INVALID;
		public ProtoPowerDataType DataType
		{
			get { return this.mDataType_; }
			set { this.mDataType_ = value; }
		}
		#endregion

		#region Data
		float mDataFloat_;
		int mDataInt_ = TypeExtensions.K_NONE;
		bool mDataBool_;
		string mDataString_;

		public float Float
		{
			get { return this.mDataFloat_; }
			set { this.mDataFloat_ = value; }
		}

		public int Int
		{
			get { return this.mDataInt_; }
			set { this.mDataInt_ = value; }
		}
		[Meta.BProtoObjectReference]
		public int ObjectId
		{
			get { return this.mDataInt_; }
			set { this.mDataInt_ = value; }
		}
		[Meta.BProtoSquadReference]
		public int SquadId
		{
			get { return this.mDataInt_; }
			set { this.mDataInt_ = value; }
		}
		[Meta.BProtoTechReference]
		public int TechId
		{
			get { return this.mDataInt_; }
			set { this.mDataInt_ = value; }
		}
		[Meta.ObjectTypeReference]
		public int ObjectType
		{
			get { return this.mDataInt_; }
			set { this.mDataInt_ = value; }
		}

		public bool Bool
		{
			get { return this.mDataBool_; }
			set { this.mDataBool_ = value; }
		}

		[Meta.SoundCueReference]
		public string SoundCue
		{
			get { return this.mDataString_; }
			set { this.mDataString_ = value; }
		}
		[Meta.TextureReference]
		public string TextureName
		{
			get { return this.mDataString_; }
			set { this.mDataString_ = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttributeEnum("type", ref this.mDataType_);
			s.StreamAttribute("name", ref this.mName_);

			switch (this.DataType)
			{
			case ProtoPowerDataType.FLOAT:
				s.StreamCursor(ref this.mDataFloat_);
				break;

			case ProtoPowerDataType.INT:
				s.StreamCursor(ref this.mDataInt_);
				break;
			case ProtoPowerDataType.PROTO_OBJECT:
				xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mDataInt_, DatabaseObjectKind.OBJECT, isOptional: false, xmlSource: XML.XmlUtil.K_SOURCE_CURSOR);
				break;
			case ProtoPowerDataType.PROTO_SQUAD:
				xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mDataInt_, DatabaseObjectKind.SQUAD, isOptional: false, xmlSource: XML.XmlUtil.K_SOURCE_CURSOR);
				break;
			case ProtoPowerDataType.TECH:
				xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mDataInt_, DatabaseObjectKind.TECH, isOptional: false, xmlSource: XML.XmlUtil.K_SOURCE_CURSOR);
				break;
			case ProtoPowerDataType.OBJECT_TYPE:
				xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mDataInt_, DatabaseObjectKind.OBJECT_TYPE, isOptional: false, xmlSource: XML.XmlUtil.K_SOURCE_CURSOR);
				break;

			case ProtoPowerDataType.BOOL:
				s.StreamCursor(ref this.mDataBool_);
				break;

			case ProtoPowerDataType.SOUND:
				s.StreamCursor(ref this.mDataString_);
				break;
			case ProtoPowerDataType.TEXTURE:
				s.StreamCursor(ref this.mDataString_);
				break;
			}

			//xs.StreamDBID(s, "ObjectType", ref mObjectType, DatabaseObjectKind.ObjectType, isOptional: false, xmlSource: XML.XmlUtil.kSourceAttr);
		}
		#endregion
	};

	public static class BProtoPowerTypesData
	{
		public static Dictionary<string, ProtoPowerDataType> kHelper = new Dictionary<string, ProtoPowerDataType>
		{
			{"HudUpSound", ProtoPowerDataType.SOUND},
			{"HudAbortSound", ProtoPowerDataType.SOUND},
			{"HudFireSound", ProtoPowerDataType.SOUND},

			{"HudLastFireSound", ProtoPowerDataType.SOUND},
			{"HudStartEnvSound", ProtoPowerDataType.SOUND},
			{"HudStopEnvSound", ProtoPowerDataType.SOUND},
		};

		public static Dictionary<string, ProtoPowerDataType> kCarpetBombing = new Dictionary<string,ProtoPowerDataType>
		{
			{"Projectile", ProtoPowerDataType.PROTO_OBJECT},
			{"Impact", ProtoPowerDataType.PROTO_OBJECT},
			{"Explosion", ProtoPowerDataType.PROTO_OBJECT},
			{"Bomber", ProtoPowerDataType.PROTO_OBJECT},
			{"InitialDelay", ProtoPowerDataType.FLOAT},
			{"FuseTime", ProtoPowerDataType.FLOAT},
			{"MaxBombs", ProtoPowerDataType.INT},
			{"MaxBombOffset", ProtoPowerDataType.FLOAT},
			{"BombSpacing", ProtoPowerDataType.FLOAT},
			{"LengthMultiplier", ProtoPowerDataType.FLOAT},
			{"WedgeLengthMultiplier", ProtoPowerDataType.FLOAT},
			{"WedgeMinOffset", ProtoPowerDataType.FLOAT},
			{"NudgeMultiplier", ProtoPowerDataType.FLOAT},
			{"BomberFlyinDistance", ProtoPowerDataType.FLOAT},
			{"BomberFlyinHeight", ProtoPowerDataType.FLOAT},
			{"BomberBombHeight", ProtoPowerDataType.FLOAT},
			{"BomberSpeed", ProtoPowerDataType.FLOAT},
			{"RequiresLOS", ProtoPowerDataType.BOOL},

			//kHelper
		};

		public static Dictionary<string, ProtoPowerDataType> kCleansing = new Dictionary<string, ProtoPowerDataType>
		{
			{"Beam", ProtoPowerDataType.PROTO_OBJECT},
			{"Projectile", ProtoPowerDataType.PROTO_OBJECT},
			{"TickLength", ProtoPowerDataType.FLOAT},
			{"SuppliesPerTick", ProtoPowerDataType.FLOAT},
			{"MinBeamDistance", ProtoPowerDataType.FLOAT},
			{"MaxBeamDistance", ProtoPowerDataType.FLOAT},
			{"CommandInterval", ProtoPowerDataType.FLOAT},
			{"MaxBeamSpeed", ProtoPowerDataType.FLOAT},
			{"RequiresLOS", ProtoPowerDataType.BOOL},
		};

		public static Dictionary<string, ProtoPowerDataType> kCryo = new Dictionary<string, ProtoPowerDataType>
		{
			{"CryoObject", ProtoPowerDataType.PROTO_OBJECT},
			{"CryoRadius", ProtoPowerDataType.FLOAT},
			{"MinCryoFalloff", ProtoPowerDataType.FLOAT},
			{"FilterType", ProtoPowerDataType.OBJECT_TYPE},
			{"TickDuration", ProtoPowerDataType.FLOAT},
			{"NumTicks", ProtoPowerDataType.INT},
			{"CryoAmountPerTick", ProtoPowerDataType.FLOAT},
			{"EffectStartTime", ProtoPowerDataType.FLOAT},
			{"MaxKillHp", ProtoPowerDataType.FLOAT},
			{"FreezingThawTime", ProtoPowerDataType.FLOAT},
			{"FrozenThawTime", ProtoPowerDataType.FLOAT},

			{"Bomber", ProtoPowerDataType.PROTO_OBJECT},
			{"BomberBombTime", ProtoPowerDataType.FLOAT},
			{"BomberFlyinDistance", ProtoPowerDataType.FLOAT},
			{"BomberFlyinHeight", ProtoPowerDataType.FLOAT},
			{"BomberBombHeight", ProtoPowerDataType.FLOAT},
			{"BomberSpeed", ProtoPowerDataType.FLOAT},
			{"BomberFlyOutTime", ProtoPowerDataType.FLOAT},

			{"AirImpactObject", ProtoPowerDataType.PROTO_OBJECT},

			//kHelper
		};

		public static Dictionary<string, ProtoPowerDataType> kDisruption = new Dictionary<string, ProtoPowerDataType>
		{
			{"DisruptionObject", ProtoPowerDataType.PROTO_OBJECT},
			{"PulseObject", ProtoPowerDataType.PROTO_OBJECT},
			{"StrikeObject", ProtoPowerDataType.PROTO_OBJECT},
			{"DisruptionRadius", ProtoPowerDataType.FLOAT},
			{"DisruptionTimeSec", ProtoPowerDataType.FLOAT},
			{"DisruptionStartTime", ProtoPowerDataType.FLOAT},
			{"PulseSpacing", ProtoPowerDataType.FLOAT},
			{"PulseSound", ProtoPowerDataType.SOUND},

			{"Bomber", ProtoPowerDataType.PROTO_OBJECT},
			{"BomberBombTime", ProtoPowerDataType.FLOAT},
			{"BomberFlyinDistance", ProtoPowerDataType.FLOAT},
			{"BomberFlyinHeight", ProtoPowerDataType.FLOAT},
			{"BomberBombHeight", ProtoPowerDataType.FLOAT},
			{"BomberSpeed", ProtoPowerDataType.FLOAT},
			{"BomberFlyOutTime", ProtoPowerDataType.FLOAT},

			{"RequiresLOS", ProtoPowerDataType.BOOL},

			//kHelper
		};

		public static Dictionary<string, ProtoPowerDataType> kOdst = new Dictionary<string, ProtoPowerDataType>
		{
			{"Projectile", ProtoPowerDataType.PROTO_OBJECT},
			{"SquadSpawnDelay", ProtoPowerDataType.FLOAT},

			{"RequiresLOS", ProtoPowerDataType.BOOL},

			//kHelper
		};

		public static Dictionary<string, ProtoPowerDataType> kOrbital = new Dictionary<string, ProtoPowerDataType>
		{
			{"TargetBeam", ProtoPowerDataType.PROTO_OBJECT},
			{"Projectile", ProtoPowerDataType.PROTO_OBJECT},
			{"Effect", ProtoPowerDataType.PROTO_OBJECT},
			{"RockSmall", ProtoPowerDataType.PROTO_OBJECT},
			{"RockMedium", ProtoPowerDataType.PROTO_OBJECT},
			{"RockLarge", ProtoPowerDataType.PROTO_OBJECT},
			{"NumShots", ProtoPowerDataType.INT},
			{"TargetingDelay", ProtoPowerDataType.FLOAT},
			{"AutoShotDelay", ProtoPowerDataType.FLOAT},
			{"AutoShotInnerRadius", ProtoPowerDataType.FLOAT},
			{"AutoShotOuterRadius", ProtoPowerDataType.FLOAT},
			{"XOffset", ProtoPowerDataType.FLOAT},
			{"YOffset", ProtoPowerDataType.FLOAT},
			{"ZOffset", ProtoPowerDataType.FLOAT},
			{"FiredSound", ProtoPowerDataType.SOUND},

			{"CommandInterval", ProtoPowerDataType.FLOAT},
			{"ShotInterval", ProtoPowerDataType.FLOAT},
			{"RequiresLOS", ProtoPowerDataType.BOOL},

			//kHelper
		};

		public static Dictionary<string, ProtoPowerDataType> kRage = new Dictionary<string, ProtoPowerDataType>
		{
			{"TickLength", ProtoPowerDataType.FLOAT},
			{"SuppliesPerTick", ProtoPowerDataType.FLOAT},
			{"SuppliesPerTickAttacking", ProtoPowerDataType.FLOAT},
			{"SuppliesPerJump", ProtoPowerDataType.FLOAT},
			{"DamageMultiplier", ProtoPowerDataType.FLOAT},
			{"DamageTakenMultiplier", ProtoPowerDataType.FLOAT},
			{"SpeedMultiplier", ProtoPowerDataType.FLOAT},
			{"NudgeMultiplier", ProtoPowerDataType.FLOAT},
			{"ScanRadius", ProtoPowerDataType.FLOAT},
			{"TeleportTime", ProtoPowerDataType.FLOAT},
			{"TeleportLateralDistance", ProtoPowerDataType.FLOAT},
			{"TeleportJumpDistance", ProtoPowerDataType.FLOAT},
			{"TimeBetweenRetarget", ProtoPowerDataType.FLOAT},
			{"MotionBlurAmount", ProtoPowerDataType.FLOAT},
			{"MotionBlurDistance", ProtoPowerDataType.FLOAT},
			{"MotionBlurTime", ProtoPowerDataType.FLOAT},
			{"DistanceVsAngleWeight", ProtoPowerDataType.FLOAT},
			{"Projectile", ProtoPowerDataType.PROTO_OBJECT},
			{"HandAttachObject", ProtoPowerDataType.PROTO_OBJECT},
			{"TeleportAttachObject", ProtoPowerDataType.PROTO_OBJECT},
			{"AuraAttachFxSmall", ProtoPowerDataType.PROTO_OBJECT},
			{"AuraAttachFxMedium", ProtoPowerDataType.PROTO_OBJECT},
			{"AuraAttachFxLarge", ProtoPowerDataType.PROTO_OBJECT},
			{"HealAttachFx", ProtoPowerDataType.PROTO_OBJECT},
			{"AuraFilterType", ProtoPowerDataType.OBJECT_TYPE},

			{"HealPerKillCombatValue", ProtoPowerDataType.FLOAT},
			{"AuraRadius", ProtoPowerDataType.FLOAT},
			{"AuraDamageBonus", ProtoPowerDataType.FLOAT},
			{"AttackSound", ProtoPowerDataType.SOUND},

			{"CommandInterval", ProtoPowerDataType.FLOAT},
			{"HintTime", ProtoPowerDataType.FLOAT},
			{"MovementProjectionMultiplier", ProtoPowerDataType.FLOAT},
			{"CameraZoom", ProtoPowerDataType.FLOAT},
		};

		public static Dictionary<string, ProtoPowerDataType> kRepair = new Dictionary<string, ProtoPowerDataType>
		{
			{"RepairObject", ProtoPowerDataType.PROTO_OBJECT},
			{"RepairAttachment", ProtoPowerDataType.PROTO_OBJECT},
			{"NeverStops", ProtoPowerDataType.BOOL},
			{"RepairRadius", ProtoPowerDataType.FLOAT},
			{"FilterType", ProtoPowerDataType.OBJECT_TYPE},
			{"TickDuration", ProtoPowerDataType.FLOAT},
			{"NumTicks", ProtoPowerDataType.INT},
			{"RepairCombatValuePerTick", ProtoPowerDataType.FLOAT},
			{"SpreadAmongSquads", ProtoPowerDataType.BOOL},
			{"AllowReinforce", ProtoPowerDataType.BOOL},
			{"CooldownTimeIfDamaged", ProtoPowerDataType.FLOAT},
			{"HealAny", ProtoPowerDataType.BOOL},

			{"IgnorePlacement", ProtoPowerDataType.BOOL},

			//kHelper
		};

		public static Dictionary<string, ProtoPowerDataType> kTransport = new Dictionary<string, ProtoPowerDataType>
		{
			{"Base", ProtoPowerDataType.TEXTURE},
			{"Mover", ProtoPowerDataType.TEXTURE},

			{"MaxGroundVehicles", ProtoPowerDataType.INT},
			{"MaxInfantryUnits", ProtoPowerDataType.INT},

			{"MinTransportDistance", ProtoPowerDataType.FLOAT},

			{"RequiresLOS", ProtoPowerDataType.BOOL},

			//kHelper
		};

		public static Dictionary<string, ProtoPowerDataType> kWave = new Dictionary<string, ProtoPowerDataType>
		{
			{"TickLength", ProtoPowerDataType.FLOAT},
			{"SuppliesPerTick", ProtoPowerDataType.FLOAT},
			{"MaxBallSpeedStagnant", ProtoPowerDataType.FLOAT},
			{"MaxBallSpeedPulling", ProtoPowerDataType.FLOAT},
			{"ExplodeTime", ProtoPowerDataType.FLOAT},
			{"PullingRange", ProtoPowerDataType.FLOAT},
			{"ExplosionForceOnDebris", ProtoPowerDataType.FLOAT},
			{"HealthToCapture", ProtoPowerDataType.FLOAT},
			{"NudgeStrength", ProtoPowerDataType.FLOAT},
			{"InitialLateralPullStrength", ProtoPowerDataType.FLOAT},
			{"CapturedRadialSpacing", ProtoPowerDataType.FLOAT},
			{"CapturedSpringStrength", ProtoPowerDataType.FLOAT},
			{"CapturedSpringDampening", ProtoPowerDataType.FLOAT},
			{"CapturedSpringRestLength", ProtoPowerDataType.FLOAT},
			{"CapturedMinLateralSpeed", ProtoPowerDataType.FLOAT},
			{"RipAttachmentChancePulling", ProtoPowerDataType.FLOAT},
			{"DebrisAngularDamping", ProtoPowerDataType.FLOAT},
			{"MaxExplosionDamageBankPerCaptured", ProtoPowerDataType.FLOAT},
			{"CommandInterval", ProtoPowerDataType.FLOAT},
			{"MinBallDistance", ProtoPowerDataType.FLOAT},
			{"MaxBallDistance", ProtoPowerDataType.FLOAT},
			{"PickupObjectRate", ProtoPowerDataType.FLOAT},
			{"MaxCapturedObjects", ProtoPowerDataType.INT},
			{"LightningPerTick", ProtoPowerDataType.INT},
			{"NudgeChancePulling", ProtoPowerDataType.INT},
			{"ThrowPartChancePulling", ProtoPowerDataType.INT},
			{"LightningChancePulling", ProtoPowerDataType.INT},
			{"BallObject", ProtoPowerDataType.PROTO_OBJECT},
			{"LightningProjectile", ProtoPowerDataType.PROTO_OBJECT},
			{"ExplodeProjectile", ProtoPowerDataType.PROTO_OBJECT},
			{"DebrisProjectile", ProtoPowerDataType.PROTO_OBJECT},
			{"PickupAttachment", ProtoPowerDataType.PROTO_OBJECT},
			{"ThrowUnitsOnExplosion", ProtoPowerDataType.BOOL},
			{"ExplodeSound", ProtoPowerDataType.SOUND},
			{"MinDamageBankPercentToThrow", ProtoPowerDataType.FLOAT},
			{"LightningBeamVisual", ProtoPowerDataType.PROTO_OBJECT},

			{"MinBallHeight", ProtoPowerDataType.FLOAT},
			{"MaxBallHeight", ProtoPowerDataType.FLOAT},
			{"CameraDistance", ProtoPowerDataType.FLOAT},
			{"CameraHeight", ProtoPowerDataType.FLOAT},
			{"CameraHoverPointDistance", ProtoPowerDataType.FLOAT},
			{"CameraMaxBallAngle", ProtoPowerDataType.FLOAT},
			{"PickupShakeDuration", ProtoPowerDataType.FLOAT},
			{"PickupRumbleShakeStrength", ProtoPowerDataType.FLOAT},
			{"PickupCameraShakeStrength", ProtoPowerDataType.FLOAT},

			{"HintTime", ProtoPowerDataType.FLOAT},
		};
	};
}
