
using XmlIgnore = System.Xml.Serialization.XmlIgnoreAttribute;

namespace KSoft.Phoenix.Phx
{
	public enum BProtoTechEffectDisplayNameIconType
	{
		UNIT,
		BUILDING,
		MISC,
		TECH,
	};

	public enum BProtoTechEffectSetAgeLevel
	{
		INVALID = TypeExtensions.K_NONE,
		NONE = 0,

		AGE1, // not explicitly parsed by the engine
		AGE2,
		AGE3,
		AGE4,
	};

	public enum BProtoTechEffectType
	{
		DATA,
		TRANSFORM_UNIT,
		TRANSFORM_PROTO_UNIT,
		TRANSFORM_PROTO_SQUAD,
		BUILD,
		SET_AGE,
		GOD_POWER,
		TECH_STATUS,
		ABILITY,
		SHARED_LOS,
		ATTACH_SQUAD,
	};

	public enum BProtoTechEffectTargetType
	{
		NONE = TypeExtensions.K_NONE,

		PROTO_UNIT,
		PROTO_SQUAD,
		UNIT,
		TECH,
		TECH_ALL,
		PLAYER,
	};

	public enum BProtoTechFlags
	{
		// 0x1C
		NO_SOUND,// = 1<<0,
		[XmlIgnore] FORBID,// = 1<<1,
		PERPETUAL,// = 1<<2,
		OR_PREREQS,// = 1<<3,
		SHADOW,// = 1<<4,
		/// <summary>Tech applies to a unique, ie specific, unit</summary>
		UNIQUE_PROTO_UNIT_INSTANCE,// = 1<<5,
		UNOBTAINABLE,// = 1<<6,
		[XmlIgnore] OWN_STATIC_DATA,// = 1<<7,

		// 0x1D
		INSTANT,// = 1<<7,

		// 0x78
		HIDDEN_FROM_STATS,// = 1<<0, // actually just appears to be a bool field
	};

	public enum BProtoTechStatus
	{
		INVALID = TypeExtensions.K_NONE,

		UN_OBTAINABLE = 0,
		OBTAINABLE,
		AVAILABLE,
		RESEARCHING,
		ACTIVE,
		DISABLED,
		COOP_RESEARCHING,
	};

	public enum BProtoTechTypeCountOperator : short
	{
		E, // '0' isn't explicitly parsed
		GT,
		LT,
	};

	public enum BProtoTechAlphaMode
	{
		NONE = -1,
		EXCLUDED = 0,
		ALPHA_ONLY = 1,
	};
}