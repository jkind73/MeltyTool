
namespace KSoft.Phoenix.Phx
{
	public enum BGameType
	{
		SKIRMISH,
		CAMPAIGN,
		SCENARIO,
	};

	public enum BMapType
	{
		UNKNOWN,

		FINAL,
		PLAYTEST,
		DEVELOPMENT,
		CAMPAIGN,

		DLC = FINAL,
	};

	public enum BScenarioObjectFlags
	{
		NO_TIE_TO_GROUND,
		INCLUDE_IN_SIM_REP,
	};

	public enum BScenarioPlayerPlacementType
	{
		INVALID,

		GROUPED,
		CONSECUTIVE, // int "Spacing" attribute
		RANDOM,
	};

	public enum BMissionType
	{
		INVALID,
		ATTACK,
		DEFEND,
		SCOUT,
		CLAIM,
		POWER,
	};

	public enum BMissionState
	{
		INVALID,
		SUCCESS,
		FAILURE,
		CREATE,
		WORKING,
		WITHDRAW,
		RETREAT,
	};

	public enum BMissionTargetType
	{
		INVALID,
		AREA,
		KB_BASE,
		CAPTURE_NODE,
	};
}