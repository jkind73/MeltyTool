
namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscHealUser
		: BPowerUser
	{
		public BPowerHelperHudSounds hudSounds = new BPowerHelperHudSounds();

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(this.hudSounds);
		}
		#endregion
	};
}