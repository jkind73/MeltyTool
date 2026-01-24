using System;
using KSoft.Collections;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Phx
{
	// #TODO BGameMode also needs this

	/// <summary>
	/// UX string references common to many objects. Up to implementing objects to declare
	/// which strings they use.
	/// </summary>
	public sealed class DatabaseObjectUserInterfaceTextData
		: IO.ITagElementStringNameStreamable
	{
		#region Flags
		private enum Flags
		{
			HAS_NAME_ID,
			HAS_DISPLAY_NAME_ID,
			HAS_DISPLAY_NAME2_ID, // BAbility only
			HAS_DESCRIPTION_ID,
			HAS_LONG_DESCRIPTION_ID,
			HAS_PREREQ_TEXT_ID,
			HAS_STATS_NAME_ID,
			HAS_ROLE_TEXT_ID,
			HAS_ROLLOVER_TEXT_ID,

			// ProtoObject specific:
			HAS_ENEMY_ROLLOVER_TEXT_ID,
			HAS_GAIA_ROLLOVER_TEXT_ID,

			// ProtoPower specific:
			HAS_CHOOSE_TEXT_ID,

			K_NUMBER_OF
		};
		private BitVector32 mFlags_;

		public bool HasNameId
		{
			get { return this.mFlags_.Test(Flags.HAS_NAME_ID); }
			set { this.mFlags_.Set(Flags.HAS_NAME_ID, value); }
		}

		public bool HasDisplayNameId
		{
			get { return this.mFlags_.Test(Flags.HAS_DISPLAY_NAME_ID); }
			set { this.mFlags_.Set(Flags.HAS_DISPLAY_NAME_ID, value); }
		}

		public bool HasDisplayName2Id
		{
			get { return this.mFlags_.Test(Flags.HAS_DISPLAY_NAME2_ID); }
			set { this.mFlags_.Set(Flags.HAS_DISPLAY_NAME2_ID, value); }
		}

		public bool HasDescriptionId
		{
			get { return this.mFlags_.Test(Flags.HAS_DESCRIPTION_ID); }
			set { this.mFlags_.Set(Flags.HAS_DESCRIPTION_ID, value); }
		}

		public bool HasLongDescriptionId
		{
			get { return this.mFlags_.Test(Flags.HAS_LONG_DESCRIPTION_ID); }
			set { this.mFlags_.Set(Flags.HAS_LONG_DESCRIPTION_ID, value); }
		}

		public bool HasPrereqTextId
		{
			get { return this.mFlags_.Test(Flags.HAS_PREREQ_TEXT_ID); }
			set { this.mFlags_.Set(Flags.HAS_PREREQ_TEXT_ID, value); }
		}

		public bool HasStatsNameId
		{
			get { return this.mFlags_.Test(Flags.HAS_STATS_NAME_ID); }
			set { this.mFlags_.Set(Flags.HAS_STATS_NAME_ID, value); }
		}

		public bool HasRoleTextId
		{
			get { return this.mFlags_.Test(Flags.HAS_ROLE_TEXT_ID); }
			set { this.mFlags_.Set(Flags.HAS_ROLE_TEXT_ID, value); }
		}

		public bool HasRolloverTextId
		{
			get { return this.mFlags_.Test(Flags.HAS_ROLLOVER_TEXT_ID); }
			set { this.mFlags_.Set(Flags.HAS_ROLLOVER_TEXT_ID, value); }
		}

		public bool HasEnemyRolloverTextId
		{
			get { return this.mFlags_.Test(Flags.HAS_ENEMY_ROLLOVER_TEXT_ID); }
			set { this.mFlags_.Set(Flags.HAS_ENEMY_ROLLOVER_TEXT_ID, value); }
		}

		public bool HasGaiaRolloverTextId
		{
			get { return this.mFlags_.Test(Flags.HAS_GAIA_ROLLOVER_TEXT_ID); }
			set {
				this.mFlags_.Set(Flags.HAS_GAIA_ROLLOVER_TEXT_ID, value);

				if (value)
					this.GaiaRolloverText = new BListArray<GaiaRolloverTextData>();
				else
					this.GaiaRolloverText = null;
			}
		}

		public bool HasChooseTextId
		{
			get { return this.mFlags_.Test(Flags.HAS_CHOOSE_TEXT_ID); }
			set { this.mFlags_.Set(Flags.HAS_CHOOSE_TEXT_ID, value); }
		}
		#endregion

		#region NameID
		int mNameId_ = TypeExtensions.K_NONE;
		[Meta.LocStringReference]
		public int NameId
		{
			get {
				if (!this.HasNameId)
					return TypeExtensions.K_NONE;

				return this.mNameId_;
			}
			set {
				Contract.Requires(this.HasNameId);
				this.mNameId_ = value;
			}
		}
		#endregion

		#region DisplayNameID
		int mDisplayNameId_ = TypeExtensions.K_NONE;
		[Meta.LocStringReference]
		public int DisplayNameId
		{
			get {
				if (!this.HasDisplayNameId)
					return TypeExtensions.K_NONE;

				return this.mDisplayNameId_;
			}
			set {
				Contract.Requires(this.HasDisplayNameId);
				this.mDisplayNameId_ = value;
			}
		}
		#endregion

		#region DisplayName2ID
		int mDisplayName2Id_ = TypeExtensions.K_NONE;
		[Meta.UnusedData("unused in code")]
		[Meta.LocStringReference]
		public int DisplayName2Id
		{
			get {
				if (!this.HasDisplayName2Id)
					return TypeExtensions.K_NONE;

				return this.mDisplayName2Id_;
			}
			set {
				Contract.Requires(this.HasDisplayName2Id);
				this.mDisplayName2Id_ = value;
			}
		}
		#endregion

		#region DescriptionID
		int mDescriptionId_ = TypeExtensions.K_NONE;
		[Meta.LocStringReference]
		public int DescriptionId
		{
			get {
				if (!this.HasDescriptionId)
					return TypeExtensions.K_NONE;

				return this.mDescriptionId_;
			}
			set {
				Contract.Requires(this.HasDescriptionId);
				this.mDescriptionId_ = value;
			}
		}
		#endregion

		#region LongDescriptionID
		int mLongDescriptionId_ = TypeExtensions.K_NONE;
		[Meta.LocStringReference]
		public int LongDescriptionId
		{
			get {
				if (!this.HasLongDescriptionId)
					return TypeExtensions.K_NONE;

				return this.mLongDescriptionId_;
			}
			set {
				Contract.Requires(this.HasLongDescriptionId);
				this.mLongDescriptionId_ = value;
			}
		}
		#endregion

		#region PrereqTextID
		int mPrereqTextId_ = TypeExtensions.K_NONE;
		[Meta.LocStringReference]
		public int PrereqTextId
		{
			get {
				if (!this.HasPrereqTextId)
					return TypeExtensions.K_NONE;

				return this.mPrereqTextId_;
			}
			set {
				Contract.Requires(this.HasPrereqTextId);
				this.mPrereqTextId_ = value;
			}
		}
		#endregion

		#region StatsNameID
		int mStatsNameId_ = TypeExtensions.K_NONE;
		[Meta.LocStringReference]
		public int StatsNameId
		{
			get {
				if (!this.HasStatsNameId)
					return TypeExtensions.K_NONE;

				return this.mStatsNameId_;
			}
			set {
				Contract.Requires(this.HasStatsNameId);
				this.mStatsNameId_ = value;
			}
		}
		#endregion

		#region RoleTextID
		int mRoleTextId_ = TypeExtensions.K_NONE;
		[Meta.LocStringReference]
		public int RoleTextId
		{
			get {
				if (!this.HasRoleTextId)
					return TypeExtensions.K_NONE;

				return this.mRoleTextId_;
			}
			set {
				Contract.Requires(this.HasRoleTextId);
				this.mRoleTextId_ = value;
			}
		}
		#endregion

		#region RolloverTextID
		int mRolloverTextId_ = TypeExtensions.K_NONE;
		[Meta.LocStringReference]
		public int RolloverTextId
		{
			get {
				if (!this.HasRolloverTextId)
					return TypeExtensions.K_NONE;

				return this.mRolloverTextId_;
			}
			set {
				Contract.Requires(this.HasRolloverTextId);
				this.mRolloverTextId_ = value;
			}
		}
		#endregion

		#region EnemyRolloverTextID
		int mEnemyRolloverTextId_ = TypeExtensions.K_NONE;
		[Meta.LocStringReference]
		public int EnemyRolloverTextId
		{
			get {
				if (!this.HasEnemyRolloverTextId)
					return TypeExtensions.K_NONE;

				return this.mEnemyRolloverTextId_;
			}
			set {
				Contract.Requires(this.HasEnemyRolloverTextId);
				this.mEnemyRolloverTextId_ = value;
			}
		}
		#endregion

		public BListArray<GaiaRolloverTextData> GaiaRolloverText { get; private set; }

		#region ChooseTextID
		int mChooseTextId_ = TypeExtensions.K_NONE;
		[Meta.LocStringReference]
		public int ChooseTextId
		{
			get {
				if (!this.HasChooseTextId)
					return TypeExtensions.K_NONE;

				return this.mChooseTextId_;
			}
			set {
				Contract.Requires(this.HasChooseTextId);
				this.mChooseTextId_ = value;
			}
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			if (this.HasNameId)
				xs.StreamStringId(s, "NameID", ref this.mNameId_);
			if (this.HasDisplayNameId)
				xs.StreamStringId(s, "DisplayNameID", ref this.mDisplayNameId_);
			if (this.HasDisplayName2Id)
				xs.StreamStringId(s, "DisplayName2ID", ref this.mDisplayName2Id_);
			if (this.HasDescriptionId)
				xs.StreamStringId(s, "DescriptionID", ref this.mDescriptionId_);
			if (this.HasLongDescriptionId)
				xs.StreamStringId(s, "LongDescriptionID", ref this.mLongDescriptionId_);
			if (this.HasPrereqTextId)
				xs.StreamStringId(s, "PrereqTextID", ref this.mPrereqTextId_);
			if (this.HasRoleTextId)
				xs.StreamStringId(s, "RoleTextID", ref this.mRoleTextId_);
			if (this.HasRolloverTextId)
				xs.StreamStringId(s, "RolloverTextID", ref this.mRolloverTextId_);
			if (this.HasEnemyRolloverTextId)
				xs.StreamStringId(s, "EnemyRolloverTextID", ref this.mEnemyRolloverTextId_);
			if (this.HasGaiaRolloverTextId)
				XML.XmlUtil.Serialize(s, this.GaiaRolloverText, GaiaRolloverTextData.KBListXmlParams);
			if (this.HasStatsNameId)
				xs.StreamStringId(s, "StatsNameID", ref this.mStatsNameId_);
			if (this.HasChooseTextId)
				xs.StreamStringId(s, "ChooseTextID", ref this.mChooseTextId_);
		}
		#endregion
	};

	public sealed class GaiaRolloverTextData
		: IO.ITagElementStringNameStreamable
		, IComparable<GaiaRolloverTextData>
		, IEquatable<GaiaRolloverTextData>
	{
		/// <summary>HW1 is hard coded to only support 4 (for the first four Civs)</summary>
		public const int C_MAX_GAIA_ROLLOVER_TEXT_INDICES = 4;

		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "GaiaRolloverTextID",
		};
		#endregion

		#region CivID
		int mCivId_ = TypeExtensions.K_NONE;
		/// <summary>No CivID means it applies to all Civs</summary>
		[Meta.BCivReference]
		public int CivId
		{
			get { return this.mCivId_; }
			set { this.mCivId_ = value; }
		}
		#endregion

		#region TextID
		int mTextId_ = TypeExtensions.K_NONE;
		[Meta.LocStringReference]
		public int TextId
		{
			get { return this.mTextId_; }
			set { this.mTextId_ = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDbid(s, "civ", ref this.mCivId_, DatabaseObjectKind.CIV, true, XML.XmlUtil.K_SOURCE_ATTR);
			xs.StreamStringId(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mTextId_, XML.XmlUtil.K_SOURCE_CURSOR);
		}
		#endregion

		#region IComparable Members
		public int CompareTo(GaiaRolloverTextData other)
		{
			if (this.CivId != other.CivId)
				this.CivId.CompareTo(other.CivId);

			return this.TextId.CompareTo(other.TextId);
		}
		#endregion

		#region IEquatable Members
		public bool Equals(GaiaRolloverTextData other)
		{
			return this.CivId == other.CivId
				&&
				this.TextId == other.TextId;
		}
		#endregion
	};
}