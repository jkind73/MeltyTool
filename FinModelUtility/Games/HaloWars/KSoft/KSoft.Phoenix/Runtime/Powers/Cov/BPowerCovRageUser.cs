
using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerCovRageUser
		: BPowerUser
	{
		public float cameraZoom, cameraHeight;
		public uint timestampNextCommand;
		public BVector moveInputDir, attackInputDir;
		public float timeUntilHint;
		public BVector lastMovePos, lastMoveDir, lastAttackDir;
		public uint commandInterval;
		public float scanRadius, movementProjectionMultiplier;
		public bool hasMoved, hasAttacked, hintShown, 
			hintCompleted, forceCommandNextUpdate;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref this.cameraZoom); s.Stream(ref this.cameraHeight);
			s.Stream(ref this.timestampNextCommand);
			s.StreamV(ref this.moveInputDir); s.StreamV(ref this.attackInputDir);
			s.Stream(ref this.timeUntilHint);
			s.StreamV(ref this.lastMovePos); s.StreamV(ref this.lastMoveDir); s.StreamV(ref this.lastAttackDir);
			s.Stream(ref this.commandInterval);
			s.Stream(ref this.scanRadius); s.Stream(ref this.movementProjectionMultiplier);
			s.Stream(ref this.hasMoved); s.Stream(ref this.hasAttacked); s.Stream(ref this.hintShown);
			s.Stream(ref this.hintCompleted); s.Stream(ref this.forceCommandNextUpdate);
		}
		#endregion
	};
}