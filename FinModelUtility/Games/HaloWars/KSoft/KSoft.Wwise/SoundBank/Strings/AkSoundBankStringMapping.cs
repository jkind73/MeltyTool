#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Wwise.SoundBank
{
	sealed class AkSoundBankStringMapping
		: AkSoundBankStringMappingBase
	{
		static readonly Memory.Strings.StringStorage KStringStorage =
					new Memory.Strings.StringStorage(Memory.Strings.StringStorageWidthType.ASCII, Memory.Strings.StringStorageLengthPrefix.INT8);
		static readonly Text.StringStorageEncoding KStringEncoding = new Text.StringStorageEncoding(KStringStorage);

		void SerializeStringType(IO.EndianStream s, AkbkHashHeader hdr, AkSoundBank bank)
		{
			Contract.Assert(hdr.Type == StringType.BANK);

			uint bankId = uint.MaxValue;
			string str = null;

			s.Stream(ref bankId);
			s.Stream(ref str, KStringEncoding);

			bank.MapIdToName(bankId, str);
		}
		public override void Serialize(IO.EndianStream s, AkSubchunkHeader header)
		{
			Contract.Assert(s.IsReading);

			var bank = s.Owner as AkSoundBank;

			long eos = EndOfStream(s, header);

			while (s.BaseStream.Position != eos)
			{
				AkbkHashHeader hdr = new AkbkHashHeader();
				hdr.Serialize(s);

				for (int x = 0; x < hdr.Size; x++)
					this.SerializeStringType(s, hdr, bank);
			}
		}
	};
}