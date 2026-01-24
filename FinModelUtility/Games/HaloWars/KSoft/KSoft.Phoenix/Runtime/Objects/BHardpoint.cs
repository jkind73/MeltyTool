
using BUnitOppID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	struct BHardpoint
		: IO.IEndianStreamSerializable
	{
		public const int K_MAX_COUNT = 0x14;

		public float yawRotationRate, pitchRotationRate;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.yawRotationRate);
			s.Stream(ref this.pitchRotationRate);
		}
		#endregion
	};

	public struct BHardpointState
		: IO.IEndianStreamSerializable
	{
		public int ownerAction;
		public float autoCenteringTimer, yawSoundActivationTimer, pitchSoundActivationTimer;
		public BUnitOppID oppId;
		public bool
			allowAutoCentering, yawSound, yawSoundPlaying,
			pitchSound, pitchSoundPlaying, secondaryTurretScanToken
			;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.ownerAction);
			s.Stream(ref this.autoCenteringTimer);	s.Stream(ref this.yawSoundActivationTimer);	s.Stream(ref this.pitchSoundActivationTimer);
			s.Stream(ref this.oppId);
			s.Stream(ref this.allowAutoCentering);	s.Stream(ref this.yawSound);				s.Stream(ref this.yawSoundPlaying);
			s.Stream(ref this.pitchSound);			s.Stream(ref this.pitchSoundPlaying);	s.Stream(ref this.secondaryTurretScanToken);
		}
		#endregion
	};
}