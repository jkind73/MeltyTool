
using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscCarpetBombUser
		: BPowerUser
	{
		public sbyte inputState;
		public BVector position, desiredForward;
		public float desiredScale, currentScale;
		public double shutdownTime;
		public BEntityID arrowId;
		public float maxBombOffset, lengthMultiplier;
		public sbyte losMode;
		public BProtoObjectID arrowProtoId;
		public BPowerHelperHudSounds hudSounds = new BPowerHelperHudSounds();

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref this.inputState);
			s.StreamV(ref this.position); s.StreamV(ref this.desiredForward);
			s.Stream(ref this.desiredScale); s.Stream(ref this.currentScale);
			s.Stream(ref this.shutdownTime);
			s.Stream(ref this.arrowId);
			s.Stream(ref this.maxBombOffset); s.Stream(ref this.lengthMultiplier);
			s.Stream(ref this.losMode);
			s.Stream(ref this.arrowProtoId);
			s.Stream(this.hudSounds);
		}
		#endregion
	};
}