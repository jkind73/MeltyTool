
using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;
using BEntityTimePair = System.UInt64;
using BProtoObjectID = System.Int32;
using BObjectTypeID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscCryo
		: BPower
	{
		public BEntityTimePair[] ignoreList;
		public uint nextTickTime;
		public BEntityID cryoObjectId;
		public BVector direction, right;
		public BProtoObjectID cryoObjectProtoId, bomberProtoId;
		public BObjectTypeID filterTypeId;
		public float cryoRadius, minCryoFalloff;
		public uint tickDuration, ticksRemaining;
		public float cryoAmountPerTick, killableHpLeft, freezingThawTime, frozenThawTime;
		public BPowerHelperBomber bomberData = new BPowerHelperBomber();
		public bool reactionPlayed;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			BSaveGame.StreamArray(s, ref this.ignoreList);
			s.Stream(ref this.nextTickTime);
			s.Stream(ref this.cryoObjectId);
			s.StreamV(ref this.direction); s.StreamV(ref this.right);
			s.Stream(ref this.cryoObjectProtoId); s.Stream(ref this.bomberProtoId);
			s.Stream(ref this.filterTypeId);
			s.Stream(ref this.cryoRadius); s.Stream(ref this.minCryoFalloff);
			s.Stream(ref this.tickDuration); s.Stream(ref this.ticksRemaining);
			s.Stream(ref this.cryoAmountPerTick); s.Stream(ref this.killableHpLeft); s.Stream(ref this.freezingThawTime); s.Stream(ref this.frozenThawTime);
			s.Stream(this.bomberData);
			s.Stream(ref this.reactionPlayed);
		}
		#endregion
	};
}