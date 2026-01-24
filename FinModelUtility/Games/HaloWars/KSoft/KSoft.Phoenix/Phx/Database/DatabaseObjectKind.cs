
namespace KSoft.Phoenix.Phx
{
	public enum DatabaseObjectKind
	{
		NONE = PhxUtil.K_OBJECT_KIND_NONE,

		// #NOTE place new DatabaseObjectKind code here

		ABILITY,
		CIV,
		DAMAGE_TYPE,
		IMPACT_EFFECT,
		LEADER,
		OBJECT,
		OBJECT_TYPE,
		POWER,
		SQUAD,
		TACTIC,
		TECH,
		TERRAIN_TILE_TYPE,
		/// <summary>Object or ObjectType</summary>
		UNIT,
		USER_CLASS,
		WEAPON_TYPE,
	};
}