
namespace KSoft.Wwise.SoundBank
{
	abstract partial class AkSoundBankObjectBase
	{
		protected static long EndOfStream(IO.EndianStream s, AkSubchunkHeader header)
		{
			return s.BaseStream.Position + header.chunkSize;
		}

		public abstract void Serialize(IO.EndianStream s, AkSubchunkHeader header);

		#region Factory
		static readonly Values.GroupTagData32 KGlobalSettingsSignature =
					new Values.GroupTagData32("STMG", "audiokinetic_global_settings"); // BankStateMgrChunkID

		static readonly Values.GroupTagData32 KFxParamsSignature =
					new Values.GroupTagData32("FXPR", "audiokinetic_fx_params"); // BankFXParamsChunkID
		static readonly Values.GroupTagData32 KEnvSettingsSignature =
					new Values.GroupTagData32("ENVS", "audiokinetic_env_settings"); // BankEnvSettingChunkID

		public static AkSoundBankObjectBase New(uint chunkId, uint generatorVersion)
		{
				 if (chunkId == KHierarchySignature.Id)		return NewHirc(generatorVersion);
			else if (chunkId == KStringMappingSignature.Id)	return NewStid(generatorVersion);
			else if (chunkId == KDataSignature.Id)			return NewData(generatorVersion);
			else if (chunkId == KMediaIndexSignature.Id)	return NewDidx(generatorVersion);

			return null;
		}
		#endregion
	};
}