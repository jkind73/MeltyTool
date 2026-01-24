#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using BProtoObjectID = System.Int32;
using BProtoObjectTrainLimit = System.Int32; // idk, 4 bytes

namespace KSoft.Phoenix.Runtime
{
	partial class CSaveMarker
	{
		public const ushort
			PROTO_OBJECT = 0x2710
			;
	};

	sealed class BProtoObject
		: BProtoObjectBase
	{
		const int K_TRAIN_LIMITS_MAX_COUNT_ = 0x64;

		public BProtoObjectID protoId;
		public BProtoObjectTrainLimit[] trainLimits;
		public BHardpoint[] hardpoints;

		public int protoVisualIndex;
		public float desiredVelocity, maxVelocity;
		public float hitpoints, shieldpoints;
		public float los;
		public int simLos;

		public float bounty;
		public BTactic tactic;
		public float ammoMax, ammoRegenRate, rateAmount;
		public int maxContained, displayNameIndex, circleMenuIconId;
		public int deathSpawnSquad;
		public byte[] commandDisabled; // utbitvector; count=4
		public byte[] commandSelectable; // utbitvector; count=4
		public bool abilityDisabled,
			autoCloak, cloakMove, cloakAttack,
			uniqueInstance;

		public void CommandDisabledNone()
		{
			for (int x = 0; x < this.commandDisabled.Length; x++)
				this.commandDisabled[x] = 0;
		}

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);
			var sg = s.Owner as BSaveGame;

			s.Stream(ref this.protoId);
			BSaveGame.StreamArray(s, ref this.trainLimits);
			Contract.Assert(this.trainLimits.Length <= K_TRAIN_LIMITS_MAX_COUNT_);
			BSaveGame.StreamArray(s, ref this.hardpoints);
			Contract.Assert(this.hardpoints.Length <= BHardpoint.K_MAX_COUNT);
			sg.StreamBCost(s, ref this.cost);
			s.Stream(ref this.protoVisualIndex);
			s.Stream(ref this.desiredVelocity); s.Stream(ref this.maxVelocity);
			s.Stream(ref this.hitpoints); s.Stream(ref this.shieldpoints);
			s.Stream(ref this.los);
			s.Stream(ref this.simLos);
			s.Stream(ref this.buildPoints);
			s.Stream(ref this.bounty);
			s.StreamNotNull(ref this.tactic);
			s.Stream(ref this.ammoMax); s.Stream(ref this.ammoRegenRate); s.Stream(ref this.rateAmount);
			s.Stream(ref this.maxContained); s.Stream(ref this.displayNameIndex); s.Stream(ref this.circleMenuIconId);
			s.Stream(ref this.deathSpawnSquad);
			BSaveGame.StreamArray(s, ref this.commandDisabled);
			BSaveGame.StreamArray(s, ref this.commandSelectable);
			s.Stream(ref this.available); s.Stream(ref this.forbid); s.Stream(ref this.abilityDisabled);
			s.Stream(ref this.autoCloak); s.Stream(ref this.cloakMove); s.Stream(ref this.cloakAttack);
			s.Stream(ref this.uniqueInstance);
			s.StreamSignature(CSaveMarker.PROTO_OBJECT);
		}
		#endregion

		#region IIndentedStreamWritable Members
#if false
		public void ToStream(IO.IndentedTextWriter s)
		{
			s.WriteLine("{0} {1}{2}{3}{4}", "CommandDisabled",
				CommandDisabled[0].ToString("X2"), CommandDisabled[1].ToString("X2"),
				CommandDisabled[2].ToString("X2"), CommandDisabled[3].ToString("X2"));
			s.WriteLine("{0} {1}{2}{3}{4}", "CommandSelectable",
				CommandSelectable[0].ToString("X2"), CommandSelectable[1].ToString("X2"),
				CommandSelectable[2].ToString("X2"), CommandSelectable[3].ToString("X2"));
		}
#endif
		#endregion
	};
}