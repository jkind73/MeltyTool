
namespace KSoft.Phoenix.Runtime
{
	static class KClassVersions
	{
		public const uint B_SAVE_GAME = 0xE;

		public const uint
			B_ACTION=0x34,		B_ACTION_MANAGER=3,	BAI=5,				BAI_GLOBALS = 3,		BAI_DIFFICULTY_SETTING=1,
			BAI_MANAGER=3,		BAI_MISSION=4,		BAI_MISSION_SCORE=1,	BAI_MISSION_TARGET=2, BAI_MISSION_TARGET_WRAPPER=2,
			BAI_POWER_MISSION=3,	BAI_SCORE_MODIFIER=1,	BAI_SQUAD_ANALYSIS=1,	BAI_TOPIC=2,			B_ARMY=1,
			B_BID=1,				B_BID_MANAGER=1,		B_CHAT_MANAGER=2,		B_CONVEX_HULL=1,		B_COST=1,
			B_CUSTOM_COMMAND=1,	B_DAMAGE_TRACKER=1,	B_DOPPLE=1,			B_ENTITY=2,			B_ENTITY_FILTER=1,
				
			B_ENTITY_FILTER_SET=1,	B_FATALITY_MANAGER=2,	B_FORMATION2=1,		B_GENERAL_EVENT_MANAGER=1, B_HINT_ENGINE=1,
			B_HINT_MANAGER=1,		BKB=1,				BKB_BASE=2,			BKB_BASE_QUERY=1,			BKB_SQUAD=2,
			BKB_SQUAD_FILTER=1,	BKB_SQUAD_FILTER_SET=1,BKB_SQUAD_QUERY=1,	B_OBJECT=8,				B_OBJECT_ANIMATION_STATE=1,
			B_OBJECTIVE_MANAGER=1,B_PATH=1,			B_PATHING_LIMITER=2,	B_PIECEWISE_DATA_POINT=1,	B_PIECEWISE_FUNC=1,
			B_PLATOON=3,			B_PLAYER=9,			B_POWER=0x16,		B_POWER_ENTRY=1,			B_POWER_ENTRY_ITEM=1,
				
			B_POWER_MANAGER=1,B_POWER_USER=9,	B_PROJECTILE=4,		B_PROTO_ACTION=2,			B_PROTO_OBJECT=3,
			B_PROTO_SQUAD=3,	B_PROTO_TECH=1,	B_SAVE_DB=9,			B_SAVE_PLAYER=1,			B_SAVE_TEAM=1,
			B_SAVE_USER=3,	B_SCORE_MANAGER=4,B_SELECTION_ABILITY=1,B_SIM_ORDER=2,			B_SIM_ORDER_ENTRY=2,
			B_SIM_TARGET=1,	B_SQUAD=6,		B_SQUAD_AI=4,			B_SQUAD_PLOTTER_RESULT=1,	B_STATS_MANAGER=1,
			B_TACTIC=2,		B_TEAM=2,		B_TRIGGER=1,			B_TRIGGER_CONDITION=2,	B_TRIGGER_EFFECT=2,
				
			B_TRIGGER_GROUP=1,	B_TRIGGER_MANAGER=1,	B_TRIGGER_SCRIPT=2,			B_TRIGGER_SCRIPT_EXTERNALS=1,	B_TRIGGER_VAR=3,
			BUI_CALLOUTS=1,		BUI_MANAGER=4,		B_UNIT=7,					B_USER=5,					B_VISIBLE_MAP=1,
			B_WEAPON=3,			B_WORLD=0xE,			B_STORED_ANIM_EVENT_MANAGER=2,	BAI_GROUP=3,					BAI_GROUP_TASK=2,
			BAI_MISSION_CACHE=4,	BAI_TELEPORTER_ZONE=1,BAI_PLAYER_MODIFIER=1,		B_ENTITY_SCHEDULER=2,			B_COLLECTIBLES_MANAGER=1,
			B_VISUAL=1,			B_VISUAL_ITEM=3,		B_VISUAL_ANIMATION_DATA=2,		B_GRANNY_INSTANCE=3,			B_TIMER_MANAGER=1,

			BUI_WIDGETS=2,	BUI_OBJECTIVE_PROGRESS_CONTROL=1,	BUI_TALKING_HEAD_CONTROL=1,	B_SQUAD_ACTION_ENTRY=2
			;

