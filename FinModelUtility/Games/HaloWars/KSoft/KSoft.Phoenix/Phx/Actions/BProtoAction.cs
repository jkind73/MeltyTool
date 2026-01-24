using System;

namespace KSoft.Phoenix.Phx
{
	public sealed partial class BProtoAction
		: Collections.BListAutoIdObject
	{
		public static readonly Predicate<BActionType> KNotInvalidActionType = e => e != BActionType.INVALID;
		static readonly Predicate<BSquadMode> KNotInvalidSquadMode = e => e != BSquadMode.INVALID;

		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "Action",
			dataName = "Name",
			flags = XML.BCollectionXmlParamsFlags.USE_ELEMENT_FOR_DATA |
				XML.BCollectionXmlParamsFlags.FORCE_NO_ROOT_ELEMENT_STREAMING,
		};
		#endregion

		#region Properties
		BActionType mActionType_ = BActionType.INVALID;
		float mProjectileSpread_ = PhxUtil.K_INVALID_SINGLE;

		[Meta.BProtoSquadReference]
		int mSquadTypeId_ = TypeExtensions.K_NONE;
		[Meta.BWeaponTypeReference]
		int mWeaponId_ = TypeExtensions.K_NONE;
		[Meta.BProtoActionReference]
		int mLinkedActionId_ = TypeExtensions.K_NONE;

		BSquadMode mSquadMode_ = BSquadMode.INVALID;
		BSquadMode mNewSquadMode_ = BSquadMode.INVALID;
#if false
		int mNewTacticStateID = TypeExtensions.kNone;
#endif

		float mWorkRate_ = PhxUtil.K_INVALID_SINGLE;
		float mWorkRateVariance_ = PhxUtil.K_INVALID_SINGLE;
		float mWorkRange_ = PhxUtil.K_INVALID_SINGLE;

		float mDamageModifiersDmg_;
		float mDamageModifiersDmgTaken_;
		bool mDamageModifiersByCombatValue_;

		int mResourceId_ = TypeExtensions.K_NONE;
		bool mDefault_;

		int mSlaveAttackActionId_ = TypeExtensions.K_NONE;
		int mBaseDpsWeaponId_ = TypeExtensions.K_NONE;

		BActionType mPersistentActionType_ = BActionType.INVALID;

		float mDuration_ = PhxUtil.K_INVALID_SINGLE;
		float mDurationSpread_ = PhxUtil.K_INVALID_SINGLE;

		int mAutoRepairIdleTime_ = TypeExtensions.K_NONE;
		float mAutoRepairThreshold_ = PhxUtil.K_INVALID_SINGLE;
		float mAutoRepairSearchDistance_ = PhxUtil.K_INVALID_SINGLE;
		int mInvalidTargetObjectId_ = TypeExtensions.K_NONE;

		int mProtoObjectId_ = TypeExtensions.K_NONE;
		bool mProtoObjectIsSquad_;
#if false
		int mCountStringID = TypeExtensions.kNone;
#endif
		int mMaxNumUnitsPerformAction_ = TypeExtensions.K_NONE;
		float mDamageCharge_ = PhxUtil.K_INVALID_SINGLE;
		#endregion

		#region BListAutoIdObject Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			var xs = s.GetSerializerInterface();
			var td = KSoft.Debug.TypeCheck.CastReference<BTacticData>(s.UserData);

			s.StreamElementEnumOpt("ActionType", ref this.mActionType_, KNotInvalidActionType);
			s.StreamElementOpt("ProjectileSpread", ref this.mProjectileSpread_, PhxPredicates.IsNotInvalid);

			xs.StreamDbid(s, "SquadType", ref this.mSquadTypeId_, DatabaseObjectKind.SQUAD);
			td.StreamId(s, "Weapon", ref this.mWeaponId_, TacticDataObjectKind.WEAPON);
			td.StreamId(s, "LinkedAction", ref this.mLinkedActionId_, TacticDataObjectKind.ACTION);

