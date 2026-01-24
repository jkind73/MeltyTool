using System.Collections.Generic;

using BProtoPowerID = System.Int32;
using BPlayerID = System.Int32;
using BTeamID = System.Int32;
using BPlayerState = System.UInt32; // states are defined in GameData.xml
using BCost = System.Single;

namespace KSoft.Phoenix.Runtime
{
	partial class CSaveMarker
	{
		public const ushort
			STATS_PLAYER = 0x2710,
			STATS_RECORDERS = 0x2711,
			STATS_POWERS = 0x2712,
			STATS_ABILITIES = 0x2713
			;
	};

	struct BStatLostDestroyed
		: IO.IEndianStreamSerializable
	{
		public uint lost, destroyed;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.lost);
			s.Stream(ref this.destroyed);
		}
		#endregion
	};
	struct BStatLostDestroyedKeyValuePair
		: IO.IEndianStreamSerializable
	{
		public int key;
		public BStatLostDestroyed value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.key);
			s.Stream(ref this.value);
		}
		#endregion
	};
	sealed class BStatLostDestroyedMap
		: IO.IEndianStreamSerializable
	{
		public int index;
		public BStatLostDestroyedKeyValuePair[] killers;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.index);
			BSaveGame.StreamArray16(s, ref this.killers);
		}
		#endregion
	};

	sealed class BStatCombat
		: IO.IEndianStreamSerializable
	{
		public const ushort DONE_INDEX = 0x2711;

		public int[] levels;
		public float xp;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray16(s, ref this.levels);
			s.Stream(ref this.xp);
		}
		#endregion
	};
	sealed class BStatCombatWithIndex
		: IO.IEndianStreamSerializable
	{
		public int index;
		public BStatCombat combat = new BStatCombat();

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.index);
			s.Stream(this.combat);
		}
		#endregion
	};

	class BStatTotal
		: IO.IEndianStreamSerializable
	{
		public byte[] killerIDs = new byte[0x20];
		public uint built, lost, destroyed, max;
		public int combatId;
		public uint firstTime, lastTime;

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			s.Stream(this.killerIDs);
			s.Stream(ref this.built); s.Stream(ref this.lost); s.Stream(ref this.destroyed); s.Stream(ref this.max);
			s.Stream(ref this.combatId);
			s.Stream(ref this.firstTime); s.Stream(ref this.lastTime);
		}
		#endregion
	};
	sealed class BStatEvent
		: BStatTotal
	{
		public uint timestamp;
		public int protoId;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref this.timestamp);
			s.Stream(ref this.protoId);
		}
		#endregion
	};

	sealed class BStatTotalKeyValuePair
		: IO.IEndianStreamSerializable
	{
		public int key;
		public BStatTotal value = new BStatTotal();

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.key);
			s.Stream(this.value);
		}
		#endregion
	};

	#region BStats Recorders
	abstract class BStatRecorderBase
		: IO.IEndianStreamSerializable
	{
		public BStatLostDestroyedMap[] killers;
		public BStatCombatWithIndex[] combat;

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray16(s, ref this.killers, isIterated:true);
			BSaveGame.StreamArray16(s, ref this.combat);
			s.StreamSignature(BStatCombat.DONE_INDEX);
		}
		#endregion
	};
	sealed class BStatTotalsRecorder
		: BStatRecorderBase
	{
		public const int K_STAT_TYPE = 1;

		public List<BStatTotalKeyValuePair> Totals { get; private set; } = [];
		public BStatTotal total = new BStatTotal();
		public BStatCombat combat;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			BSaveGame.StreamCollection(s, this.Totals);
			s.Stream(this.total);
			s.StreamNotNull(ref this.combat);
		}
		#endregion
	};
	sealed class BStatEventRecorder
		: BStatRecorderBase
	{
		public const int K_STAT_TYPE = 2;

		public BStatTotal total = new BStatTotal();
		public BStatEvent[] events;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(this.total);
			BSaveGame.StreamArray16(s, ref this.events);
		}
		#endregion
	};
	sealed class BStatGraphRecorder
		: BStatRecorderBase
	{
		public const int K_STAT_TYPE = 3;
	};
	sealed class BStatResourceGraphRecorder
		: BStatRecorderBase
	{
		public const int K_STAT_TYPE = 4;
	};
	sealed class BStatPopGraphRecorder
		: BStatRecorderBase
	{
		public const int K_STAT_TYPE = 5;
	};
	sealed class BStatBaseGraphRecorder
		: BStatRecorderBase
	{
		public const int K_STAT_TYPE = 6;
	};
	sealed class BStatScoreGraphRecorder
		: BStatRecorderBase
	{
		public const int K_STAT_TYPE = 7;
	};

	sealed class BStatsRecorder
		: IO.IEndianStreamSerializable
	{
		public short index;

		public byte statType;
		public BStatRecorderBase stat;

		#region IEndianStreamSerializable Members
		static BStatRecorderBase FromType(int statType)
		{
			switch (statType)
			{
			case BStatTotalsRecorder.K_STAT_TYPE: return new BStatTotalsRecorder();
			case BStatEventRecorder.K_STAT_TYPE: return new BStatEventRecorder();
#if false
			case BStatGraphRecorder.kStatType: return new BStatGraphRecorder();
			case BStatResourceGraphRecorder.kStatType: return new BStatResourceGraphRecorder();
			case BStatPopGraphRecorder.kStatType: return new BStatPopGraphRecorder();
			case BStatBaseGraphRecorder.kStatType: return new BStatBaseGraphRecorder();
			case BStatScoreGraphRecorder.kStatType: return new BStatScoreGraphRecorder();
#endif

			default: throw new KSoft.Debug.UnreachableException(statType.ToString());
			}
		}
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.index);
			s.Stream(ref this.statType);
			s.Stream(ref this.stat, 
				() => FromType(this.statType));
		}
		#endregion
	};
	#endregion

	struct BStatPowerKeyValuePair
		: IO.IEndianStreamSerializable
	{
		public BProtoPowerID key;
		public uint value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.key);
			s.Stream(ref this.value);
		}
		#endregion
	};
	struct BStatAbilityKeyValuePair
		: IO.IEndianStreamSerializable
	{
		public int key;
		public uint value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.key);
			s.Stream(ref this.value);
		}
		#endregion
	};
	sealed class BStatsManager
		: IO.IEndianStreamSerializable
	{
		public BStatsRecorder[] recorders;
		public List<BStatPowerKeyValuePair> Powers { get; private set; } = [];
		public List<BStatAbilityKeyValuePair> Abilities { get; private set; } =
			[];

		public BCost[] totalResources, maxResources, 
			gatheredResources, tributedResources;
		public BPlayerID playerId;
		public BTeamID teamId;
		public uint playerStateTime;
		public BPlayerState playerState;
		public uint strengthTime, strengthTimer;
		public int civId, leaderId;
		public byte resourcesUsed, playerType;
		public bool randomCiv, randomLeader, resigned, defeated, disconnected, won;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			var sg = s.Owner as BSaveGame;

			BSaveGame.StreamArray16(s, ref this.recorders, isIterated:true);
			s.StreamSignature(CSaveMarker.STATS_RECORDERS);
			BSaveGame.StreamCollection(s, this.Powers);
			s.StreamSignature(CSaveMarker.STATS_POWERS);
			BSaveGame.StreamCollection(s, this.Abilities);
			s.StreamSignature(CSaveMarker.STATS_ABILITIES);
			sg.StreamBCost(s, ref this.totalResources); sg.StreamBCost(s, ref this.maxResources);
			sg.StreamBCost(s, ref this.gatheredResources); sg.StreamBCost(s, ref this.tributedResources);
			s.Stream(ref this.playerId);
			s.Stream(ref this.teamId);
			s.Stream(ref this.playerStateTime);
			s.Stream(ref this.playerState);
			s.Stream(ref this.strengthTime); s.Stream(ref this.strengthTimer);
			s.Stream(ref this.civId); s.Stream(ref this.leaderId);
			s.Stream(ref this.resourcesUsed); s.Stream(ref this.playerType);
			s.Stream(ref this.randomCiv); s.Stream(ref this.randomLeader); s.Stream(ref this.resigned);
			s.Stream(ref this.defeated); s.Stream(ref this.disconnected); s.Stream(ref this.won);
			s.StreamSignature(CSaveMarker.STATS_PLAYER);
		}
		#endregion
	};
}
