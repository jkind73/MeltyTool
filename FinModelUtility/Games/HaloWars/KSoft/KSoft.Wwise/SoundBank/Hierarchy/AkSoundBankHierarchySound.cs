
namespace KSoft.Wwise.SoundBank
{
	sealed class AkSoundBankHierarchySound
		: AkSoundBankHierarchyObjectBase
	{
		public AkBankSourceData source = new AkBankSourceData();

		public string name;
		public uint bankId;

		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(this.source);
			// There's more...
		}

		internal void PrepareForExtraction(AkSoundBank bank)
		{
			if (this.source.streamType != AkBankSourceData.SourceType.DATA)
				this.bankId = bank.Id;
			else
				this.bankId = this.source.mediaInfo.fileId;
		}
	};
}