		#region IEndianStreamSerializable Members
		public static void Serialize(IO.EndianStream s)
		{
			s.StreamVersion(B_ACTION);			s.StreamVersion(B_ACTION_MANAGER);	s.StreamVersion(BAI);				s.StreamVersion(BAI_GLOBALS);		s.StreamVersion(BAI_DIFFICULTY_SETTING);
			s.StreamVersion(BAI_MANAGER);		s.StreamVersion(BAI_MISSION);		s.StreamVersion(BAI_MISSION_SCORE);	s.StreamVersion(BAI_MISSION_TARGET);	s.StreamVersion(BAI_MISSION_TARGET_WRAPPER);
			s.StreamVersion(BAI_POWER_MISSION);	s.StreamVersion(BAI_SCORE_MODIFIER);	s.StreamVersion(BAI_SQUAD_ANALYSIS);	s.StreamVersion(BAI_TOPIC);			s.StreamVersion(B_ARMY);
			s.StreamVersion(B_BID);				s.StreamVersion(B_BID_MANAGER);		s.StreamVersion(B_CHAT_MANAGER);		s.StreamVersion(B_CONVEX_HULL);		s.StreamVersion(B_COST);
			s.StreamVersion(B_CUSTOM_COMMAND);	s.StreamVersion(B_DAMAGE_TRACKER);	s.StreamVersion(B_DOPPLE);			s.StreamVersion(B_ENTITY);			s.StreamVersion(B_ENTITY_FILTER);
				
			s.StreamVersion(B_ENTITY_FILTER_SET);	s.StreamVersion(B_FATALITY_MANAGER);	s.StreamVersion(B_FORMATION2);		s.StreamVersion(B_GENERAL_EVENT_MANAGER);	s.StreamVersion(B_HINT_ENGINE);
			s.StreamVersion(B_HINT_MANAGER);		s.StreamVersion(BKB);				s.StreamVersion(BKB_BASE);			s.StreamVersion(BKB_BASE_QUERY);			s.StreamVersion(BKB_SQUAD);
			s.StreamVersion(BKB_SQUAD_FILTER);	s.StreamVersion(BKB_SQUAD_FILTER_SET);	s.StreamVersion(BKB_SQUAD_QUERY);		s.StreamVersion(B_OBJECT);				s.StreamVersion(B_OBJECT_ANIMATION_STATE);
			s.StreamVersion(B_OBJECTIVE_MANAGER);	s.StreamVersion(B_PATH);				s.StreamVersion(B_PATHING_LIMITER);	s.StreamVersion(B_PIECEWISE_DATA_POINT);	s.StreamVersion(B_PIECEWISE_FUNC);
			s.StreamVersion(B_PLATOON);			s.StreamVersion(B_PLAYER);			s.StreamVersion(B_POWER);			s.StreamVersion(B_POWER_ENTRY);			s.StreamVersion(B_POWER_ENTRY_ITEM);
				
			s.StreamVersion(B_POWER_MANAGER);	s.StreamVersion(B_POWER_USER);	s.StreamVersion(B_PROJECTILE);		s.StreamVersion(B_PROTO_ACTION);			s.StreamVersion(B_PROTO_OBJECT);
			s.StreamVersion(B_PROTO_SQUAD);	s.StreamVersion(B_PROTO_TECH);	s.StreamVersion(B_SAVE_DB);			s.StreamVersion(B_SAVE_PLAYER);			s.StreamVersion(B_SAVE_TEAM);
			s.StreamVersion(B_SAVE_USER);		s.StreamVersion(B_SCORE_MANAGER);	s.StreamVersion(B_SELECTION_ABILITY);	s.StreamVersion(B_SIM_ORDER);				s.StreamVersion(B_SIM_ORDER_ENTRY);
			s.StreamVersion(B_SIM_TARGET);	s.StreamVersion(B_SQUAD);		s.StreamVersion(B_SQUAD_AI);			s.StreamVersion(B_SQUAD_PLOTTER_RESULT);	s.StreamVersion(B_STATS_MANAGER);
			s.StreamVersion(B_TACTIC);		s.StreamVersion(B_TEAM);			s.StreamVersion(B_TRIGGER);			s.StreamVersion(B_TRIGGER_CONDITION);		s.StreamVersion(B_TRIGGER_EFFECT);
				
			s.StreamVersion(B_TRIGGER_GROUP);		s.StreamVersion(B_TRIGGER_MANAGER);	s.StreamVersion(B_TRIGGER_SCRIPT);			s.StreamVersion(B_TRIGGER_SCRIPT_EXTERNALS);	s.StreamVersion(B_TRIGGER_VAR);
			s.StreamVersion(BUI_CALLOUTS);		s.StreamVersion(BUI_MANAGER);		s.StreamVersion(B_UNIT);						s.StreamVersion(B_USER);						s.StreamVersion(B_VISIBLE_MAP);
			s.StreamVersion(B_WEAPON);			s.StreamVersion(B_WORLD);			s.StreamVersion(B_STORED_ANIM_EVENT_MANAGER);	s.StreamVersion(BAI_GROUP);					s.StreamVersion(BAI_GROUP_TASK);
			s.StreamVersion(BAI_MISSION_CACHE);	s.StreamVersion(BAI_TELEPORTER_ZONE);	s.StreamVersion(BAI_PLAYER_MODIFIER);			s.StreamVersion(B_ENTITY_SCHEDULER);			s.StreamVersion(B_COLLECTIBLES_MANAGER);
			s.StreamVersion(B_VISUAL);			s.StreamVersion(B_VISUAL_ITEM);		s.StreamVersion(B_VISUAL_ANIMATION_DATA);		s.StreamVersion(B_GRANNY_INSTANCE);			s.StreamVersion(B_TIMER_MANAGER);

			s.StreamVersion(BUI_WIDGETS);	s.StreamVersion(BUI_OBJECTIVE_PROGRESS_CONTROL);	s.StreamVersion(BUI_TALKING_HEAD_CONTROL);	s.StreamVersion(B_SQUAD_ACTION_ENTRY);

			s.StreamSignature(CSaveMarker.VERSIONS);
		}
		#endregion
	};
}