
namespace KSoft.Wwise.SoundBank
{
	partial class AkSoundBankObjectBase
	{
		static readonly Values.GroupTagData32 KDataSignature =
					new Values.GroupTagData32("DATA", "audiokinetic_sound_bank_data"); // BankDataChunkID

		static AkSoundBankObjectBase NewData(uint generatorVersion)
		{
			return new AkSoundBankData();
		}
	};

	sealed class AkSoundBankData
		: AkSoundBankObjectBase
	{
		public byte[] buffer;

		public override void Serialize(IO.EndianStream s, AkSubchunkHeader header)
		{
			if (s.IsReading)
				this.buffer = new byte[header.chunkSize];

			s.Stream(this.buffer);
		}
	};
}