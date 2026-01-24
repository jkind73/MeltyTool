
namespace KSoft.Phoenix.Phx
{
	public enum BProtoObjectCommandType
	{
		INVALID = TypeExtensions.K_NONE,

		RESEARCH = 0,
		TRAIN_UNIT,
		BUILD,
		TRAIN_SQUAD,
		UNLOAD,
		REINFORCE,
		CHANGE_MODE,
		ABILITY,
		KILL,
		CANCEL_KILL,
		TRIBUTE,
		CUSTOM_COMMAND,
		POWER,
		BUILD_OTHER,
		TRAIN_LOCK,
		TRAIN_UNLOCK,
		RALLY_POINT,
		CLEAR_RALLY_POINT,
		DESTROY_BASE,
		CANCEL_DESTROY_BASE,
		REVERSE_HOT_DROP,
	};
}

namespace KSoft.Phoenix
{
	partial class TypeExtensionsPhx
	{
		public static bool RequiresValidId(this Phx.BProtoObjectCommandType type)
		{
			switch(type)
			{
				case Phx.BProtoObjectCommandType.RESEARCH:
				case Phx.BProtoObjectCommandType.TRAIN_UNIT:
				case Phx.BProtoObjectCommandType.BUILD:
				case Phx.BProtoObjectCommandType.BUILD_OTHER:
				case Phx.BProtoObjectCommandType.TRAIN_SQUAD:
				case Phx.BProtoObjectCommandType.ABILITY:
				case Phx.BProtoObjectCommandType.POWER:
					return true;

				default:
					return false;
			}
		}

		public static Phx.DatabaseObjectKind GetIdKind(this Phx.BProtoObjectCommandType type)
		{
			switch (type)
			{
				case Phx.BProtoObjectCommandType.RESEARCH:
					return Phx.DatabaseObjectKind.TECH;

				case Phx.BProtoObjectCommandType.TRAIN_UNIT:
				case Phx.BProtoObjectCommandType.BUILD:
				case Phx.BProtoObjectCommandType.BUILD_OTHER:
					return Phx.DatabaseObjectKind.OBJECT;

				case Phx.BProtoObjectCommandType.TRAIN_SQUAD:
					return Phx.DatabaseObjectKind.SQUAD;

				case Phx.BProtoObjectCommandType.ABILITY:
					return Phx.DatabaseObjectKind.ABILITY;

				case Phx.BProtoObjectCommandType.POWER:
					return Phx.DatabaseObjectKind.POWER;

				default:
					return Phx.DatabaseObjectKind.NONE;
			}
		}
	};
}