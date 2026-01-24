
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscMacUser
		: BPowerUser
	{
		public BPowerHelperHudSounds hudSounds = new BPowerHelperHudSounds();
		public string helpString;
		public BEntityID fakeTargettingLaserId, realTargettingLaserId, targettedSquadId;
		public uint shotsRemaining;
		public float lastCommandSent, commandInterval, lastShotSent, shotInterval;
		public int losMode;
		public bool flagLastCameraAutoZoomInstantEnabled, flagLastCameraAutoZoomEnabled,
			flagLastCameraZoomEnabled, flagLastCameraYawEnabled;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(this.hudSounds);
			s.StreamPascalWideString32(ref this.helpString);
			s.Stream(ref this.fakeTargettingLaserId); s.Stream(ref this.realTargettingLaserId); s.Stream(ref this.targettedSquadId);
			s.Stream(ref this.shotsRemaining);
			s.Stream(ref this.lastCommandSent); s.Stream(ref this.commandInterval); s.Stream(ref this.lastShotSent); s.Stream(ref this.shotInterval);
			s.Stream(ref this.losMode);
			s.Stream(ref this.flagLastCameraAutoZoomInstantEnabled); s.Stream(ref this.flagLastCameraAutoZoomEnabled);
			s.Stream(ref this.flagLastCameraZoomEnabled); s.Stream(ref this.flagLastCameraYawEnabled);
		}
		#endregion
	};
}