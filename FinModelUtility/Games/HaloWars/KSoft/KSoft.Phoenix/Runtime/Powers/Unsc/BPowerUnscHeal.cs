
using BEntityID = System.Int32;
using BEntityTimePair = System.UInt64;
using BProtoObjectID = System.Int32;
using BObjectTypeID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscHeal
		: BPower
	{
		public BEntityID[] squadsRepairing;
		public BEntityTimePair[] ignoreList;
		public uint nextTickTime;
		public BEntityID repairObjectId;
		public BProtoObjectID repairAttachmentProtoId;
		public BObjectTypeID filterTypeId;
		public float repairRadius;
		public uint tickDuration;
		public float repairCombatValuePerTick;
		public uint cooldownTimeIfDamaged, ticksRemaining;
		public bool spreadAmongSquads, allowReinforce, ignorePlacement,
			healAny, neverStops;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			BSaveGame.StreamArray(s, ref this.squadsRepairing);
			BSaveGame.StreamArray(s, ref this.ignoreList);
			s.Stream(ref this.nextTickTime);
			s.Stream(ref this.repairObjectId);
			s.Stream(ref this.repairAttachmentProtoId);
			s.Stream(ref this.filterTypeId);
			s.Stream(ref this.repairRadius);
			s.Stream(ref this.tickDuration);
			s.Stream(ref this.repairCombatValuePerTick);
			s.Stream(ref this.cooldownTimeIfDamaged); s.Stream(ref this.ticksRemaining);
			s.Stream(ref this.spreadAmongSquads); s.Stream(ref this.allowReinforce); s.Stream(ref this.ignorePlacement);
			s.Stream(ref this.healAny); s.Stream(ref this.neverStops);
		}
		#endregion
	};
}