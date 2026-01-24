
namespace KSoft.Wwise.SoundBank
{
	sealed class AkSoundBankHierarchyEvent
		: AkSoundBankHierarchyObjectBase
	{
		public uint[] actionList;

		public string name;

		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.StreamArrayInt32(ref this.actionList, s.Stream);
		}
	};
}