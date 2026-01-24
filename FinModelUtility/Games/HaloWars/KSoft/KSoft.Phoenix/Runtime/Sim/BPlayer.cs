using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;
using BPlayerID = System.Int32;
using BTeamID = System.Int32;
using BPlayerState = System.UInt32; // states are defined in GameData.xml
using BCost = System.Single;
using BPowerLevel = System.UInt32; // idk, 4 bytes

namespace KSoft.Phoenix.Runtime
{
	using BRallyPoint = System.Numerics.Vector4; // this is only a guess

	partial class CSaveMarker
	{
		public const ushort
			PLAYER1 = 0x2710, PLAYER2 = 0x2711, PLAYER3 = 0x2712, PLAYER4 = 0x2713
			;
	};

	sealed class BPlayer
		: IO.IEndianStreamSerializable
	{
		public struct PowerInfo : IO.IEndianStreamSerializable
		{
			public uint rechargeTime;
			public int useLimit;
			public BPowerLevel level;
			public uint availableTime;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.rechargeTime);
				s.Stream(ref this.useLimit);
				s.Stream(ref this.level);
				s.Stream(ref this.availableTime);
			}
			#endregion
		};
		public struct RateInfo
			: IO.IEndianStreamSerializable
		{
			public float amount, multiplier;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.amount);
				s.Stream(ref this.multiplier);
			}
			#endregion
		};
		#region UnitCountInfo
		public struct UnitCountInfo
			: IO.IEndianStreamSerializable
		{
			public uint futureUnitCount, deadUnitCount;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.futureUnitCount);
				s.Stream(ref this.deadUnitCount);
			}
			#endregion
		};
		static readonly CondensedListInfo KUnitCountsListInfo = new CondensedListInfo()
		{
			IndexSize=sizeof(short),
		};
		#endregion
		#region WeaponType
		public struct WeaponType
			: IO.IEndianStreamSerializable
		{
			public sbyte damageType;
			public float modifier;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.damageType);
				s.Stream(ref this.modifier);
			}
			#endregion
		};
		// Index = actual WeaponType
		static readonly CondensedListInfo KWeaponTypesListInfo = new CondensedListInfo()
		{
			IndexSize=sizeof(sbyte),
			MaxCount=0x4E20,
		};
		#endregion

		#region Player1
		static readonly CondensedListInfo KProtoUnitsListInfo = new CondensedListInfo()
		{
			SerializeCapacity=true,
			IndexSize=sizeof(short),
			MaxCount=0x4E20,
		};
		static readonly CondensedListInfo KProtoTechsListInfo = new CondensedListInfo()
		{
			SerializeCapacity=true,
			IndexSize=sizeof(short),
			MaxCount=0x1388,
		};
		static readonly CondensedListInfo KProtoUniqueUnitsListInfo = new CondensedListInfo()
		{
			SerializeCapacity=true,
			IndexSize=sizeof(short),
			MaxCount=0x3E8,
		};

		public BVector lookAtPos; // Gaia's values are ints, not floats...
		public string name;
		public BRallyPoint rallyPoint; // Gaia's values are ints, not floats...
		public BStatsManager statsManager = new BStatsManager();
		public List<CondensedListItem16<BProtoObject>> protoObjects;
		public List<CondensedListItem16<BProtoSquad>> protoSquads;
		public List<CondensedListItem16<BProtoTech>> protoTechs;
		public BProtoObject[] uniqueProtoObjects;
		public BProtoSquad[] uniqueProtoSquad;
		public BPowerEntry[] powerEntries;
		public int[] abilities;
		public PowerInfo[] powers;
		public BCost[] resources;
		public RateInfo[] rates;
		public BCost[] totalResources;
		public BCost[] resourceTrickleRate;
		public BPlayerPop[] populations;
		#endregion
		#region Player2
		public BHintEngine hintEngine;
		public List<CondensedListItemValue16<UnitCountInfo>> genericObjectCounts, squadCounts;
		#endregion
		#region Player3
		public uint totalFutureUnitCounts, totalDeadUnitCounts;
		public uint totalFutureSquadCounts, totalDeadSquadCounts;
		public BEntityID[] gotoBases;
		public List<CondensedListItemValue8<WeaponType>> weaponTypes;
		public float[] abilityRecoverTimes;
		public BTechTree techTree = new BTechTree();
		#endregion
		#region Player4
		public int mpid, colorIndex;
		public BPlayerID id, coopId, scenarioId;
		public int civId;
		public BTeamID teamId;
		public BPlayerState playerState;
		public int leaderId, bountyResource;
		public BEntityID rallyObject;
		public int strength;
		public float tributeCost;
		public BCost[] repairCost;
		public float repairTime, handicapMultiplier, shieldRegenRate;
		public uint shieldRegenDelay;
		public float totalCombatValue;
		public float difficulty;
		public uint gamePlayedTime;
		public BPlayerID floodPoofPlayer;
		public sbyte playerType, squadSearchAttempts;
		public float weaponPhysicsMultiplier, aiDamageMultiplier, aiDamageTakenMultiplier,
			aiBuildSpeedMultiplier;
		public bool flagRallyPoint, flagBountyResource, flagMinimapBlocked,
			flagLeaderPowersBlocked, flagDefeatedDestroy;
		public int squadAiSearchIndex, squadAiWorkIndex, squadAiSecondaryTurretScanIndex;
		#endregion

		#region IEndianStreamSerializable Members
		long mPositionMarker_;
		public void Serialize(IO.EndianStream s)
		{
			var sg = s.Owner as BSaveGame;

			#region Init
			if (s.IsReading)
			{
				this.protoObjects = new List<CondensedListItem16<BProtoObject>>(sg.Database.GenericProtoObjects.Count);
				this.protoSquads = new List<CondensedListItem16<BProtoSquad>>(sg.Database.ProtoSquads.Count);
				this.protoTechs = new List<CondensedListItem16<BProtoTech>>(sg.Database.ProtoTechs.Count);

				this.powers = new PowerInfo[sg.Database.ProtoPowers.Count];
				this.rates = new RateInfo[sg.Database.Rates.Count];
				this.populations = new BPlayerPop[sg.Database.Populations.Count];

				this.genericObjectCounts = new List<CondensedListItemValue16<UnitCountInfo>>(sg.Database.GenericProtoObjects.Count);
				this.squadCounts = new List<CondensedListItemValue16<UnitCountInfo>>(sg.Database.ProtoSquads.Count);
				this.weaponTypes = new List<CondensedListItemValue8<WeaponType>>(sg.Database.WeaponTypes.Count);
			}
			#endregion

			#region Player1
			s.StreamV(ref this.lookAtPos);
			s.StreamPascalString32(ref this.name);
			s.StreamV(ref this.rallyPoint);
			s.Stream(this.statsManager);
			BSaveGame.StreamList(s, this.protoObjects, KProtoUnitsListInfo);
			BSaveGame.StreamList(s, this.protoSquads, KProtoUnitsListInfo);
			BSaveGame.StreamList(s, this.protoTechs, KProtoTechsListInfo);
			BSaveGame.StreamArray16(s, ref this.uniqueProtoObjects);
			Contract.Assert(this.uniqueProtoObjects.Length <= KProtoUniqueUnitsListInfo.MaxCount);
			BSaveGame.StreamArray16(s, ref this.uniqueProtoSquad);
			Contract.Assert(this.uniqueProtoSquad.Length <= KProtoUniqueUnitsListInfo.MaxCount);
			BSaveGame.StreamArray(s, ref this.powerEntries);
			BSaveGame.StreamArray(s, ref this.abilities);
			for (int x = 0; x < this.powers.Length; x++)
				s.Stream(ref this.powers[x]);
			sg.StreamBCost(s, ref this.resources);
			for (int x = 0; x < this.rates.Length; x++)
				s.Stream(ref this.rates[x]);
			sg.StreamBCost(s, ref this.totalResources);
			sg.StreamBCost(s, ref this.resourceTrickleRate);
			for (int x = 0; x < this.populations.Length; x++)
				s.Stream(ref this.populations[x]);
			s.StreamSignature(CSaveMarker.PLAYER1);
			#endregion
			#region Player2
			s.StreamNotNull(ref this.hintEngine);
			BSaveGame.StreamList(s, this.genericObjectCounts, KUnitCountsListInfo);
			BSaveGame.StreamList(s, this.squadCounts, KUnitCountsListInfo);
			s.StreamSignature(CSaveMarker.PLAYER2);
			#endregion
			#region Player3
			s.Stream(ref this.totalFutureUnitCounts); s.Stream(ref this.totalDeadUnitCounts);
			s.Stream(ref this.totalFutureSquadCounts); s.Stream(ref this.totalDeadSquadCounts);
			BSaveGame.StreamArray(s, ref this.gotoBases);
			BSaveGame.StreamList(s, this.weaponTypes, KWeaponTypesListInfo);
			BSaveGame.StreamArray16(s, ref this.abilityRecoverTimes);
			s.Stream(this.techTree);
			s.StreamSignature(CSaveMarker.PLAYER3);
			#endregion
			#region Player4
			s.Stream(ref this.mpid); s.Stream(ref this.colorIndex);
			s.Stream(ref this.id); s.Stream(ref this.coopId); s.Stream(ref this.scenarioId);
			s.Stream(ref this.civId);
			s.Stream(ref this.teamId);
			s.Stream(ref this.playerState);
			s.Stream(ref this.leaderId); s.Stream(ref this.bountyResource);
			s.Stream(ref this.rallyObject);
			s.Stream(ref this.strength);
			s.Stream(ref this.tributeCost);
			sg.StreamBCost(s, ref this.repairCost);
			s.Stream(ref this.repairTime); s.Stream(ref this.handicapMultiplier); s.Stream(ref this.shieldRegenRate);
			s.Stream(ref this.shieldRegenDelay);
			s.Stream(ref this.totalCombatValue);
			s.Stream(ref this.difficulty);
			s.Stream(ref this.gamePlayedTime);
			s.Stream(ref this.floodPoofPlayer);
			s.Stream(ref this.playerType); s.Stream(ref this.squadSearchAttempts);
			s.Stream(ref this.weaponPhysicsMultiplier); s.Stream(ref this.aiDamageMultiplier); s.Stream(ref this.aiDamageTakenMultiplier);
			s.Stream(ref this.aiBuildSpeedMultiplier);
			s.Stream(ref this.flagRallyPoint); s.Stream(ref this.flagBountyResource); s.Stream(ref this.flagMinimapBlocked);
			s.Stream(ref this.flagLeaderPowersBlocked); s.Stream(ref this.flagDefeatedDestroy);
			s.Stream(ref this.squadAiSearchIndex); s.Stream(ref this.squadAiWorkIndex); s.Stream(ref this.squadAiSecondaryTurretScanIndex);
			s.StreamSignature(CSaveMarker.PLAYER4);
			#endregion
			s.TraceAndDebugPosition(ref this.mPositionMarker_);
		}
		#endregion
	};
}