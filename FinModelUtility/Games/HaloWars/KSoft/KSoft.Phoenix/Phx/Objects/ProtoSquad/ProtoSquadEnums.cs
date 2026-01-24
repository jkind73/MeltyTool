using System;

using XmlIgnore = System.Xml.Serialization.XmlIgnoreAttribute;

namespace KSoft.Phoenix.Phx
{
	[Obsolete("Only for reverse engineering documentation purposes, don't use")]
	public enum BProtoSquadDataType
	{
		INVALID = BObjectDataType.INVALID,

		ENABLE = BObjectDataType.ENABLE,
		BUILD_POINTS = BObjectDataType.BUILD_POINTS,
		COST = BObjectDataType.COST,
		LEVEL = BObjectDataType.LEVEL,
	};

	public enum BProtoSquadFlags
	{
		// 0x68
		KB_AWARE, // 1<<2
		ONE_TIME_SPAWN_USED, // 1<<4

		// 0x144
		ALWAYS_ATTACK_REVIVE_UNITS, // 1<<0
		INSTANT_TRAIN_WITH_RECHARGE, // 1<<1
		FORCE_TO_GAIA_PLAYER, // 1<<2,
		CREATE_AUDIO_REACTIONS, // 1<<3
		CHATTER, // 1<<4
		REPAIRABLE, // 1<<5

		// 0x145
		REBEL, // 1<<0
		FORERUNNER, // 1<<1
		FLOOD, // 1<<2
		FLYING_FLOOD, // 1<<3
		DIES_WHEN_FROZEN, // 1<<4
		ONLY_SHOW_BOBBLE_HEAD_WHEN_CONTAINED, // 1<<5
		ALWAYS_RENDER_SELECTION_DECAL, // 1<<6
		SCARED_BY_ROAR, // 1<<7

		// 0x146
		ALWAYS_SHOW_HP_BAR, // 1<<3
		NO_PLATOON_MERGE, // 1<<6

		[Obsolete, XmlIgnore] JOIN_ALL,
	};

	public enum BProtoSquadFormationType
	{
		STANDARD,

		FLOCK,
		GAGGLE,
		LINE,
	};

	public enum BSquadBirthType
	{
		INVALID = TypeExtensions.K_NONE,

		TRAINED,
		FLY_IN,
	};

	public enum BSquadSoundType
	{
		NONE = TypeExtensions.K_NONE,

		EXIST,
		STOP_EXIST,
		MOVE_CHATTER,
		ATTACK_CHATTER,
		MOVE_ATTACK_CHATTER,
		IDLE_CHATTER,
		ALLY_KILLED,
		KILLED_ENEMY,
		CHEER,
		LEVEL_UP,

		REACT_BIRTH,
		REACT_DEATH,
		REACT_JOIN_BATTLE,
		REACT_POW_CARPET_BOMB,
		REACT_POW_ORBITAL,
		REACT_POW_CLEANSING,
		REACT_POW_CRYO,
		REACT_POW_RAGE,
		REACT_POW_WAVE,
		REACT_POW_DISRUPTION,
		REACT_FATALITY_UNSC,
		REACT_FATALITY_COV,
		REACT_JACKING,
		REACT_COMMANDEER,
		REACT_HOT_DROP,

		START_MOVE,
		STOP_MOVE,

		KAMIKAZE,

		START_JUMP,
		STOP_JUMP,
	};

	public enum BSquadStance
	{
		INVALID = TypeExtensions.K_NONE,

		PASSIVE,
		AGGRESSIVE,
		DEFENSIVE,
	};

	public enum BUnitRole
	{
		NORMAL,
		LEADER,
		SUPPORT,
	};
}