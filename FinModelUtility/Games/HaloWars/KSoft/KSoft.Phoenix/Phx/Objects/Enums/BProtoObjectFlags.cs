using System;
using XmlIgnore = System.Xml.Serialization.XmlIgnoreAttribute;

namespace KSoft.Phoenix.Phx
{
	public enum BProtoObjectFlags
	{
		// 0x88
		ATTACK_WHILE_CLOAKED, // 0
		MOVE_WHILE_CLOAKED, // 1
		AUTO_CLOAK, // 2
		// 3?
		ABILITY_DISABLED, // 4

		// 0x89
		SOUND_BEHIND_FOW,// = 1<<0,
		VISIBLE_TO_ALL,// = 1<<1,
		DOPPLED,// = 1<<2,
		GRAY_MAP_DOPPLED,// = 1<<3,
		NO_TIE_TO_GROUND,// = 1<<4,
		FORCE_TO_GAIA_PLAYER,// = 1<<5,

		// 0x8A
		NO_GRAY_MAP_DOPPLED_IN_CAMPAIGN, // 6
		NEUTRAL,// = 1<<7,

		// 0x348
		MANUAL_BUILD, // 0
		BUILD, // 1
		// HasTrainSquadCommand // 2
		SELECTED_RECT, // 3
		ORIENT_UNIT_WITH_GROUND, // 4
		NON_COLLIDEABLE, // 5, actually "Collideable" in code
		PLAYER_OWNS_OBSTRUCTION, // 6
		DONT_ROTATE_OBSTRUCTION, // 7, actually "RotateObstruction" in code

		// 0x349
		HAS_HP_BAR, // 0
		UNLIMITED_RESOURCES, // 1
		DIE_AT_ZERO_RESOURCES, // 2
		// 3?
		// 4?
		IMMOVEABLE, // 5

		// 0x34A
		HIGH_ARC, // 0
		IS_AFFECTED_BY_GRAVITY, // 1
		// 2?
		// 3?
		INVULNERABLE, // 4
		AUTO_REPAIR, // 5
		BLOCK_MOVEMENT, // 6
		BLOCK_LOS, // 7

		// 0x34B
		KILL_GARRISONED, // 0
		DAMAGE_GARRISONED, // 1
		TRACKING, // 2
		SHOW_RANGE, // 3
		PASSIVE_GARRISONED, // 4
		UNGARRISON_TO_GAIA, // 5
		CAPTURABLE, // 6
		// 7?

		#region 0x34C
		ROCKET_ON_DEATH, // 0
		VISIBLE_FOR_TEAM_ONLY, // 1
		VISIBLE_FOR_OWNER_ONLY, // 2
		DESTRUCTIBLE, // 3
		KB_CREATES_BASE, // 4
		EXTERNAL_SHIELD, // 5
		KB_AWARE, // 6
		UI_DECAL, // 7
		#endregion

		// 0x34D
		START_AT_MAX_AMMO, // 0
		TARGETS_FOOT_OF_UNIT,// = 1<<1,
		// 2?
		HAS_TRACK_MASK, // 3
		NO_CULL, // 4
		FADE_ON_DEATH, // 5
		DONT_ATTACK_WHILE_MOVING, // 6
		BEAM, // 7

		#region 0x34E
		REPAIRABLE,// = 1<<0,
		NO_RENDER,// = 1<<1,
		OBSCURABLE,// = 1<<2,
		ALWAYS_VISIBLE_ON_MINIMAP,// = 1<<3,
		FORCE_ANIM_RATE, // 4
		NO_ACTION_OVERRIDE_MOVE, // 5
		UPDATE, // 6
		INVULNERABLE_WHEN_GAIA, // 7
		#endregion

		// 0x34F
		FORCE_CREATE_OBSTRUCTION, // 0
		DAMAGE_LINKED_SOCKETS_FIRST,// = 1<<1,
		NO_BUILD_UNDER_ATTACK,// = 1<<2,
		// 3?
		AIR_MOVEMENT,// = 1<<4,
		WALK_TO_TURN,// = 1<<5,
		SCALE_BUILD_ANIM_RATE, // 6
		DO_NOT_FILTER_ORIENT, // 7, actually "FilterOrient" in code

