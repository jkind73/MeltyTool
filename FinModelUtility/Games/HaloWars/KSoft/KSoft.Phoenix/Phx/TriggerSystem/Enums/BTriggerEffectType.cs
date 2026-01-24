
namespace KSoft.Phoenix.Phx
{
	public enum BTriggerEffectType
	{
		#region 0

		#region 30
		TRIGGER_ACTIVATE = 31,

		PLAY_SOUND = 33,


		CREATE_SQUAD = 36,
		#endregion
		#region 40
		#endregion
		#region 50
		SHUTDOWN = 51,



		RANDOM_LOCATION = 55,
		#endregion
		#region 60
		#endregion
		#region 70
		ATTACHMENT_ADD_TYPE = 74,


		POWER_CHARGE_USE_OF = 77,
		#endregion
		#region 80
		IS_ALIVE = 80,


		ATTACHMENT_REMOVE_TYPE = 83,

		COPY_TECH = 85,


		COPY_PROTO_OBJECT = 88,
		COPY_OBJECT_TYPE = 89,
		#endregion
		#region 90
		COPY_PROTO_SQUAD = 90,

		COPY_PLAYER = 97,
		COPY_INT = 98,
		COPY_LOCATION = 99,
		#endregion

		#endregion

		#region 100

		#region 00
		COPY_FLOAT = 103,
		#endregion
		#region 10
		#endregion
		#region 20
		ITERATOR_PLAYER_LIST = 120,
		//Iterator?List = 121,


		PLAYER_LIST_ADD = 124,
		#endregion
		#region 30
		SET_PLAYER_STATE = 130,






		CHANGE_OWNER = 137,
		#endregion
		#region 40






		COPY_SQUAD_LIST = 145,
		UNIT_LIST_GET_SIZE = 146,
		SQUAD_LIST_GET_SIZE = 147,
		UNIT_LIST_ADD = 148,
		SQUAD_LIST_ADD = 149,
		#endregion
		#region 50
		UNIT_LIST_REMOVE = 150,
		SQUAD_LIST_REMOVE = 151,
		ITERATOR_UNIT_LIST = 152,
		ITERATOR_SQUAD_LIST = 153,
		#endregion
		#region 60
		COPY_STRING = 169,
		#endregion
		#region 70
		RANDOM_INT = 178,
		MATH_INT = 179,
		#endregion
		#region 80
		GET_LOCATION = 189,
		#endregion
		#region 90
		GET_OWNER = 193,



		DEBUG_VAR_TECH = 197,
		#endregion

		#endregion

		#region 200

		#region 00
		DEBUG_VAR_PROTO_SQUAD = 202,





		DEBUG_VAR_PLAYER = 208,
		DEBUG_VAR_INT = 209,
		#endregion
		#region 10
		DEBUG_VAR_FLOAT = 218,
		#endregion
		#region 20
		#endregion
		#region 30
		DEBUG_VAR_STRING = 230,








		GET_PLAYER_CIV = 239,
		#endregion
		#region 40
		#endregion
		#region 50
		#endregion
		#region 60
		MATH_TIME = 263,

		GET_GAME_TIME = 265,
		#endregion
		#region 70
		SET_RESOURCES = 277,
		#endregion
		#region 80
		//Iterator?List = 281,

		ITERATOR_LOCATION_LIST = 288,
		#endregion
		#region 90
		#endregion

		#endregion

		#region 300

		#region 00
		PROTO_SQUAD_LIST_SHUFFLE = 305,
		#endregion
		#region 10
		#endregion
		#region 20
		#endregion
		#region 30
		UI_UNLOCK = 330,
		#endregion
		#region 40

		FILTER_CLEAR = 341,


		FILTER_ADD_PLAYERS = 344,

		FILTER_ADD_PROTO_OBJECTS = 346,
		FILTER_ADD_PROTO_SQUADS = 347,
		FILTER_ADD_OBJECT_TYPES = 348,
		UNIT_LIST_FILTER = 349,
		#endregion
		#region 50
		MATH_FLOAT = 353,
		#endregion
		#region 60
		#endregion
		#region 70
		#endregion
		#region 80
		#endregion
		#region 90
		LAUNCH_SCRIPT = 392,
		GET_PLAYER_TEAM = 393,
		#endregion

		#endregion

		#region 400

