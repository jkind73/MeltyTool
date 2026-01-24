using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	partial class CSaveMarker
	{
		public const ushort
			HINT_ENGINE = 0x2710,
			CONCEPT = 0x2711,
			PARAMETER_PAGE = 0x2712
			;
	};

	sealed class BParameterPage
		: IO.IEndianStreamSerializable
	{
		public const ushort K_DONE_INDEX = 0x3E9;
		const int K_MAX_ENTITIES_PER_LIST_ = 0x3E8;

		public BVector vector;
		public BEntityID[] squadList, unitList;
		public BEntityFilterSet entityFilterSet = new BEntityFilterSet();
		public float @float;
		public int objectType;
		public uint locStringId;
		public bool hasVector, hasSquadList, hasUnitList,
			hasEntityFilterSet, hasFloat, hasObjectType,
			hasLocStringId;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamV(ref this.vector);
			BSaveGame.StreamArray16(s, ref this.squadList);
			Contract.Assert(this.squadList.Length <= K_MAX_ENTITIES_PER_LIST_);
			BSaveGame.StreamArray16(s, ref this.unitList);
			Contract.Assert(this.squadList.Length <= K_MAX_ENTITIES_PER_LIST_);
			s.Stream(this.entityFilterSet);
			s.Stream(ref this.@float);
			s.Stream(ref this.objectType);
			s.Stream(ref this.locStringId);
			s.Stream(ref this.hasVector); s.Stream(ref this.hasSquadList); s.Stream(ref this.hasUnitList);
			s.Stream(ref this.hasEntityFilterSet); s.Stream(ref this.hasFloat); s.Stream(ref this.hasObjectType);
			s.Stream(ref this.hasLocStringId);
		}
		#endregion
	};

	sealed class BConcept
		: IO.IEndianStreamSerializable
	{
		public const int K_MAX_COUNT = BParameterPage.K_DONE_INDEX-1;

		static readonly CondensedListInfo KPagesListInfo = new CondensedListInfo()
		{
			SerializeCapacity=true,
			IndexSize=sizeof(short),
			MaxCount=K_MAX_COUNT,
			DoneIndex=BParameterPage.K_DONE_INDEX,
		};

		public bool hasCommand;
		public byte newCommand;
		public bool stateChanged;
		public byte newState;
		public bool hasPreconditionResult;
		public byte preconditionResult;
		public uint preconditionTime;
		public byte state;
		public int gamesReinforced, timesReinforced, hintDisplayedCount;
		public List<CondensedListItem16<BParameterPage>> pages = [];
		public int timesReinforcedThisGame;
		public bool eventReady, active, permission;
		public float initialWaitTimeRemaining, terminalWaitTimeRemaining;
		public uint coolDownTimer, lastCoolDownAmount;
		public float coolDownTimerAccumulator;
		public int[] subHints; // ids
		public int parentHint;
		public bool prereqsMet, dirtyProfile;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.hasCommand); s.Stream(ref this.newCommand);
			s.Stream(ref this.stateChanged); s.Stream(ref this.newState);
			s.Stream(ref this.hasPreconditionResult); s.Stream(ref this.preconditionResult);
			s.Stream(ref this.preconditionTime);
			s.Stream(ref this.state);
			s.Stream(ref this.gamesReinforced); s.Stream(ref this.timesReinforced); s.Stream(ref this.hintDisplayedCount);
			BSaveGame.StreamList(s, this.pages, KPagesListInfo);
			s.Stream(ref this.timesReinforcedThisGame);
			s.Stream(ref this.eventReady); s.Stream(ref this.active); s.Stream(ref this.permission);
			s.Stream(ref this.initialWaitTimeRemaining); s.Stream(ref this.terminalWaitTimeRemaining);
			s.Stream(ref this.coolDownTimer); s.Stream(ref this.lastCoolDownAmount);
			s.Stream(ref this.coolDownTimerAccumulator);
			BSaveGame.StreamArray(s, ref this.subHints);
			s.Stream(ref this.parentHint); Contract.Assert(this.parentHint <= K_MAX_COUNT);
			s.Stream(ref this.prereqsMet); s.Stream(ref this.dirtyProfile);
			s.StreamSignature(CSaveMarker.CONCEPT);
		}
		#endregion
	};

	sealed class BHintEngine
		: IO.IEndianStreamSerializable
	{
		static readonly CondensedListInfo KConceptsListInfo = new CondensedListInfo()
		{
			SerializeCapacity=true,
			IndexSize=sizeof(uint),
			MaxCount=BConcept.K_MAX_COUNT,
			DoneIndex=int.MaxValue,
		};

		public List<CondensedListItem32<BConcept>> Concepts { get; private set; } =
			[];
		public float timeSinceLastHint;
		public bool hintMessageOn;
		public int[] allowedConcepts;
		public float waitForNextRescore;
		public uint lastGameTime;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamList(s, this.Concepts, KConceptsListInfo);
			s.Stream(ref this.timeSinceLastHint);
			s.Stream(ref this.hintMessageOn);
			BSaveGame.StreamArray16(s, ref this.allowedConcepts);
			Contract.Assert(this.allowedConcepts.Length <= BConcept.K_MAX_COUNT);
			s.Stream(ref this.waitForNextRescore);
			s.Stream(ref this.lastGameTime);
			s.StreamSignature(CSaveMarker.HINT_ENGINE);
		}
		#endregion
	};
}
