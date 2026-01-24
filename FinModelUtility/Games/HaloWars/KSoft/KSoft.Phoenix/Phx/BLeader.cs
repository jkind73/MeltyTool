using System;
using System.Collections.Generic;

using BProtoPowerID = System.Int32;
using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.Phx
{
	public sealed class BLeader
		: DatabaseNamedObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams("Leader")
		{
			dataName = "Name",
			flags = 0
		};
		public static readonly Engine.XmlFileInfo KXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.DATA,
			FileName = "Leaders.xml",
			RootName = KBListXmlParams.rootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo KProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.PROTO_DATA,
			KXmlFileInfo);

		static readonly XML.BTypeValuesXmlParams<float> KRepairCostTypeValuesXmlParams = new
			XML.BTypeValuesXmlParams<float>("RepairCost", "Type");
		static readonly XML.BTypeValuesXmlParams<float> KReverseHotDropCostTypeValuesXmlParams = new
			XML.BTypeValuesXmlParams<float>("ReverseHotDropCost", "Type");
		#endregion

		#region IconName
		string mIconName_;
		[Meta.TextureReference]
		public string IconName
		{
			get { return this.mIconName_; }
			set { this.mIconName_ = value; }
		}
		#endregion

		#region Test
		bool mTest_;
		public bool Test
		{
			get { return this.mTest_; }
			set { this.mTest_ = value; }
		}
		#endregion

		#region IsExcludedFromAlpha
		int mAlpha_ = TypeExtensions.K_NONE;
		public bool IsExcludedFromAlpha
		{
			get { return this.mAlpha_ == 0; }
			set { this.mAlpha_ = value ? 0 : TypeExtensions.K_NONE; }
		}
		#endregion

		#region LeaderPickerOrder
		sbyte mLeaderPickerOrder_ = TypeExtensions.K_NONE;
		/// <summary>
		/// Leaders with this attribute set to zero or greater (and without the test attribute set) will be sorted by this value and shown in the leader picker.
		/// </summary>
		public sbyte LeaderPickerOrder
		{
			get { return this.mLeaderPickerOrder_; }
			set { this.mLeaderPickerOrder_ = value; }
		}
		#endregion

		#region StatsID
		sbyte mStatsId_ = TypeExtensions.K_NONE;
		/// <summary>
		/// This identifies leaders for stats/leaderboard purposes, which allows you to reorder the leaders in the XML file without messing up stats/leaderboards.
		/// </summary>
		public sbyte StatsId
		{
			get { return this.mStatsId_; }
			set { this.mStatsId_ = value; }
		}
		#endregion

		#region DefaultPlayerSlotFlags
		byte mDefaultPlayerSlotFlags_;
		/// <summary>
		/// Bit flags defining how leaders are assigned to player slots.
		/// Bits 0-5 indicate that the leader is the default leader for the associated slot when AI is assigned to that slot.
		/// Bit 6 is unused.
		/// Bit 7 indicates that the leader is the default leader to assign to a human player when they join a lobby.
		/// If multiple leaders have a particular bit set, the first matching leader will be used.
		/// </summary>
		public byte DefaultPlayerSlotFlags
		{
			get { return this.mDefaultPlayerSlotFlags_; }
			set { this.mDefaultPlayerSlotFlags_ = value; }
		}
		#endregion

		#region IsRandom
		bool mIsRandom_;
		public bool IsRandom
		{
			get { return this.mIsRandom_; }
			set { this.mIsRandom_ = value; }
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

		#region CivID
		int mCivId_ = TypeExtensions.K_NONE;
		[Meta.BCivReference]
		public int CivId
		{
			get { return this.mCivId_; }
			set { this.mCivId_ = value; }
		}
		#endregion

		#region FlashCivID
		sbyte mFlashCivId_ = TypeExtensions.K_NONE;
		/// <summary>
		/// The civilization as specified in the Flash scene. 0 is UNSC, 1 is Covenant, and 2 is Random.
		/// If you figure out how to modify the Flash scene to add other values to the leader picker, this may be useful to you.
		/// </summary>
		[Meta.BCivReference]
		public sbyte FlashCivId
		{
			get { return this.mFlashCivId_; }
			set { this.mFlashCivId_ = value; }
		}
		#endregion

		#region LeaderPowerID
		int mPowerId_ = TypeExtensions.K_NONE;
		[Meta.BProtoPowerReference]
		public int PowerId
		{
			get { return this.mPowerId_; }
			set { this.mPowerId_ = value; }
		}
		#endregion

		#region FlashImg
		string mFlashImg_;
		[Meta.UnusedData]
		public string FlashImg
		{
			get { return this.mFlashImg_; }
			set { this.mFlashImg_ = value; }
		}
		#endregion

		#region FlashPortrait
		string mFlashPortrait_; // img://unknown.ddx
		[Meta.TextureReference]
		public string FlashPortrait
		{
			get { return this.mFlashPortrait_; }
			set { this.mFlashPortrait_ = value; }
		}
		#endregion

		public Collections.BListArray<BLeaderSupportPower> SupportPowers { get; private set; }
		public Collections.BListArray<BLeaderStartingSquad> StartingSquads { get; private set; }
		public Collections.BListArray<BLeaderStartingUnit> StartingUnits { get; private set; }

		#region RallyPointOffset
		BVector mRallyPointOffset_;
		public BVector RallyPointOffset
		{
			get { return this.mRallyPointOffset_; }
			set { this.mRallyPointOffset_ = value; }
		}
		#endregion

		#region RepairRate
		float mRepairRate_;
		public float RepairRate
		{
			get { return this.mRepairRate_; }
			set { this.mRepairRate_ = value; }
		}
		#endregion

		#region RepairDelay
		float mRepairDelay_;
		/// <remarks>In seconds</remarks>
		public float RepairDelay
		{
			get { return this.mRepairDelay_; }
			set { this.mRepairDelay_ = value; }
		}
		#endregion

		public Collections.BTypeValuesSingle RepairCost { get; private set; }

		#region RepairTime
		float mRepairTime_;
		public float RepairTime
		{
			get { return this.mRepairTime_; }
			set { this.mRepairTime_ = value; }
		}
		#endregion

		public Collections.BTypeValuesSingle ReverseHotDropCost { get; private set; }

		public Collections.BTypeValues<BPopulation> Populations { get; private set; }

		/// <summary>Initial resources and which resources are considered 'active'</summary>
		public Collections.BTypeValuesSingle Resources { get; private set; }

		#region UIControlBackground
		string mUiControlBackground_;
		[Meta.TextureReference]
		public string UiControlBackground
		{
			get { return this.mUiControlBackground_; }
			set { this.mUiControlBackground_ = value; }
		}
		#endregion

		// Empty Leaders just have a Civ
		public bool IsEmpty { get { return this.mTechId_.IsNone(); } }

		public BLeader()
		{
			var textData = this.CreateDatabaseObjectUserInterfaceTextData();
			textData.HasNameId = true;
			textData.HasDescriptionId = true;

			this.SupportPowers = new Collections.BListArray<BLeaderSupportPower>();
			this.StartingSquads = new Collections.BListArray<BLeaderStartingSquad>();
			this.StartingUnits = new Collections.BListArray<BLeaderStartingUnit>();
			this.RepairCost = new Collections.BTypeValuesSingle(BResource.KBListTypeValuesParams);
			this.ReverseHotDropCost = new Collections.BTypeValuesSingle(BResource.KBListTypeValuesParams);
			this.Populations = new Collections.BTypeValues<BPopulation>(BPopulation.KBListParams);
			this.Resources = new Collections.BTypeValuesSingle(BResource.KBListTypeValuesParams);
		}

		#region ITagElementStreamable<string> Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			var xs = s.GetSerializerInterface();

			s.StreamAttributeOpt("Icon", ref this.mIconName_, Predicates.IsNotNullOrEmpty);
			s.StreamAttributeOpt("Test", ref this.mTest_, Predicates.IsTrue);
			s.StreamAttributeOpt("Alpha", ref this.mAlpha_, Predicates.IsNotNone);

			s.StreamAttributeOpt("Random", ref this.mIsRandom_, Predicates.IsTrue);
			s.StreamAttributeOpt("StatsID", ref this.mStatsId_, Predicates.IsNotNone);
			s.StreamAttributeOpt("LeaderPickerOrder", ref this.mLeaderPickerOrder_, Predicates.IsNotNone);
			s.StreamAttributeOpt("DefaultPlayerSlotFlags", ref this.mDefaultPlayerSlotFlags_, Predicates.IsNotZero,
				NumeralBase.HEX);

			xs.StreamDbid(s, "Tech", ref this.mTechId_, DatabaseObjectKind.TECH);
			xs.StreamDbid(s, "Civ", ref this.mCivId_, DatabaseObjectKind.CIV);
			xs.StreamDbid(s, "Power", ref this.mPowerId_, DatabaseObjectKind.POWER);
			s.StreamElementOpt("FlashCivID", ref this.mFlashCivId_, Predicates.IsNotNone);
			s.StreamStringOpt("FlashImg", ref this.mFlashImg_, toLower: false, type: XML.XmlUtil.K_SOURCE_ELEMENT);
			// TODO: HW360's FlashPortrait elements have an ending " character (sans a starting quote). Be careful!
			s.StreamStringOpt("FlashPortrait", ref this.mFlashPortrait_, toLower: false, type: XML.XmlUtil.K_SOURCE_ELEMENT);
			XML.XmlUtil.Serialize(s, this.SupportPowers, BLeaderSupportPower.KBListXmlParams);
			XML.XmlUtil.Serialize(s, this.StartingSquads, BLeaderStartingSquad.KBListXmlParams);
			XML.XmlUtil.Serialize(s, this.StartingUnits, BLeaderStartingUnit.KBListXmlParams);
			s.StreamBVector("RallyPointOffset", ref this.mRallyPointOffset_);
			s.StreamElementOpt("RepairRate", ref this.mRepairRate_, Predicates.IsNotZero);
			s.StreamElementOpt("RepairDelay", ref this.mRepairDelay_, Predicates.IsNotZero);
			XML.XmlUtil.Serialize(s, this.RepairCost, KRepairCostTypeValuesXmlParams);
			s.StreamElementOpt("RepairTime", ref this.mRepairTime_, Predicates.IsNotZero);
			XML.XmlUtil.Serialize(s, this.ReverseHotDropCost, KReverseHotDropCostTypeValuesXmlParams);
			XML.XmlUtil.Serialize(s, this.Populations, BPopulation.KBListXmlParams);
			XML.XmlUtil.Serialize(s, this.Resources, BResource.KBListTypeValuesXmlParams);
			s.StreamStringOpt("UIControlBackground", ref this.mUiControlBackground_, toLower: false, type: XML.XmlUtil.K_SOURCE_ELEMENT);
		}
		#endregion
	};

	public sealed class BLeaderSupportPower
		: IO.ITagElementStringNameStreamable
		, IComparable<BLeaderSupportPower>
		, IEquatable<BLeaderSupportPower>
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "SupportPower",
		};
		#endregion

		#region IconLocation
		int mIconLocation_;
		public int IconLocation
		{
			get { return this.mIconLocation_; }
			set { this.mIconLocation_ = value; }
		}
		#endregion

		#region TechPrereqID
		int mTechPrereqId_ = TypeExtensions.K_NONE;
		[Meta.BProtoTechReference]
		public int TechPrereqId
		{
			get { return this.mTechPrereqId_; }
			set { this.mTechPrereqId_ = value; }
		}
		#endregion

		[Meta.BProtoPowerReference]
		public List<BProtoPowerID> SupportPowerIDs { get; private set; } = [];

		public bool IsEmpty { get { return this.SupportPowerIDs.Count == 0; } }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttribute("IconLocation", ref this.mIconLocation_);

			xs.StreamDbid(s, "Power", this.SupportPowerIDs, DatabaseObjectKind.POWER, isOptional: false, xmlSource: XML.XmlUtil.K_SOURCE_CURSOR);
		}
		#endregion

		#region IComparable Members
		public int CompareTo(BLeaderSupportPower other)
		{
			if (this.IconLocation != other.IconLocation)
				this.IconLocation.CompareTo(other.IconLocation);

			if (this.TechPrereqId != other.TechPrereqId)
				this.TechPrereqId.CompareTo(other.TechPrereqId);

			if (this.SupportPowerIDs.Count != other.SupportPowerIDs.Count)
				this.SupportPowerIDs.Count.CompareTo(other.SupportPowerIDs.Count);

			int aHash = PhxUtil.CalculateHashCodeForDbiDs(this.SupportPowerIDs);
			int bHash = PhxUtil.CalculateHashCodeForDbiDs(other.SupportPowerIDs);
			return aHash.CompareTo(bHash);
		}
		#endregion

		#region IEquatable Members
		public bool Equals(BLeaderSupportPower other)
		{
			return this.IconLocation == other.IconLocation
				&&
				this.TechPrereqId == other.TechPrereqId
				&&
				this.SupportPowerIDs.EqualsList(other.SupportPowerIDs);
		}
		#endregion
	};

	public sealed class BLeaderStartingSquad
		: IO.ITagElementStringNameStreamable
		, IComparable<BLeaderStartingSquad>
		, IEquatable<BLeaderStartingSquad>
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "StartingSquad",
		};
		#endregion

		#region SquadID
		int mSquadId_ = TypeExtensions.K_NONE;
		[Meta.BProtoSquadReference]
		public int SquadId
		{
			get { return this.mSquadId_; }
			set { this.mSquadId_ = value; }
		}
		#endregion

		#region FlyIn
		bool mFlyIn_;
		public bool FlyIn
		{
			get { return this.mFlyIn_; }
			set { this.mFlyIn_ = value; }
		}
		#endregion

		#region Offset
		BVector mOffset_;
		public BVector Offset
		{
			get { return this.mOffset_; }
			set { this.mOffset_ = value; }
		}
		#endregion

		public bool IsInvalid { get { return PhxUtil.IsUndefinedReferenceHandleOrNone(this.SquadId); } }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamBVector("Offset", ref this.mOffset_, xmlSource: XML.XmlUtil.K_SOURCE_ATTR);
			s.StreamAttributeOpt("FlyIn", ref this.mFlyIn_, Predicates.IsTrue);

			XML.BXmlSerializerInterface.StreamSquadId(s, xs, ref this.mSquadId_);
		}
		#endregion

		#region IComparable Members
		public int CompareTo(BLeaderStartingSquad other)
		{
			if (this.FlyIn != other.FlyIn)
				this.FlyIn.CompareTo(other.FlyIn);

			if (this.Offset != other.Offset)
				this.Offset.CompareTo(other.Offset);

			return this.SquadId.CompareTo(other.SquadId);
		}
		#endregion

		#region IEquatable Members
		public bool Equals(BLeaderStartingSquad other)
		{
			return this.FlyIn == other.FlyIn
				&&
				this.Offset == other.Offset
				&&
				this.SquadId == other.SquadId;
		}
		#endregion
	};

	public sealed class BLeaderStartingUnit
		: IO.ITagElementStringNameStreamable
		, IComparable<BLeaderStartingUnit>
		, IEquatable<BLeaderStartingUnit>
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "StartingUnit",
		};
		#endregion

		#region DoppleOnStart
		bool mDoppleOnStart_;
		public bool DoppleOnStart
		{
			get { return this.mDoppleOnStart_; }
			set { this.mDoppleOnStart_ = value; }
		}
		#endregion

		#region ObjectTypeID
		int mObjectTypeId_ = TypeExtensions.K_NONE;
		[Meta.BProtoObjectReference]
		public int ObjectTypeId
		{
			get { return this.mObjectTypeId_; }
			set { this.mObjectTypeId_ = value; }
		}
		#endregion

		#region BuildOtherID
		int mBuildOtherId_ = TypeExtensions.K_NONE;
		[Meta.BProtoObjectReference]
		public int BuildOtherId
		{
			get { return this.mBuildOtherId_; }
			set { this.mBuildOtherId_ = value; }
		}
		#endregion

		#region Offset
		BVector mOffset_;
		public BVector Offset
		{
			get { return this.mOffset_; }
			set { this.mOffset_ = value; }
		}
		#endregion

		public bool IsInvalid { get { return PhxUtil.IsUndefinedReferenceHandleOrNone(this.ObjectTypeId); } }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamBVector("Offset", ref this.mOffset_, xmlSource: XML.XmlUtil.K_SOURCE_ATTR);

			xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mObjectTypeId_, DatabaseObjectKind.OBJECT, false, XML.XmlUtil.K_SOURCE_CURSOR);
			xs.StreamDbid(s, "BuildOther", ref this.mBuildOtherId_, DatabaseObjectKind.OBJECT, xmlSource: XML.XmlUtil.K_SOURCE_ATTR);

			s.StreamAttributeOpt("DoppleOnStart", ref this.mDoppleOnStart_, Predicates.IsTrue);
		}
		#endregion

		#region IComparable Members
		public int CompareTo(BLeaderStartingUnit other)
		{
			if (this.DoppleOnStart != other.DoppleOnStart)
				this.DoppleOnStart.CompareTo(other.DoppleOnStart);

			if (this.Offset != other.Offset)
				this.Offset.CompareTo(other.Offset);

			if (this.ObjectTypeId != other.ObjectTypeId)
				this.ObjectTypeId.CompareTo(other.ObjectTypeId);

			return this.BuildOtherId.CompareTo(other.BuildOtherId);
		}
		#endregion

		#region IEquatable Members
		public bool Equals(BLeaderStartingUnit other)
		{
			return this.DoppleOnStart == other.DoppleOnStart
				&&
				this.Offset == other.Offset
				&&
				this.ObjectTypeId == other.ObjectTypeId
				&&
				this.BuildOtherId == other.BuildOtherId;
		}
		#endregion
	};
}
