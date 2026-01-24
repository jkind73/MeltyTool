using System.Collections.Generic;

namespace KSoft.Phoenix.Runtime
{
	partial class CSaveMarker
	{
		public const ushort
			WORLD1 = 0x2710, WORLD2 = 0x2711, WORLD3 = 0x2712, WORLD4 = 0x2713, WORLD5 = 0x2714,
			PLAYERS = 0x2715,
			TEAMS = 0x2716,
			SIM_ORDER = 0x2717,
			UNIT_OPP = 0x2718,
			PATH_MOVE_DATA = 0x2719,
			PLATOONS = 0x271A,
			DOPPLES = 0x271B,
			PROJECTILES = 0x271C,
			AIR_SPOTS = 0x271D,
			ARMIES = 0x271E,
			SQUADS = 0x271F,

			UNITS = 0x2720,
			OBJECTIVE_MANAGER = 0x2721,
			GENERAL_EVENTS = 0x2722,
			TRIGGERS = 0x2723,
			VISIBILTY = 0x2724,
			SCORE_MANAGER = 0x2725,
			STORED_ANIM_EVENT_MANAGER = 0x2726,
			ENTITY_SCHEDULER = 0x2727,
			COLLECTIBLES_MANAGER = 0x2728,
			OBJECTS = 0x2729
			;

		public const ushort OBJECT_ANIM_EVENT_TAG_QUEUE_DONE_INDEX = 0x9C5;

		public const byte B_ACTION_CONTROLLER_C_NUMBER_CONTROLLERS = 2;
	};

	sealed class BWorld
		: IO.IEndianStreamSerializable
	{
		public const byte C_MAXIMUM_SUPPORTED_PLAYERS = 9,
			C_MAX_PLAYER_COLOR_CATEGORIES = 2,
			C_MAXIMUM_SUPPORTED_TEAMS = 5;

		public sealed class ObjectGroup
			: IO.IEndianStreamSerializable
		{
			public short id;

			public int[] objects; // not sure if BProtoObjectID, etc
			public int[] triggeredTeams;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.id);
				BSaveGame.StreamArray(s, ref this.objects);
				BSaveGame.StreamArray(s, ref this.triggeredTeams);
			}
			#endregion
		};

		public struct BExplorationGroupTimerEntry
			: IO.IEndianStreamSerializable
		{
			public uint unknown0, unknown4, unknown8;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.unknown0); s.Stream(ref this.unknown4); s.Stream(ref this.unknown8);
			}
			#endregion
		};

		public struct PlayerColorCategory
			: IO.IEndianStreamSerializable
		{
			public uint objects, corpse, selection, 
				minimap, ui;
			public byte index;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.objects); s.Stream(ref this.corpse); s.Stream(ref this.selection);
				s.Stream(ref this.minimap); s.Stream(ref this.ui);
				s.Stream(ref this.index);
			}
			#endregion
		};

		ObjectGroup[] numExplorationGroups_;
		BExplorationGroupTimerEntry[] activeExplorationGroups_;
		public BPlayer[] players;
		PlayerColorCategory[,] playerColorCategories_ = new PlayerColorCategory[C_MAX_PLAYER_COLOR_CATEGORIES, C_MAXIMUM_SUPPORTED_PLAYERS];
		List<CondensedListItem16<BSimOrder>> simOrders_ = [];
		List<CondensedListItem16<BUnitOpp>> unitOpps_ = [];
		List<CondensedListItem16<BPathMoveData>> pathMoveData_ = [];

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			var sg = s.Owner as BSaveGame;

			if (s.IsReading)
			{
				this.players = new BPlayer[sg.Players.Count];
				for(int x = 0; x < this.players.Length; x++)
					this.players[x] = new BPlayer();
			}

			BSaveGame.StreamArray16(s, ref this.numExplorationGroups_, isIterated:true);
			BSaveGame.StreamArray(s, ref this.activeExplorationGroups_);
			s.StreamSignature(CSaveMarker.WORLD1);
			foreach (var player in this.players)
				s.Stream(player);
			s.StreamSignature(CSaveMarker.PLAYERS);
			s.StreamSignature(C_MAXIMUM_SUPPORTED_PLAYERS);
			s.StreamSignature(C_MAX_PLAYER_COLOR_CATEGORIES);
 			for (int x = 0; x < C_MAX_PLAYER_COLOR_CATEGORIES; x++)
 				for (int y = 0; y < C_MAXIMUM_SUPPORTED_PLAYERS; y++)
 					s.Stream(ref this.playerColorCategories_[x, y]);
			s.StreamSignature(CSaveMarker.WORLD2);
			BSaveGame.StreamFreeList(s, this.simOrders_, BSimOrder.KFreeListInfo);
			BSaveGame.StreamFreeList(s, this.unitOpps_, BUnitOpp.KFreeListInfo);
			BSaveGame.StreamFreeList(s, this.pathMoveData_, BPathMoveData.KFreeListInfo);

			//...
		}
		#endregion
	};
}