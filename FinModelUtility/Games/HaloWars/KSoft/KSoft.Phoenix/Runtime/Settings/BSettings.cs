
namespace KSoft.Phoenix.Runtime
{
	sealed class BSettings : IO.IEndianStreamSerializable
	{
		const uint K_VERSION_ = 0;

		public BGameSettings GameSettings { get; private set; } = new BGameSettings();
		public BConfigSettings ConfigSettings { get; private set; } = new BConfigSettings();

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamVersion(K_VERSION_);
			s.Stream(this.GameSettings);
			s.Stream(this.ConfigSettings);
		}
		#endregion
	};
}
