#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Wwise.SoundBank
{
	partial class AkSoundBank
	{
		struct AkBankHeader : IO.IEndianStreamSerializable
		{
			/// <summary>Size of this struct on disk</summary>
			public const int K_SIZE_OF = sizeof(uint) * 4;

			public uint bankGeneratorVersion, soundBankId, languageId, feedbackSupported;

			#region IEndianStreamSerializable Members
			void SerializeOld(IO.EndianStream s)
			{
				Contract.Assert(s.IsReading);

				s.Pad32(); // Type; 0 or 1 (Init.bk)
				s.Pad32(); // LanguageID?
				s.Stream(ref this.bankGeneratorVersion);
				s.Pad32(); // seen as '0', '12'
				s.Pad32(); // some kind of ID
				s.Stream(ref this.soundBankId);
			}
			public void Serialize(IO.EndianStream s)
			{
				uint sdkVer = (s.Owner as AkSoundBank).SdkVersion;

				if (AkVersion.HasOldBankHeader(sdkVer))
					this.SerializeOld(s);
				else
				{
					s.Stream(ref this.bankGeneratorVersion);
					s.Stream(ref this.soundBankId);
					s.Stream(ref this.languageId);
					s.Stream(ref this.feedbackSupported);
				}
			}
			#endregion
		};
	};
}