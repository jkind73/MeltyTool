
namespace KSoft.Phoenix.Phx
{
	public enum BAutoLockDown
	{
		NONE,
		LOCK_AND_UNLOCK,
		LOCK_ONLY,
		UNLOCK_ONLY,

		K_NUMBER_OF
	};

	public enum BGotoType
	{
		NONE,
		BASE,
		MILITARY,
		INFANTRY,
		VEHICLE,
		AIR,
		CIVILIAN,
		SCOUT,
		NODE,
		ALERT,
		ARMY,
		HERO,

		K_NUMBER_OF
	};

	public enum BObjectSoundType
	{
		NONE = TypeExtensions.K_NONE,

		CREATE,
		DEATH,
		SELECT,
		WORK,
		ATTACK,
		CAPTURE_COMPLETE,
		ABILITY,
		ABILITY_JACKED,

		STOP_EXIST,
		STEP_DOWN,
		STEP_UP,
		SKID_ON,
		SKID_OFF,
		ROCKET_START,
		ROCKET_END,
		/// <summary>Ramp up sound leading to moving sound</summary>
		START_MOVE,
		CORPSE_DEATH,
		/// <summary>The ramp down sound</summary>
		STOP_MOVE,
		JUMP,
		LAND,
		IMPACT_DEATH,
		PIECE_THROWN_OFF,
		LAND_HARD,

		SELECT_DOWNED,
		UNUSED24,

		PAIN,
		CLOAK,
		UN_CLOAK,
		EXIST,

		SHIELD_LOW,
		SHIELD_DEPLETED,
		SHIELD_REGEN,
	};

	public enum BPickPriority
	{
		NONE,
		BUILDING,
		RESOURCE,
		UNIT,
		RALLY,
	};

	public enum BProtoObjectClassType
	{
		INVALID = TypeExtensions.K_NONE,

		OBJECT,
		SQUAD,
		BUILDING,
		UNIT,
		PROJECTILE,
	};

	public enum BProtoObjectExitDirection
	{
		FROM_FRONT,
		FROM_FRONT_RIGHT,
		FROM_FRONT_LEFT,
		FROM_RIGHT,
		FROM_LEFT,
		FROM_BACK,

		K_NUMBER_OF
	};

	public enum BProtoObjectMovementType
	{
		NONE,
		LAND,
		AIR,
		FLOOD,
		SCARAB,
		HOVER,
	};

	public enum BProtoObjectSelectType
	{
		NONE,

		UNIT,
		COMMAND,
		TARGET,
		SINGLE_UNIT,
		SINGLE_TYPE,
	};

	public enum BRallyPointType
	{
		INVALID = TypeExtensions.K_NONE,

		MILITARY,
		CIVILIAN,
	};

	partial class BProtoObjectChildObject
	{
		public enum ChildObjectType
		{
			OBJECT,
			PARKING_LOT,
			SOCKET,
			RALLY,
			ONE_TIME_SPAWN_SQUAD,
			UNIT,
			FOUNDATION,
		};
	};

	partial class BProtoObjectTrainLimit
	{
		public enum LimitType
		{
			INVALID = TypeExtensions.K_NONE,

			UNIT,
			SQUAD,
		};
	};
}