			s.StreamElementEnumOpt("SquadMode", ref this.mSquadMode_, KNotInvalidSquadMode);
			s.StreamElementEnumOpt("NewSquadMode", ref this.mNewSquadMode_, KNotInvalidSquadMode);
#if false
			td.StreamID(s, "NewTacticState", ref mNewTacticStateID, BTacticData.ObjectKind.TacticState);
#endif

			#region Work
			s.StreamElementOpt("WorkRate", ref this.mWorkRate_, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("WorkRateVariance", ref this.mWorkRateVariance_, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("WorkRange", ref this.mWorkRange_, PhxPredicates.IsNotInvalid);
			#endregion

			#region DamageModifiers
			using (var bm = s.EnterCursorBookmarkOpt("DamageModifiers", this, o => this.mDamageModifiersDmg_ != PhxUtil.K_INVALID_SINGLE)) if (bm.IsNotNull)
			{
				s.StreamAttribute("damage", ref this.mDamageModifiersDmg_);
				s.StreamAttributeOpt("damageTaken", ref this.mDamageModifiersDmgTaken_, PhxPredicates.IsNotInvalid);
				s.StreamAttributeOpt("byCombatValue", ref this.mDamageModifiersByCombatValue_, Predicates.IsTrue);
			}
			#endregion

			xs.StreamTypeName(s, "Resource", ref this.mResourceId_, GameDataObjectKind.COST);
			// if element equals 'true' this is the default action
			s.StreamElementOpt("Default", ref this.mDefault_, Predicates.IsTrue);

			td.StreamId(s, "SlaveAttackAction", ref this.mSlaveAttackActionId_, TacticDataObjectKind.ACTION);
			td.StreamId(s, "BaseDPSWeapon", ref this.mBaseDpsWeaponId_, TacticDataObjectKind.WEAPON);

			s.StreamElementEnumOpt("PersistentActionType", ref this.mPersistentActionType_, KNotInvalidActionType);

			#region Duration
			using (var bm = s.EnterCursorBookmarkOpt("Duration", this, o => this.mDuration_ != PhxUtil.K_INVALID_SINGLE)) if (bm.IsNotNull)
			{
				s.StreamCursor(ref this.mDuration_);
				s.StreamAttributeOpt("DurationSpread", ref this.mDurationSpread_, PhxPredicates.IsNotInvalid);
			}
			#endregion

			#region AutoRepair
			using (var bm = s.EnterCursorBookmarkOpt("AutoRepair", this, o => this.mAutoRepairIdleTime_ != PhxUtil.K_INVALID_SINGLE)) if (bm.IsNotNull)
			{
				s.StreamAttribute("AutoRepairIdleTime", ref this.mAutoRepairIdleTime_);
				s.StreamAttribute("AutoRepairThreshold", ref this.mAutoRepairThreshold_);
				s.StreamAttribute("AutoRepairSearchDistance", ref this.mAutoRepairSearchDistance_);
			}
			#endregion
			xs.StreamDbid(s, "InvalidTarget", ref this.mInvalidTargetObjectId_, DatabaseObjectKind.OBJECT);

			#region ProtoObject
			using (var bm = s.EnterCursorBookmarkOpt("ProtoObject", this, o => this.mProtoObjectId_.IsNotNone())) if (bm.IsNotNull)
			{
				// TODO: This IS optional, right? Only on 'true'?
				// inner text: if 0, proto object, if not, proto squad
				s.StreamAttributeOpt("Squad", ref this.mProtoObjectIsSquad_, Predicates.IsTrue);
				xs.StreamDbid(s, null, ref this.mSquadTypeId_,
				              this.mProtoObjectIsSquad_ ? DatabaseObjectKind.SQUAD : DatabaseObjectKind.OBJECT, false, XML.XmlUtil.K_SOURCE_CURSOR);
			}
			#endregion
#if false
			xs.StreamXmlForStringID(s, "Count", ref mCountStringID);
#endif
			s.StreamElementOpt("MaxNumUnitsPerformAction", ref this.mMaxNumUnitsPerformAction_, Predicates.IsNotNone);
			s.StreamElementOpt("DamageCharge", ref this.mDamageCharge_, PhxPredicates.IsNotInvalid);
		}
		#endregion
	};
}