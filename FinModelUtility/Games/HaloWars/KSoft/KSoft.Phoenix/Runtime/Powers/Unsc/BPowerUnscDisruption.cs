
using BVector = System.Numerics.Vector4;
using BCueIndex = System.Int32;
using BEntityID = System.Int32;
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscDisruption
		: BPower
	{
		public BEntityID disruptionObjectId;
		public float disruptionRadius, disruptionRadiusSqr, timeRemainingSec,
			disruptionStartTime;
		public BVector direction, right;
		public BProtoObjectID bomberProtoId, disruptionObjectProtoId, 
			pulseObjectProtoId, strikeObjectProtoId;
		public float pulseSpacing, nextPulseTime;
		public int numPulses;
		public BPowerHelperBomber bomberData = new BPowerHelperBomber();
		public BCueIndex pulseSound;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref this.disruptionObjectId);
			s.Stream(ref this.disruptionRadius); s.Stream(ref this.disruptionRadiusSqr); s.Stream(ref this.timeRemainingSec);
			s.Stream(ref this.disruptionStartTime);
			s.StreamV(ref this.direction); s.StreamV(ref this.right);
			s.Stream(ref this.bomberProtoId); s.Stream(ref this.disruptionObjectProtoId);
			s.Stream(ref this.pulseObjectProtoId); s.Stream(ref this.strikeObjectProtoId);
			s.Stream(ref this.pulseSpacing); s.Stream(ref this.nextPulseTime);
			s.Stream(ref this.numPulses);
			s.Stream(this.bomberData);
			s.Stream(ref this.pulseSound);
		}
		#endregion
	};
}