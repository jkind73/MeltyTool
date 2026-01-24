using XmlIgnore = System.Xml.Serialization.XmlIgnoreAttribute;

namespace KSoft.Phoenix.Phx
{
	public enum BActionType : byte
	{
		#region 0x00
		[XmlIgnore] IDLE, // entity
		[XmlIgnore] LISTEN, // entity
		MOVE, GAGGLE_MOVE = MOVE, // unit...
		MOVE_AIR,
		[XmlIgnore] MOVE_WARTHOG,
		[XmlIgnore] MOVE_GHOST,
		RANGED_ATTACK, HAND_ATTACK = RANGED_ATTACK,
		[XmlIgnore] BUILDING,
		[XmlIgnore] DOT,
		[XmlIgnore] UNIT_CHANGE_MODE,
		[XmlIgnore] DEATH,
		[XmlIgnore] INFECT_DEATH,
		GARRISON,
		UNGARRISON,
		[XmlIgnore] SHIELD_REGEN,
		HONK,
		#endregion
		#region 0x10
		SPAWN_SQUAD,
		CAPTURE,
		JOIN,
		CHANGE_OWNER,
		[XmlIgnore] AMMO_REGEN,
		PHYSICS,
		[XmlIgnore] PLAY_BLOCKING_ANIMATION,
		MINES,
		DETONATE,
		GATHER,
		COLLISION_ATTACK,
		AREA_ATTACK,
		UNDER_ATTACK,
		SECONDARY_TURRET_ATTACK,
		REVEAL_TO_TEAM,
		AIR_TRAFFIC_CONTROL,
		#endregion
		#region 0x20
		HITCH,
		UNHITCH,
		SLAVE_TURRET_ATTACK,
		THROWN,
		DODGE,
		DEFLECT,
		AVOID_COLLISION_AIR,
		[XmlIgnore] PLAY_ATTACHMENT_ANIMS,
		HEAL,
		REVIVE,
		BUFF,
		INFECT,
		HOT_DROP,
		TENTACLE_DORMANT,
		[XmlIgnore] HERO_DEATH,
		STASIS,
		#endregion
		#region 0x30
		BUBBLE_SHIELD,
		BOMB,
		PLASMA_SHIELD_GEN,
		JUMP,
		AMBIENT_LIFE_SPAWNER,
		JUMP_GATHER,
		JUMP_GARRISON,
		JUMP_ATTACK,
		POINT_BLANK_ATTACK,
		ROAR,
		ENERGY_SHIELD,
		[XmlIgnore] SCALE_LOS,
		CHARGE, // ChargedRangedAttack
		TOWER_WALL,
		AOE_HEAL,
		[XmlIgnore] ATTACK, // squad
		#endregion
		#region 0x40
		CHANGE_MODE, // squad...
		[XmlIgnore] REPAIR,
		REPAIR_OTHER,
		[XmlIgnore] SQUAD_SHIELD_REGEN,
		[XmlIgnore] SQUAD_GARRISON,
		[XmlIgnore] SQUAD_UNGARRISON,
		TRANSPORT,
		[XmlIgnore] SQUAD_PLAY_BLOCKING_ANIMATION,
		[XmlIgnore] SQUAD_MOVE,
		[XmlIgnore] REINFORCE,
		[XmlIgnore] WORK,
		CARPET_BOMB,
		AIR_STRIKE,
		[XmlIgnore] SQUAD_HITCH,
		[XmlIgnore] SQUAD_UNHITCH,
		[XmlIgnore] SQUAD_DETONATE,
		#endregion
		#region 0x50
		WANDER,
		CLOAK,
		CLOAK_DETECTOR,
		DAZE,
		[XmlIgnore] SQUAD_JUMP,
		AMBIENT_LIFE,
		REFLECT_DAMAGE,
		CRYO,
		[XmlIgnore] PLATOON_MOVE,
		CORE_SLIDE, // unit...
		INFANTRY_ENERGY_SHIELD,
		DOME,
		SPIRIT_BOND, // squad
		RAGE, // unit
		//
		//
		#endregion

		INVALID = 0x5E
	};

	public enum BProtoActionFlags
	{
		// 0x138
		INSTANT_ATTACK,
		MELEE_RANGE,
		KILL_SELF_ON_ATTACK,
		DONT_CHECK_ORIENT_TOLERANCE,
		DONT_LOOP_ATTACK_ANIM,
		STOP_ATTACKING_WHEN_AMMO_DEPLETED,
		MAIN_ATTACK,
		STATIONARY,

		// 0x139
		//Strafing, // 1<<5, set when the Strafing element is streamed
		//CanOrientOwner, // 1<<6, actually tests the inner text of the element for 'false'
		INFECTION, // 1<<7

		// 0x13C
		//
		//
		//
		//
		WAIT_FOR_DODGE_COOLDOWN,
		MULTI_DEFLECT,
		WAIT_FOR_DEFLECT_COOLDOWN,
		//

		// 0x13D
		AVOID_ONLY,
		HIDE_SPAWN_UNTIL_RELEASE,
		DO_SHAKE_ON_ATTACK_TAG,
		SMALL_ARMS,

		// 0x13E
		DONT_AUTO_RESTART, // 1<<7
	};

	public enum BWeaponFlags
	{
		// 0x58
		THROW_DAMAGE_PARTS,
		THROW_ALIVE_UNITS,
		THROW_UNITS,
		USES_AMMO,
		PHYSICS_LAUNCH_AXIAL,
		ENABLE_HEIGHT_BONUS_DAMAGE,
		ALLOW_FRIENDLY_FIRE,

		// 0x59
		USE_GROUP_RANGE,
		//
		USE_DP_SAS_DPA,
		SMALL_ARMS_DEFLECTABLE,
		DEFLECTABLE,
		DODGEABLE,
		FLAIL_THROWN_UNITS,
		//PulseObject, // Set automatically when the PulseObject element is streamed

		// 0x5A
		//
		//
		TENTACLE,
		APPLY_KNOCKBACK,
		STASIS_BOMB,
		STASIS_DRAIN,
		//StasisSmartTargeting,
		CARRIED_OBJECT_AS_PROJECTILE_VISUAL,

		// 0x5B
		//
		//
		//
		//
		AOE_IGNORES_Y_AXIS, // 1<<4
		OVERRIDES_REVIVE,
		//
		AOE_LINEAR_DAMAGE,

		// 0xDC
		//
		//
		//
		AIR_BURST, // 1<<3
		PULL_UNITS,
		//
		KEEP_DPS_RAMP,
		TARGETS_FOOT_OF_UNIT,
	};

	public enum BTargetRuleFlags
	{
		TARGETS_GROUND,
		CONTAINS_UNITS,
		GAIA_OWNED,
		MERGE_SQUADS,
		MELEE_ATTACKER,
	};

	public enum BTargetRuleTargetStates
	{
		UNBUILT,
		DAMAGED,
		CAPTURABLE,
	};

	partial class BProtoAction
	{
		enum BJoinType
		{
			FOLLOW,
			MERGE,
			BOARD,
			FOLLOW_ATTACK,
		};

		enum BMergeType
		{
			NONE,

			GROUND,
			AIR,
		};
	};
}