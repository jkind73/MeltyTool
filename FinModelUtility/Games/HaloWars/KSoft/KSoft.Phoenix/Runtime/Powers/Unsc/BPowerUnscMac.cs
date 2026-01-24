
using BVector = System.Numerics.Vector4;
using BCueIndex = System.Int32;
using BEntityID = System.Int32;
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscMac
		: BPower
	{
		public sealed class BShot
			: IO.IEndianStreamSerializable
		{
			public BVector launchPos, targetPos;
			public uint launchTime, createLaserTime;
			public BEntityID laserObj;
			public bool laserCreated;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.StreamV(ref this.launchPos); s.StreamV(ref this.targetPos);
				s.Stream(ref this.launchTime); s.Stream(ref this.createLaserTime);
				s.Stream(ref this.laserObj);
				s.Stream(ref this.laserCreated);
			}
			#endregion
		};

		public BEntityID realTargettingLaserId;
		public BVector desiredTargettingPosition;
		public BShot[] shots;
		public bool firedInitialShot;
		public uint shotsRemaining, impactsToProcess;
		public BProtoObjectID targetBeamId, projectileId, effectProtoId,
			rockSmallProtoId, rockMediumProtoId, rockLargeProtoId;
		public BCueIndex firedSound;
		public uint targetingDelay, autoShotDelay;
		public float autoShotInnerRadius, autoShotOuterRadius, 
			xOffset, yOffset, zOffset;
		public int losMode;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref this.realTargettingLaserId);
			s.StreamV(ref this.desiredTargettingPosition);
			BSaveGame.StreamArray(s, ref this.shots);
			s.Stream(ref this.firedInitialShot);
			s.Stream(ref this.shotsRemaining); s.Stream(ref this.impactsToProcess);
			s.Stream(ref this.targetBeamId); s.Stream(ref this.projectileId); s.Stream(ref this.effectProtoId);
			s.Stream(ref this.rockSmallProtoId); s.Stream(ref this.rockMediumProtoId); s.Stream(ref this.rockLargeProtoId);
			s.Stream(ref this.firedSound);
			s.Stream(ref this.targetingDelay); s.Stream(ref this.autoShotDelay);
			s.Stream(ref this.autoShotInnerRadius); s.Stream(ref this.autoShotOuterRadius);
			s.Stream(ref this.xOffset); s.Stream(ref this.yOffset); s.Stream(ref this.zOffset);
			s.Stream(ref this.losMode);
		}
		#endregion
	};
}