		// 0x350
		IS_STICKY,// = 1<<0,
		EXPIRE_ON_TIMER,// = 1<<1,
		EXPLODE_ON_TIMER,// = 1<<2,
		COMMANDABLE_BY_ANY_PLAYER,// = 1<<3,
		SINGLE_SOCKET_BUILDING,// = 1<<4,
		ALWAYS_ATTACK_REVIVE_UNITS,// = 1<<5,
		DONT_AUTO_ATTACK_ME,// = 1<<6,
		// 7?

		#region 0x351
		USE_BUILDING_ACTION, // 0
		NON_ROTATABLE, // 1, actually "Rotatable" in code
		SHATTER_DEATH_REPLACEMENT, // 2
		DAMAGED_DEATH_REPLACEMENT,// = 1<<3,
		HAS_PIVOTING_ENGINES,// = 1<<4,
		OVERRIDES_REVIVE,// = 1<<5,
		LINEAR_COST_ESCALATION,// = 1<<6,
		IS_NEEDLER,// = 1<<7,
		#endregion

		#region 0x352
		FORCE_UPDATE_CONTAINED_UNITS,// = 1<<0,
		IS_FLAME_EFFECT,// = 1<<1,
		SINGLE_STICK,// = 1<<2,
		DIE_LAST, // 3
		CHILD_FOR_DAMAGE_TAKEN_SCALAR, // 4
		SELF_PARKING_LOT, // 5
		KILL_CHILD_OBJECTS_ON_DEATH, // 6
		LOCKDOWN_MENU, // 7
		#endregion

		#region 0x353
		TRIGGERS_BATTLE_MUSIC_WHEN_ATTACKED,// = 1<<0,
		NO_RENDER_FOR_OWNER,// = 1<<1,
		NO_CORPSE,// = 1<<2,
		SHOW_RESCUED_COUNT,// = 1<<3,
		MUST_OWN_TO_SELECT,// = 1<<4,
		ABILITY_ATTACKS_MELEE_ONLY,// = 1<<5,
		REGULAR_ATTACKS_MELEE_ONLY,// = 1<<6,
		FLATTEN_TERRAIN,// = 1<<7,
		#endregion

		#region 0x354
		CAN_SET_AS_RALLY_POINT,// = 1<<0,
		IGNORE_SQUAD_AI,// = 1<<1,
		NOT_SELECTABLE_WHEN_CHILD_OBJECT,// = 1<<2,
		TELEPORTER,// = 1<<3,
		ONE_SQUAD_CONTAINMENT,// = 1<<4,
		PROJECTILE_TUMBLES,// = 1<<5,
		PROJECTILE_OBSTRUCTABLE,// = 1<<6,
		AUTO_EXPLORATION_GROUP,// = 1<<7,
		#endregion

		#region 0x355
		SELECTION_DONT_CONFORM_TO_TERRAIN, // 0
		PHYSICS_DETONATE_ON_DEATH,// = 1<<1,
		OBSTRUCTS_AIR,// = 1<<2,
		NO_RANDOM_MOVE_ANIM_START,// = 1<<3, actually "RandomMoveAnimStart" in code
		HIDE_ON_IMPACT,// = 1<<4,
		PERMANENT_SOCKET,// = 1<<5,
		SELF_DAMAGE,// = 1<<6,
		SECONDARY_BUILDING_QUEUE,// = 1<<7,
		#endregion

		#region 0x356
		USE_AUTO_PARKING_LOT,// = 1<<0,
		USE_BUILD_ROTATION,// = 1<<1,
		CARRY_NO_RENDER_TO_CHILDREN,// = 1<<2,
		USE_RELAXED_SPEED_GROUP,// = 1<<3,
		APPEARS_BELOW_DECALS,// = 1<<4,
		IK_TRANSITION_TO_IDLE,// = 1<<5,
		SYNC_ANIM_RATE_TO_PHYSICS,// = 1<<6,
		TURN_IN_PLACE,// = 1<<7,
		#endregion

		// 0x357
		NO_STICKY_CAM,// = 1<<3, actually "StickyCam" in code
		// 4?
		CHECK_LOS_AGAINST_BASE,// = 1<<5,
		//CheckPos, // 6, see: DeathSpawnSquad
		KILL_ON_DETACH,// = 1<<7,

		#region Alpha only
		[Obsolete] C_FLAG_PHYSICS_CONTROL,
		#endregion

		//[Obsolete] NonCollidable = NonCollideable, // Fixed in HW's XmlFiles.cs
		[Obsolete, XmlIgnore] NON_SOLID,
		[Obsolete, XmlIgnore] RENDER_BELOW_DECALS,
	};
}