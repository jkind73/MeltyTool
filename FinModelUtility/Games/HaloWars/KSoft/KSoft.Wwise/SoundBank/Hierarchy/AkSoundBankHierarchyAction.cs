
namespace KSoft.Wwise.SoundBank
{
	using AkActionTypeStreamer = IO.EnumBinaryStreamer<AkActionType>;

	sealed class AkSoundBankHierarchyAction
		: AkSoundBankHierarchyObjectBase
	{
		public AkActionType type;
		public uint targetId;

		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref this.type, AkActionTypeStreamer.Instance);
			s.Stream(ref this.targetId);
			// There's more...
		}
	};
}