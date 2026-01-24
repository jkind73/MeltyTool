using System.Collections.Generic;

using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Phx
{
	public sealed class BAbility
		: DatabaseNamedObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			rootName = "Abilities",
			elementName = "Ability",
			dataName = K_XML_ATTR_NAME_N,
		};
		public static readonly Engine.XmlFileInfo KXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.DATA,
			FileName = "Abilities.xml",
			RootName = KBListXmlParams.rootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo KProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.GAME_DATA,
			KXmlFileInfo);
		#endregion

		#region Type
		BAbilityType mType_ = BAbilityType.INVALID;
		public BAbilityType Type
		{
			get { return this.mType_; }
			set { this.mType_ = value; }
		}
		#endregion

		#region AmmoCost
		float mAmmoCost_;
		public float AmmoCost
		{
			get { return this.mAmmoCost_; }
			set { this.mAmmoCost_ = value; }
		}
		#endregion

		[Meta.BProtoObjectReference]
		public List<BProtoObjectID> ObjectIDs { get; private set; }

		#region SquadMode
		BSquadMode mSquadMode_ = BSquadMode.INVALID;
		public BSquadMode SquadMode
		{
			get { return this.mSquadMode_; }
			set { this.mSquadMode_ = value; }
		}
		#endregion

		#region RecoverStart
		BRecoverType mRecoverStart_ = BRecoverType.MOVE;
		public BRecoverType RecoverStart
		{
			get { return this.mRecoverStart_; }
			set { this.mRecoverStart_ = value; }
		}
		#endregion

		#region RecoverType
		BRecoverType mRecoverType_ = BRecoverType.MOVE;
		public BRecoverType RecoverType
		{
			get { return this.mRecoverType_; }
			set { this.mRecoverType_ = value; }
		}
		#endregion

		#region RecoverTime
		float mRecoverTime_;
		public float RecoverTime
		{
			get { return this.mRecoverTime_; }
			set { this.mRecoverTime_ = value; }
		}
		#endregion

		#region MovementSpeedModifier
		float mMovementSpeedModifier_;
		public float MovementSpeedModifier
		{
			get { return this.mMovementSpeedModifier_; }
			set { this.mMovementSpeedModifier_ = value; }
		}
		#endregion

		#region MovementModifierType
		BMovementModifierType mMovementModifierType_ = BMovementModifierType.MODE;
		public BMovementModifierType MovementModifierType
		{
			get { return this.mMovementModifierType_; }
			set { this.mMovementModifierType_ = value; }
		}
		#endregion

		#region DamageTakenModifier
		float mDamageTakenModifier_;
		public float DamageTakenModifier
		{
			get { return this.mDamageTakenModifier_; }
			set { this.mDamageTakenModifier_ = value; }
		}
		#endregion

		#region DodgeModifier
		float mDodgeModifier_;
		public float DodgeModifier
		{
			get { return this.mDodgeModifier_; }
			set { this.mDodgeModifier_ = value; }
		}
		#endregion

		#region Icon
		string mIcon_;
		[Meta.TextureReference]
		public string Icon
		{
			get { return this.mIcon_; }
			set { this.mIcon_ = value; }
		}
		#endregion

		#region TargetType
		BAbilityTargetType mTargetType_ = BAbilityTargetType.NONE;
		public BAbilityTargetType TargetType
		{
			get { return this.mTargetType_; }
			set { this.mTargetType_ = value; }
		}
		#endregion

		#region RecoverAnimAttachment
		string mRecoverAnimAttachment_;
		[Meta.AttachmentTypeReference]
		public string RecoverAnimAttachment
		{
			get { return this.mRecoverAnimAttachment_; }
			set { this.mRecoverAnimAttachment_ = value; }
		}
		#endregion

		#region RecoverStartAnim
		string mRecoverStartAnim_;
		[Meta.BAnimTypeReference]
		public string RecoverStartAnim
		{
			get { return this.mRecoverStartAnim_; }
			set { this.mRecoverStartAnim_ = value; }
		}
		#endregion

		#region RecoverEndAnim
		string mRecoverEndAnim_;
		[Meta.BAnimTypeReference]
		public string RecoverEndAnim
		{
			get { return this.mRecoverEndAnim_; }
			set { this.mRecoverEndAnim_ = value; }
		}
		#endregion

		#region Sprinting
		bool mSprinting_;
		public bool Sprinting
		{
			get { return this.mSprinting_; }
			set { this.mSprinting_ = value; }
		}
		#endregion

		#region DontInterruptAttack
		bool mDontInterruptAttack_;
		public bool DontInterruptAttack
		{
			get { return this.mDontInterruptAttack_; }
			set { this.mDontInterruptAttack_ = value; }
		}
		#endregion

		#region KeepSquadMode
		bool mKeepSquadMode_;
		public bool KeepSquadMode
		{
			get { return this.mKeepSquadMode_; }
			set { this.mKeepSquadMode_ = value; }
		}
		#endregion

		#region AttackSquadMode
		bool mAttackSquadMode_;
		public bool AttackSquadMode
		{
			get { return this.mAttackSquadMode_; }
			set { this.mAttackSquadMode_ = value; }
		}
		#endregion

		#region Duration
		float mDuration_;
		public float Duration
		{
			get { return this.mDuration_; }
			set { this.mDuration_ = value; }
		}
		#endregion

		#region SmartTargetRange
		const float C_DEFAULT_SMART_TARGET_RANGE_ = 15.0f;

		float mSmartTargetRange_ = C_DEFAULT_SMART_TARGET_RANGE_;
		public float SmartTargetRange
		{
			get { return this.mSmartTargetRange_; }
			set { this.mSmartTargetRange_ = value; }
		}
		#endregion

		#region CanHeteroCommand
		bool mCanHeteroCommand_ = true;
		public bool CanHeteroCommand
		{
			get { return this.mCanHeteroCommand_; }
			set { this.mCanHeteroCommand_ = value; }
		}
		#endregion

		#region NoAbilityReticle
		bool mNoAbilityReticle_;
		public bool NoAbilityReticle
		{
			get { return this.mNoAbilityReticle_; }
			set { this.mNoAbilityReticle_ = value; }
		}
		#endregion

		public BAbility()
		{
			var textData = this.CreateDatabaseObjectUserInterfaceTextData();
			textData.HasDisplayNameId = true;
			textData.HasDisplayName2Id = true;
			textData.HasRolloverTextId = true;

			this.ObjectIDs = [];
		}

		#region ITagElementStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);
			var xs = s.GetSerializerInterface();

			s.StreamElementEnumOpt("Type", ref this.mType_, e => e != BAbilityType.INVALID);
			s.StreamElementOpt("AmmoCost", ref this.mAmmoCost_, Predicates.IsNotZero);
			s.StreamElements("Object", this.ObjectIDs, xs, XML.BXmlSerializerInterface.StreamObjectId);
			s.StreamElementEnumOpt("SquadMode", ref this.mSquadMode_, e => e != BSquadMode.INVALID);
			s.StreamElementEnumOpt("RecoverStart", ref this.mRecoverStart_, e => e != BRecoverType.MOVE);
			s.StreamElementEnumOpt("RecoverType", ref this.mRecoverType_, e => e != BRecoverType.MOVE);
			s.StreamElementOpt("RecoverTime", ref this.mRecoverTime_, Predicates.IsNotZero);
			s.StreamElementOpt("MovementSpeedModifier", ref this.mMovementSpeedModifier_, Predicates.IsNotZero);
			s.StreamElementEnumOpt("MovementModifierType", ref this.mMovementModifierType_, e => e != BMovementModifierType.MODE);
			s.StreamElementOpt("DamageTakenModifier", ref this.mDamageTakenModifier_, Predicates.IsNotZero);
			s.StreamElementOpt("DodgeModifier", ref this.mDodgeModifier_, Predicates.IsNotZero);
			s.StreamStringOpt("Icon", ref this.mIcon_, toLower: false, type: XML.XmlUtil.K_SOURCE_ELEMENT);
			s.StreamElementEnumOpt("TargetType", ref this.mTargetType_, e => e != BAbilityTargetType.NONE);
			s.StreamStringOpt("RecoverAnimAttachment", ref this.mRecoverAnimAttachment_, toLower: false, type: XML.XmlUtil.K_SOURCE_ELEMENT);
			s.StreamStringOpt("RecoverStartAnim", ref this.mRecoverStartAnim_, toLower: false, type: XML.XmlUtil.K_SOURCE_ELEMENT);
			s.StreamStringOpt("RecoverEndAnim", ref this.mRecoverEndAnim_, toLower: false, type: XML.XmlUtil.K_SOURCE_ELEMENT);
			s.StreamElementOpt("Sprinting", ref this.mSprinting_, Predicates.IsTrue);
			s.StreamElementOpt("DontInterruptAttack", ref this.mDontInterruptAttack_, Predicates.IsTrue);
			s.StreamElementOpt("KeepSquadMode", ref this.mKeepSquadMode_, Predicates.IsTrue);
			s.StreamElementOpt("AttackSquadMode", ref this.mAttackSquadMode_, Predicates.IsTrue);
			s.StreamElementOpt("Duration", ref this.mDuration_, Predicates.IsNotZero);
			s.StreamElementOpt("SmartTargetRange", ref this.mSmartTargetRange_, v => v != C_DEFAULT_SMART_TARGET_RANGE_);
			s.StreamElementOpt("CanHeteroCommand", ref this.mCanHeteroCommand_, Predicates.IsFalse);
			s.StreamElementOpt("NoAbilityReticle", ref this.mNoAbilityReticle_, Predicates.IsTrue);
		}
		#endregion
	};
}