		#region 00
		#endregion
		#region 10
		IS_BUILT = 414,
		#endregion
		#region 20
		#endregion
		#region 30
		GET_PLAYERS2 = 431,
		#endregion
		#region 40
		ITERATOR_KB_BASE_LIST = 443,
		#endregion
		#region 50
		#endregion
		#region 60
		#endregion
		#region 70
		GET_PLAYER_LEADER = 475,
		#endregion
		#region 80
		COPY_PROTO_SQUAD_LIST = 482,
		#endregion
		#region 90
		//Iterator?List = 490,
		ITERATOR_PROTO_SQUAD_LIST = 491,
		//Iterator?List = 492,
		//Iterator?List = 493,

		NEXT_PROTO_SQUAD = 495,


		AS_INT = 498,
		#endregion

		#endregion

		#region 500

		#region 00
		GET_LEGAL_SQUADS = 505,
		#endregion
		#region 10
		#endregion
		#region 20
		#endregion
		#region 30
		#endregion
		#region 40
		AI_SCORE_MISSION_TARGETS = 544,
		AI_SORT_MISSION_TARGETS = 545,
		#endregion
		#region 50
		AI_GET_MISSION_TARGETS = 551,
		#endregion
		#region 60
		COPY_INTEGER_LIST = 562,

		AI_SET_SCORING_PARMS = 564,



		INT_TO_COUNT = 568,
		#endregion
		#region 70
		PROTO_SQUAD_LIST_ADD = 571,
		PROTO_SQUAD_LIST_REMOVE = 572,
		#endregion
		#region 80
		INTEGER_LIST_ADD = 580,
		INTEGER_LIST_REMOVE = 581,
		INTEGER_LIST_GET_SIZE = 582,
		#endregion
		#region 90
		#endregion

		#endregion

		#region 600

		#region 00
		#endregion
		#region 10
		#endregion
		#region 20
		#endregion
		#region 30
		INPUT_UI_SQUAD_LIST = 639,
		#endregion
		#region 40
		GET_PLAYER_ECONOMY = 647,
		#endregion
		#region 50
		TIMER_CREATE = 658,
		#endregion
		#region 60
		AI_ANALYZE_SQUAD_LIST = 668,
		#endregion
		#region 70
		AISA_GET_COMPONENT = 672,

		AI_ANALYZE_PROTO_SQUAD_LIST = 674,

		COST_TO_FLOAT = 678,
		GET_COST = 679,
		#endregion
		#region 80
		#endregion
		#region 90
		//?ListGetSize = 694,
		#endregion

		#endregion

		#region 700

		#region 00
		#endregion
		#region 10
		#endregion
		#region 20
		AI_BIND_LOG = 720,
		#endregion
		#region 30
		GET_POP = 736,
		#endregion
		#region 40
		SET_PLAYER_POP = 741,
		#endregion
		#region 50
		#endregion
		#region 60
		GET_TABLE_ROW = 762,
		#endregion
		#region 70
		#endregion
		#region 80
		#endregion
		#region 90
		#endregion

		#endregion

		#region 800

		#region 00
		#endregion
		#region 10
		#endregion
		#region 20
		#endregion
		#region 30
		#endregion
		#region 40
		//?ListGetSize = 843,
		#endregion
		#region 50
		//?ListGetSize = 859,
		#endregion
		#region 60
		#endregion
		#region 70
		//?ListGetSize = 870,


		//?ListGetSize = 873,
		#endregion
		#region 80
		//?ListGetSize = 889,
		#endregion
		#region 90
		#endregion

		#endregion

		#region 900

		#region 00
		#endregion
		#region 10
		#endregion
		#region 20
		#endregion
		#region 30
		#endregion
		#region 40
		#endregion
		#region 50
		#endregion
		#region 60
		#endregion
		#region 70
		//?ListGetSize = 970,





		GET_POWER_RADIUS = 976,
		#endregion
		#region 80
		#endregion
		#region 90
		#endregion

		#endregion

		#region 1000

		#region 00
		AI_CREATE_AREA_TARGET = 1004,
		AI_MISSION_CREATE = 1005,


		AI_CREATE_TARGET_WRAPPER = 1008,
		#endregion
		#region 10

		AI_WRAPPER_MODIFY_RADIUS = 1011,
		AI_WRAPPER_MODIFY_FLAGS = 1012,
		AI_WRAPPER_MODIFY_PARMS = 1013,

		OBJECT_TYPE_TO_PROTO_OBJECTS = 1019,
		#endregion
		#region 20
		#endregion
		#region 30
		#endregion
		#region 40
		#endregion
		#region 50
		#endregion
		#region 60
		GET_GAME_MODE = 1065,
		AI_SET_PLAYER_BUILD_SPEED_MODIFIERS = 1066,
		FILTER_ADD_CAN_CHANGE_OWNER = 1067,


		#endregion

		#endregion
	};
}