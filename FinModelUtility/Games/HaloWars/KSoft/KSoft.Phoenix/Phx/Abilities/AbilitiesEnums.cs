
namespace KSoft.Phoenix.Phx
{
	public enum BAbilityTargetType
	{
		NONE,
		LOCATION,
		UNIT,
		UNIT_OR_LOCATION,
	};

	public enum BMovementModifierType
	{
		ABILITY,
		MODE,
	};

	public enum BRecoverType
	{
		MOVE,
		ATTACK,
		ABILITY,
	};

	public enum BSquadMode
	{
		INVALID = TypeExtensions.K_NONE,

		NORMAL = 0,
		STAND_GROUND,
		LOCKDOWN,
		SNIPER,
		HIT_AND_RUN,
		PASSIVE,
		COVER,
		ABILITY,
		CARRYING_OBJECT,
		POWER,
		SCARAB_SCAN,
		SCARAB_TARGET,
		SCARAB_KILL,
	};
}