
namespace KSoft.Phoenix.Phx
{
	public enum BCollectibleSkullEffectType
	{
		INVALID,

		SCORE,
		GRUNT_TANK,
		GRUNT_CONFETTI,
		PHYSICS,
		SCARAB_BEAM,
		MINIMAP_DISABLE,
		WEAKNESS,
		HITPOINT_MOD,
		DAMAGE_MOD,
		VETERANCY,
		ABILITY_RECHARGE,
		DEATH_EXPLODE,
		TRAIN_MOD,
		SUPPLY_MOD,
		POWER_RECHARGE,
		UNIT_MOD_WARTHOG,
		UNIT_MOD_WRAITH,
	};

	public enum BCollectibleSkullFlags
	{
		// 0x3C
		// 0
		ON_FROM_BEGINNING, // 1
		// 2
		HIDDEN, // 3
		// 4
		SELF_ACTIVE, // 5
		// 6
		ACTIVE, // 7
	};

	public enum BCollectibleSkullTarget
	{
		NONE,

		PLAYER_UNITS,
		NON_PLAYER_UNITS,
		OWNER_ONLY,
	};
}