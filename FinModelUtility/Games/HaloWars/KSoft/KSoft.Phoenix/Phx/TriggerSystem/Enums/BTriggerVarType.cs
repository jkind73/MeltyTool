using System;
using KSoft.Phoenix.HaloWars;

namespace KSoft.Phoenix.Phx
{
	/// <remarks>
	/// The engine's parsing logic for this duplicates the IsNull and InnerText.IsNullOrEmpty checks for all
	/// the related cases. Would be more efficient to split these up into diff groups to avoid code dupe
	/// </remarks>
	public enum BTriggerVarType : byte
	{
		#region 0x0
		NONE,

		TECH,
		/// <see cref="BProtoTechStatus"/>
		TECH_STATUS,
		/// <see cref="BOperatorType"/>
		OPERATOR,
		PROTO_OBJECT,
		OBJECT_TYPE,
		PROTO_SQUAD,
		SOUND,
		ENTITY,
		ENTITY_LIST,
		TRIGGER, // int
		TIME, // int
		PLAYER,
		[Obsolete] UI_LOCATION,
		[Obsolete] UI_ENTITY,
		COST, // TODO: special KV parsing...
		#endregion

		#region 0x10
		/// <remarks>Engine defined</remarks>
		/// <see cref="HaloWars.BAnimType"/>
		ANIM_TYPE,
		/// <see cref="BActionStatus"/>
		ACTION_STATUS,
		POWER,
		BOOL,
		FLOAT,
		[Obsolete] ITERATOR,
		TEAM, // int
		PLAYER_LIST,
		TEAM_LIST,
		/// <see cref="BGameData.PlayerStates"/>
		PLAYER_STATE,
		OBJECTIVE, // int
		UNIT,
		UNIT_LIST,
		SQUAD,
		SQUAD_LIST,
		[Obsolete] UI_UNIT,
		#endregion

		#region 0x20
		[Obsolete] UI_SQUAD,
		[Obsolete] UI_SQUAD_LIST,
		STRING,
		MESSAGE_INDEX, // int
		MESSAGE_JUSTIFY, // int
		MESSAGE_POINT, // float
		COLOR,
		PROTO_OBJECT_LIST,
		OBJECT_TYPE_LIST,
		PROTO_SQUAD_LIST,
		TECH_LIST,
		/// <see cref="BMathOperatorType"/>
		MATH_OPERATOR,
		/// <see cref="BObjectDataType"/>
		OBJECT_DATA_TYPE,
		/// <see cref="BObjectDataRelative"/>
		OBJECT_DATA_RELATIVE,
		CIV,
		PROTO_OBJECT_COLLECTION,
		#endregion

		#region 0x30
		OBJECT,
		OBJECT_LIST,
		GROUP, // int
		/// <see cref="BGameData.RefCounts"/>
		REF_COUNT_TYPE,
		/// <see cref="BGameData.UnitFlags"/>
		UNIT_FLAG,
		/// <see cref="BlosType"/>
		LOS_TYPE,
		[Obsolete] ENTITY_FILTER_SET,
		/// <remarks>Population Bucket</remarks>
		POP_BUCKET,
		/// <see cref="BListPosition"/>
		LIST_POSITION,
		/// <see cref="BRelationType"/>
		DIPLOMACY,
		/// <see cref="BExposedAction"/>
		EXPOSED_ACTION,
		/// <see cref="BSquadMode"/>
		SQUAD_MODE,
		EXPOSED_SCRIPT, // int
		[Obsolete] KB_BASE,
		[Obsolete] KB_BASE_LIST, // engine still initializes the list
		/// <see cref="BDataScalar"/>
		DATA_SCALAR,
		#endregion

		#region 0x40
		[Obsolete] KB_BASE_QUERY, // Obsolete? engine sets a flag in the BTriggerVar
		DESIGN_LINE, // int
		LOC_STRING_ID, // int
		LEADER,
		CINEMATIC, // int
		/// <see cref="BFlareType"/>
		FLARE_TYPE,
		CINEMATIC_TAG, // int
		ICON_TYPE, // parses as ProtoObject...
		DIFFICULTY, // int
		INTEGER, // int (XMB specialized)
		/// <remarks>Engine defined</remarks>
		/// <see cref="BhudItem"/>
		HUD_ITEM,
		/// <see cref="BuiControlType"/>
		CONTROL_TYPE,
		[Obsolete] UI_BUTTON,
		/// <see cref="BMissionType"/>
		MISSION_TYPE,
		/// <see cref="BMissionState"/>
		MISSION_STATE,
		/// <see cref="BMissionTargetType"/>
		MISSION_TARGET_TYPE,
		#endregion

		#region 0x50
		INTEGER_LIST,
		/// <see cref="BBidType"/>
		BID_TYPE,
		/// <see cref="BBidState"/>
		BID_STATE,
		[Obsolete] BUILDING_COMMAND_STATE,
		VECTOR,
		VECTOR_LIST,
		PLACEMENT_RULE,
		[Obsolete] KB_SQUAD,
		[Obsolete] KB_SQUAD_LIST, // engine still initializes the list
		KB_SQUAD_QUERY, // Obsolete? engine sets a flag in the BTriggerVar
		[Obsolete] AI_SQUAD_ANALYSIS,
		/// <see cref="BaiSquadAnalysisComponent"/>
		AI_SQUAD_ANALYSIS_COMPONENT,
		[Obsolete] KB_SQUAD_FILTER_SET,
		/// <remarks>Engine defined</remarks>
		/// <see cref="HaloWars.BChatSpeaker"/>
		CHAT_SPEAKER,
		/// <see cref="BRumbleType"/>
		RUMBLE_TYPE,
		/// <see cref="BRumbleMotor"/>
		RUMBLE_MOTOR,
		#endregion

		#region 0x60
		/// <see cref="BProtoObjectCommandType"/>
		COMMAND_TYPE,
		/// <see cref="BObjectDataType"/>
		SQUAD_DATA_TYPE,
		/// <see cref="BEventType"/>
		EVENT_TYPE,
		TIME_LIST, // int[]
		DESIGN_LINE_LIST, // int[]
		/// <see cref="BGameStatePredicate"/>
		GAME_STATE_PREDICATE,
		FLOAT_LIST,
		[Obsolete] UI_LOCATION_MINIGAME,
		/// <see cref="BGameData.SquadFlags"/>
		SQUAD_FLAG,
		/// <remarks>Engine defined</remarks>
		/// <see cref="BFlashableUiItem"/>
		FLASHABLE_UI_ITEM, // aka FlashableItems
		TALKING_HEAD, // int (XMB specialized)
		CONCEPT, // int
		CONCEPT_LIST, // int[]
		USER_CLASS_TYPE, // int (XMB specialized)


		#endregion

		// "s/w b/w": sandwiched between

		DISTANCE = FLOAT, // s/w b/w Trigger & Time
		PERCENT = FLOAT, // s/w b/w ActionStatus & Hitpoints
		HITPOINTS = FLOAT, // s/w b/w Percent & Power

		COUNT = INTEGER, // s/w b/w Player & Location

		LOCATION = VECTOR, // s/w b/w Count & UILocation
		DIRECTION = VECTOR, // s/w b/w TalkingHead & FlareType

		LOCATION_LIST = VECTOR_LIST, // s/w b/w Group & RefCountType
	};
}