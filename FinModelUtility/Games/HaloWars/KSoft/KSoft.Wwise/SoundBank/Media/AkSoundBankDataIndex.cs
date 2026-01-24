
namespace KSoft.Wwise.SoundBank
{
	partial class AkSoundBankObjectBase
	{
		static readonly Values.GroupTagData32 KMediaIndexSignature =
					new Values.GroupTagData32("DIDX", "audiokinetic_sound_bank_data_index"); // BankDataIndexChunkID

		static AkSoundBankObjectBase NewDidx(uint generatorVersion)
		{
			return new AkSoundBankDataIndex();
		}
	};

	sealed class AkSoundBankDataIndex
		: AkSoundBankObjectBase
	{
		public AkMediaHeader[] loadedMedia;

		public override void Serialize(IO.EndianStream s, AkSubchunkHeader header)
		{
			if (s.IsReading)
				this.loadedMedia = new AkMediaHeader[header.chunkSize / AkMediaHeader.K_SIZE_OF];

			s.StreamArray(this.loadedMedia);
		}
	};
}