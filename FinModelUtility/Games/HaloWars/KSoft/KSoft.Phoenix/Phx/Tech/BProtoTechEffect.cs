#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Phx
{
	// internal engine structure is only 0x34 bytes...
	public sealed partial class BProtoTechEffect
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams("Effect")
		{
			flags = 0
		};
		#endregion

		BProtoTechEffectType mType_;
		public BProtoTechEffectType Type { get { return this.mType_; } }

		DataUnion mDu_;

		#region ObjectData
		bool mAllActions_;

		string mAction_;

		public BObjectDataType SubType { get { return this.mDu_.SubType; } }

		// Amount can be negative, so use NaN as the 'invalid' value instead
		float mAmount_ = PhxUtil.K_INVALID_SINGLE_NA_N;

		BObjectDataRelative mRelativity_ = BObjectDataRelative.INVALID;

		#region Command
		public BProtoObjectCommandType CommandType { get {
			Contract.Requires(this.Type == BProtoTechEffectType.DATA);
			Contract.Requires(this.SubType == BObjectDataType.COMMAND_ENABLE || this.SubType == BObjectDataType.COMMAND_SELECTABLE);
			return this.mDu_.CommandType;
		} }

		public int CommandDataId { get {
			Contract.Requires(this.Type == BProtoTechEffectType.DATA);
			Contract.Requires(this.SubType == BObjectDataType.COMMAND_ENABLE || this.SubType == BObjectDataType.COMMAND_SELECTABLE);
			return this.mDu_.CommandData;
		} }
		public BSquadMode CommandDataSquadMode { get {
			Contract.Requires(this.Type == BProtoTechEffectType.DATA);
			Contract.Requires(this.SubType == BObjectDataType.COMMAND_ENABLE || this.SubType == BObjectDataType.COMMAND_SELECTABLE);
			return this.mDu_.CommandDataSM;
		} }

		public DatabaseObjectKind CommandDataObjectKind { get {
			Contract.Requires(this.Type == BProtoTechEffectType.DATA);
			Contract.Requires(this.SubType == BObjectDataType.COMMAND_ENABLE || this.SubType == BObjectDataType.COMMAND_SELECTABLE);
			switch (this.mDu_.CommandType)
			{
			case BProtoObjectCommandType.RESEARCH:		return DatabaseObjectKind.TECH;
			case BProtoObjectCommandType.TRAIN_UNIT:
			case BProtoObjectCommandType.BUILD:			return DatabaseObjectKind.OBJECT;
			case BProtoObjectCommandType.TRAIN_SQUAD:
			case BProtoObjectCommandType.BUILD_OTHER:	return DatabaseObjectKind.SQUAD;
			case BProtoObjectCommandType.ABILITY:		return DatabaseObjectKind.ABILITY;
			case BProtoObjectCommandType.POWER:			return DatabaseObjectKind.POWER;

			default: throw new KSoft.Debug.UnreachableException(this.mDu_.CommandType.ToString());
			}
		} }
		#endregion
		#endregion

		public BProtoTechEffectSetAgeLevel SetAgeLevel { get { return this.mDu_.SetAgeLevel; } }

		public Collections.BListArray<BProtoTechEffectTarget> Targets { get; private set; }
		public bool HasTargets { get { return this.Targets != null && this.Targets.Count != 0; } }

		public BProtoTechEffect()
		{
			this.Targets = new Collections.BListArray<BProtoTechEffectTarget>();

			this.mDu_.Initialize();
		}

		#region ITagElementStreamable<string> Members
		public DatabaseObjectKind TransformProtoObjectKind { get {
			switch (this.Type)
			{
				case BProtoTechEffectType.TRANSFORM_PROTO_UNIT:
					return DatabaseObjectKind.UNIT;
				case BProtoTechEffectType.TRANSFORM_PROTO_SQUAD:
					return DatabaseObjectKind.SQUAD;
				default:
					return DatabaseObjectKind.NONE;
			}
		} }

		void StreamXmlObjectData<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs)
			where TDoc : class
			where TCursor : class
		{
			// Unused - SubTypes (with data) which no techs in HW1 made use of
			switch (this.mDu_.SubType)
			{
			#region Unused
			case BObjectDataType.RATE_AMOUNT:
			case BObjectDataType.RATE_MULTIPLIER:
				xs.StreamTypeName(s, "Rate", ref this.mDu_.ID, GameDataObjectKind.RATE, false, XML.XmlUtil.K_SOURCE_ATTR);
				break;
			#endregion

			case BObjectDataType.COMMAND_ENABLE:
			case BObjectDataType.COMMAND_SELECTABLE: // Unused
				this.mDu_.StreamCommand(s, xs);
				break;

			case BObjectDataType.COST:
				this.mDu_.StreamCost(s, xs);
				break;

			#region Unused
			case BObjectDataType.DAMAGE_MODIFIER:
				this.mDu_.StreamDamageModifier(s, xs);
				break;
			#endregion

			case BObjectDataType.POP_CAP:
			case BObjectDataType.POP_MAX:
				// #NOTE engine parses this as "PopType", but its parser ignores case
				xs.StreamTypeName(s, "popType", ref this.mDu_.ID, GameDataObjectKind.POP, false, XML.XmlUtil.K_SOURCE_ATTR);
				break;

			#region Unused
			case BObjectDataType.UNIT_TRAIN_LIMIT:
				this.mDu_.StreamTrainLimit(s, xs, DatabaseObjectKind.OBJECT);
				break;
			case BObjectDataType.SQUAD_TRAIN_LIMIT:
				this.mDu_.StreamTrainLimit(s, xs, DatabaseObjectKind.SQUAD);
				break;
			#endregion

			case BObjectDataType.POWER_RECHARGE_TIME:
			case BObjectDataType.POWER_USE_LIMIT:
			case BObjectDataType.POWER_LEVEL:
				// #NOTE engine parses this as "Power", but its parser ignores case
				xs.StreamDbid(s, "power", ref this.mDu_.ID, DatabaseObjectKind.POWER, false, XML.XmlUtil.K_SOURCE_ATTR);
				break;
 			case BObjectDataType.IMPACT_EFFECT:
				// #NOTE engine parses this as "ImpactEffect", but its parser ignores case
				xs.StreamDbid(s, "impactEffect", ref this.mDu_.ID, DatabaseObjectKind.IMPACT_EFFECT, false, XML.XmlUtil.K_SOURCE_ATTR);
 				break;

			#region Unused
			case BObjectDataType.DISPLAY_NAME_ID:
				xs.StreamStringId(s, "StringID", ref this.mDu_.ID, XML.XmlUtil.K_SOURCE_ATTR);
				break;
			#endregion

			case BObjectDataType.ICON:
			// #NOTE engine actually doesn't explicitly handle this case when loading, but it is supported at runtime for Squads
			case BObjectDataType.ALT_ICON:
				this.mDu_.StreamIcon(s, xs);
				break;

			case BObjectDataType.TURRET_YAW_RATE:
 			case BObjectDataType.TURRET_PITCH_RATE:
				// #TODO need to validate this type so that Targets.Count==1, TargetType=ProtoUnit, and resolve the Hardpoint name
				s.StreamStringOpt("Hardpoint", ref this.mDu_.TurretRate_HardpointName, false);
 				break;

			case BObjectDataType.ABILITY_RECOVER_TIME:
				xs.StreamDbid(s, "Ability", ref this.mDu_.ID, DatabaseObjectKind.ABILITY, false, XML.XmlUtil.K_SOURCE_ATTR);
				break;

			case BObjectDataType.HP_BAR:
				// #TODO need to make an BProtoHPBar reference
				s.StreamStringOpt("hpbar", ref this.mDu_.HPBar_Name, false);
				break;

			#region Unused
			case BObjectDataType.DEATH_SPAWN:
				xs.StreamDbid(s, "squadName", ref this.mDu_.ID, DatabaseObjectKind.SQUAD, false, XML.XmlUtil.K_SOURCE_ATTR);
				break;
			#endregion

			#region Object effects
			// although some apply to squads too
			case BObjectDataType.ENABLE: // Amount>0
			case BObjectDataType.SHIELDPOINTS:
			case BObjectDataType.HITPOINTS:
			case BObjectDataType.AMMO_MAX:
			case BObjectDataType.LOS:
			case BObjectDataType.MAXIMUM_VELOCITY:
			#region Weapon effects
			case BObjectDataType.MAXIMUM_RANGE:
			case BObjectDataType.DAMAGE:
			case BObjectDataType.MIN_RANGE:
			case BObjectDataType.AOE_RADIUS:
			case BObjectDataType.AOE_PRIMARY_TARGET_FACTOR:
			case BObjectDataType.AOE_DISTANCE_FACTOR:
			case BObjectDataType.AOE_DAMAGE_FACTOR:
			case BObjectDataType.ACCURACY:
			case BObjectDataType.MAX_DEVIATION:
			case BObjectDataType.MOVING_MAX_DEVIATION:
			case BObjectDataType.DATA_ACCURACY_DISTANCE_FACTOR:
			case BObjectDataType.ACCURACY_DEVIATION_FACTOR:
			case BObjectDataType.MAX_VELOCITY_LEAD:
			case BObjectDataType.MAX_DAMAGE_PER_RAM:
			case BObjectDataType.REFLECT_DAMAGE_FACTOR:
			case BObjectDataType.AIR_BURST_SPAN:
			case BObjectDataType.DO_TRATE:
			case BObjectDataType.DO_TDURATION:
			case BObjectDataType.STASIS:

			case BObjectDataType.PROJECTILE:
			#endregion
			#region ProtoAction effects
			case BObjectDataType.WORK_RATE:
			case BObjectDataType.ACTION_ENABLE:
			case BObjectDataType.BOARD_TIME:
			#endregion
			case BObjectDataType.BUILD_POINTS:
			case BObjectDataType.AUTO_CLOAK: // Amount>0
			case BObjectDataType.MOVE_WHILE_CLOAKED: // Amount>0
			case BObjectDataType.ATTACK_WHILE_CLOAKED: // Amount>0
			case BObjectDataType.BOUNTY:
			case BObjectDataType.MAX_CONTAINED:
			case BObjectDataType.ABILITY_DISABLED: // Amount>0
			case BObjectDataType.AMMO_REGEN_RATE:
			case BObjectDataType.SHIELD_REGEN_RATE:
			case BObjectDataType.SHIELD_REGEN_DELAY:
			#endregion
			#region Squad effects
			case BObjectDataType.LEVEL:
			case BObjectDataType.TECH_LEVEL:
			#endregion
			case BObjectDataType.RESEARCH_POINTS: // Tech and TechAll only
			#region Player effects
			case BObjectDataType.RESOURCE_TRICKLE_RATE:
			case BObjectDataType.BOUNTY_RESOURCE: // Amount!=0, uses Cost
			case BObjectDataType.REPAIR_COST:
			case BObjectDataType.REPAIR_TIME:
			case BObjectDataType.WEAPON_PHYSICS_MULTIPLIER:
			#endregion
			default:
				this.mDu_.StreamCost(s, xs, isResourceOptional: true);
				break;
			}
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttributeEnum("type", ref this.mType_);

			bool streamTargets = false;
			switch (this.mType_)
			{
			case BProtoTechEffectType.DATA:
				// #NOTE engine parses these as AllActions,Action,SubType,Amount,Relativity

				// e.g., SubType==Icon and these won't be used...TODO: is Icon the only one?
				s.StreamAttributeOpt("amount", ref this.mAmount_, PhxPredicates.IsNotInvalidNaN);
				s.StreamAttributeEnum("subtype", ref this.mDu_.SubType);
				// #NOTE the engine treats AllActions being present as 'true', no matter its actual value
				s.StreamAttributeOpt("allactions", ref this.mAllActions_, Predicates.IsTrue);
				s.StreamStringOpt("action", ref this.mAction_, false, intern: true);
				s.StreamAttributeEnumOpt("relativity", ref this.mRelativity_, x => x != BObjectDataRelative.INVALID);
				this.StreamXmlObjectData(s, xs);
				streamTargets = true;
				break;
			case BProtoTechEffectType.TRANSFORM_UNIT:
			case BProtoTechEffectType.BUILD:
				xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mDu_.ToTypeID, DatabaseObjectKind.OBJECT, false, XML.XmlUtil.K_SOURCE_CURSOR);
				break;
			case BProtoTechEffectType.TRANSFORM_PROTO_UNIT:
			case BProtoTechEffectType.TRANSFORM_PROTO_SQUAD:
				xs.StreamDbid(s, "FromType", ref this.mDu_.FromTypeID, this.TransformProtoObjectKind, false, XML.XmlUtil.K_SOURCE_ATTR);
				xs.StreamDbid(s, "ToType", ref this.mDu_.ToTypeID, this.TransformProtoObjectKind, false, XML.XmlUtil.K_SOURCE_ATTR);
				break;
			#region Unused
			case BProtoTechEffectType.SET_AGE:
				s.StreamCursorEnum(ref this.mDu_.SetAgeLevel);
				break;
			#endregion
			case BProtoTechEffectType.GOD_POWER:
				xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mDu_.ID, DatabaseObjectKind.POWER, false, XML.XmlUtil.K_SOURCE_CURSOR);
				s.StreamAttribute("amount", ref this.mAmount_);
				break;
			#region Unused
			case BProtoTechEffectType.TECH_STATUS:
				xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mDu_.ID, DatabaseObjectKind.TECH, false, XML.XmlUtil.K_SOURCE_CURSOR);
				break;
			case BProtoTechEffectType.ABILITY:
				xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mDu_.ID, DatabaseObjectKind.ABILITY, false, XML.XmlUtil.K_SOURCE_CURSOR);
				break;
 			case BProtoTechEffectType.SHARED_LOS: // no extra parsed data
 				break;
			case BProtoTechEffectType.ATTACH_SQUAD:
				xs.StreamDbid(s, "squadType", ref this.mDu_.ID, DatabaseObjectKind.SQUAD, false, XML.XmlUtil.K_SOURCE_ATTR);
				streamTargets = true;
				break;
			#endregion
			}

			if (streamTargets)
				XML.XmlUtil.Serialize(s, this.Targets, BProtoTechEffectTarget.KBListXmlParams);
		}
		#endregion
	};
}