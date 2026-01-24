
using BCueIndex = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerHelperHudSounds
		: IO.IEndianStreamSerializable
	{
		public BCueIndex hudUpSound, hudAbortSound, hudFireSound,
			hudLastFireSound, hudStartEnvSound, hudStopEnvSound;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.hudUpSound); s.Stream(ref this.hudAbortSound); s.Stream(ref this.hudFireSound);
			s.Stream(ref this.hudLastFireSound); s.Stream(ref this.hudStartEnvSound); s.Stream(ref this.hudStopEnvSound);
		}
		#endregion
	};
}