
using BVector = System.Numerics.Vector4;
using BWaveGravityBall = System.UInt64; // idk, 8 bytes

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerCovDebrisUser
		: BPowerUser
	{
		public BWaveGravityBall fakeGravityBall;
		public BVector horizontalMoveInputDir, verticalMoveInputDir, 
			lastUpdatePos, cameraFocusPoint;
		public uint timestampNextCommand;
		public float timeUntilHint;
		public uint commandInterval;
		public float minBallDistance, maxBallDistance, minBallHeight,
			maxBallHeight, maxBallSpeedStagnant, maxBallSpeedPulling,
			cameraDistance, cameraHeight, cameraHoverPointDistance,
			cameraMaxBallAngle, pullingRange, pickupShakeDuration,
			pickupRumbleShakeStrength, pickupCameraShakeStrength, 
			explodeTime, delayShutdownTimeLeft;
		public bool hintShown, hintCompleted, shuttingDown;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref this.fakeGravityBall);
			s.StreamV(ref this.horizontalMoveInputDir); s.StreamV(ref this.verticalMoveInputDir);
			s.StreamV(ref this.lastUpdatePos); s.StreamV(ref this.cameraFocusPoint);
			s.Stream(ref this.timestampNextCommand);
			s.Stream(ref this.timeUntilHint);
			s.Stream(ref this.commandInterval);
			s.Stream(ref this.minBallDistance); s.Stream(ref this.maxBallDistance); s.Stream(ref this.minBallHeight);
			s.Stream(ref this.maxBallHeight); s.Stream(ref this.maxBallSpeedStagnant); s.Stream(ref this.maxBallSpeedPulling);
			s.Stream(ref this.cameraDistance); s.Stream(ref this.cameraHeight); s.Stream(ref this.cameraHoverPointDistance);
			s.Stream(ref this.cameraMaxBallAngle); s.Stream(ref this.pullingRange); s.Stream(ref this.pickupShakeDuration);
			s.Stream(ref this.pickupRumbleShakeStrength); s.Stream(ref this.pickupCameraShakeStrength);
			s.Stream(ref this.explodeTime); s.Stream(ref this.delayShutdownTimeLeft);
			s.Stream(ref this.hintShown); s.Stream(ref this.hintCompleted); s.Stream(ref this.shuttingDown);
		}
		#endregion
	};
}