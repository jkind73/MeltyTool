
namespace KSoft.Phoenix.Runtime
{
	sealed class BGameSetting
		: IO.IEndianStreamSerializable
	{
		string mName_;
		BGameSettingVariant mValue_;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.mName_);
			s.Stream(ref this.mValue_);
		}
		#endregion
	};
}