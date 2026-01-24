
namespace KSoft.Phoenix.Phx
{
	public enum BMinigameType
	{
		NONE,

		ONE_BUTTON_PRESS,
		TWO_BUTTON_PRESS,
		THREE_BUTTON_PRESS,
	};

	public enum BPowerFlags
	{
		SEQUENTIAL_RECHARGE,
		INFINITE_USES,
		UNIT_POWER,
		NOT_DISRUPTABLE,
		MULTI_RECHARGE_POWER,
		SHOW_LIMIT,
		//ShowTargetHighlight,
		LEADER_POWER,
	};

	// The following flags respect the true/false value in the XmlText
	public enum BPowerToggableFlags
	{
		CAMERA_ENABLE_USER_SCROLL,
		CAMERA_ENABLE_USER_YAW,
		CAMERA_ENABLE_USER_ZOOM,
		CAMERA_ENABLE_AUTO_ZOOM_INSTANT,
		CAMERA_ENABLE_AUTO_ZOOM,

		SHOW_IN_POWER_MENU,
		SHOW_TRANSPORT_ARROWS,
	};

	public enum BPowerType
	{
		INVALID,

		CLEANSING,
		ORBITAL,
		CARPET_BOMBING,
		CRYO,
		RAGE,
		WAVE,
		DISRUPTION,
		TRANSPORT,
		ODST,
		REPAIR,
	};

	public enum ProtoPowerDataType
	{
		INVALID,
		FLOAT,
		INT,
		PROTO_OBJECT,
		PROTO_SQUAD,
		TECH,
		BOOL,
		[System.Obsolete]
		COST,
		OBJECT_TYPE,
		SOUND,
		TEXTURE,
	};
}