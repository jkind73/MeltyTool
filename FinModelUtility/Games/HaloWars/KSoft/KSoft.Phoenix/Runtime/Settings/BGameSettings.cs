using System.Collections.Generic;

namespace KSoft.Phoenix.Runtime
{
	sealed class BGameSettings
		: IO.IEndianStreamSerializable
	{
		const uint K_VERSION_ = 1;
		const int K_DEFAULT_SETTINGS_CAPACITY_ = 113;

		public List<BGameSetting> Settings { get; private set; } = new List<BGameSetting>(K_DEFAULT_SETTINGS_CAPACITY_);

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Pad8();
			s.StreamVersion(K_VERSION_);
			BSaveGame.StreamCollection(s, this.Settings);
		}
		#endregion
	};
}
