using System.ComponentModel;

namespace KSoft.Phoenix.Phx
{
	public sealed class BCiv
		: DatabaseNamedObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams("Civ")
		{
			dataName = "Name",
			flags = XML.BCollectionXmlParamsFlags.USE_ELEMENT_FOR_DATA
		};
		public static readonly Engine.XmlFileInfo KXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.DATA,
			FileName = "Civs.xml",
			RootName = KBListXmlParams.rootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo KProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.PROTO_DATA,
			KXmlFileInfo);
		#endregion

		#region IsExcludedFromAlpha
		int mAlpha_ = TypeExtensions.K_NONE;
		public bool IsExcludedFromAlpha
		{
			get { return this.mAlpha_ == 0; }
			set { this.mAlpha_ = value ? 0 : TypeExtensions.K_NONE; }
		}
		#endregion

		#region TechID
		int mTechId_ = TypeExtensions.K_NONE;
		[Meta.BProtoTechReference]
		public int TechId
		{
			get { return this.mTechId_; }
			set { this.mTechId_ = value; }
		}
		#endregion

		#region CommandAckObjectID
		int mCommandAckObjectId_ = TypeExtensions.K_NONE;
		[Meta.BProtoObjectReference]
		public int CommandAckObjectId
		{
			get { return this.mCommandAckObjectId_; }
			set { this.mCommandAckObjectId_ = value; }
		}
		#endregion

		#region RallyPointObjectID
		int mRallyPointObjectId_ = TypeExtensions.K_NONE;
		[Meta.BProtoObjectReference]
		public int RallyPointObjectId
		{
			get { return this.mRallyPointObjectId_; }
			set { this.mRallyPointObjectId_ = value; }
		}
		#endregion

		#region LocalRallyPointObjectID
		int mLocalRallyPointObjectId_ = TypeExtensions.K_NONE;
		[Meta.BProtoObjectReference]
		public int LocalRallyPointObjectId
		{
			get { return this.mLocalRallyPointObjectId_; }
			set { this.mLocalRallyPointObjectId_ = value; }
		}
		#endregion

		#region TransportObjectID
		int mTransportObjectId_ = TypeExtensions.K_NONE;
		[Meta.BProtoObjectReference]
		public int TransportObjectId
		{
			get { return this.mTransportObjectId_; }
			set { this.mTransportObjectId_ = value; }
		}
		#endregion

		#region TransportTriggerObjectID
		int mTransportTriggerObjectId_ = TypeExtensions.K_NONE;
		[Meta.BProtoObjectReference]
		public int TransportTriggerObjectId
		{
			get { return this.mTransportTriggerObjectId_; }
			set { this.mTransportTriggerObjectId_ = value; }
		}
		#endregion

		#region HullExpansionRadius
		float mHullExpansionRadius_;
		public float HullExpansionRadius
		{
			get { return this.mHullExpansionRadius_; }
			set { this.mHullExpansionRadius_ = value; }
		}
		#endregion

		#region TerrainPushOffRadius
		float mTerrainPushOffRadius_;
		public float TerrainPushOffRadius
		{
			get { return this.mTerrainPushOffRadius_; }
			set { this.mTerrainPushOffRadius_ = value; }
		}
		#endregion

		#region BuildingMagnetRange
		float mBuildingMagnetRange_;
		public float BuildingMagnetRange
		{
			get { return this.mBuildingMagnetRange_; }
			set { this.mBuildingMagnetRange_ = value; }
		}
		#endregion

		#region SoundBank
		string mSoundBank_;
		// .bnk
		public string SoundBank
		{
			get { return this.mSoundBank_; }
			set { this.mSoundBank_ = value; }
		}
		#endregion

		#region LeaderMenuNameID
		int mLeaderMenuNameId_ = TypeExtensions.K_NONE;
		[Meta.LocStringReference]
		public int LeaderMenuNameId
		{
			get { return this.mLeaderMenuNameId_; }
			set { this.mLeaderMenuNameId_ = value; }
		}
		#endregion

		#region PowerFromHero
		bool mPowerFromHero_;
		public bool PowerFromHero
		{
			get { return this.mPowerFromHero_; }
			set { this.mPowerFromHero_ = value; }
		}
		#endregion

		#region UIControlBackground
		string mUiControlBackground_;
		[Meta.TextureReference]
		public string UiControlBackground
		{
			get { return this.mUiControlBackground_; }
			set { this.mUiControlBackground_ = value; }
		}
		#endregion

		// Empty Civs just have a name
		[Browsable(false)]
		public bool IsEmpty { get { return this.mTechId_.IsNotNone(); } }

		public BCiv()
		{
			var textData = this.CreateDatabaseObjectUserInterfaceTextData();
			textData.HasDisplayNameId = true;
		}

		#region ITagElementStreamable<string> Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			var xs = s.GetSerializerInterface();

			s.StreamAttributeOpt("Alpha", ref this.mAlpha_, Predicates.IsNotNone);
			xs.StreamDbid(s, "CivTech", ref this.mTechId_, DatabaseObjectKind.TECH);
			xs.StreamDbid(s, "CommandAckObject", ref this.mCommandAckObjectId_, DatabaseObjectKind.OBJECT);
			xs.StreamDbid(s, "RallyPointObject", ref this.mRallyPointObjectId_, DatabaseObjectKind.OBJECT);
			xs.StreamDbid(s, "LocalRallyPointObject", ref this.mLocalRallyPointObjectId_, DatabaseObjectKind.OBJECT);
			xs.StreamDbid(s, "Transport", ref this.mTransportObjectId_, DatabaseObjectKind.OBJECT);
			xs.StreamDbid(s, "TransportTrigger", ref this.mTransportTriggerObjectId_, DatabaseObjectKind.OBJECT);
			s.StreamElementOpt("ExpandHull", ref this.mHullExpansionRadius_, Predicates.IsNotZero);
			s.StreamElementOpt("TerrainPushOff", ref this.mTerrainPushOffRadius_, Predicates.IsNotZero);
			s.StreamElementOpt("BuildingMagnetRange", ref this.mBuildingMagnetRange_, Predicates.IsNotZero);
			s.StreamStringOpt("SoundBank", ref this.mSoundBank_, toLower: false, type: XML.XmlUtil.K_SOURCE_ELEMENT);
			xs.StreamStringId(s, "LeaderMenuNameID", ref this.mLeaderMenuNameId_);
			s.StreamElementOpt("PowerFromHero", ref this.mPowerFromHero_, Predicates.IsTrue);
			s.StreamStringOpt("UIControlBackground", ref this.mUiControlBackground_, toLower: false, type: XML.XmlUtil.K_SOURCE_ELEMENT);
		}
		#endregion
	};
}