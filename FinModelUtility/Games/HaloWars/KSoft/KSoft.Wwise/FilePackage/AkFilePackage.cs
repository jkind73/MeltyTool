using System.Collections.Generic;

namespace KSoft.Wwise.FilePackage
{
	public sealed class AkFilePackage
		: IO.IEndianStreamSerializable
	{
		internal AkFilePackageSettings Settings { get; private set; }

		AkFilePackageHeader mHeader_;
		AkLanguageMap mLangMap_;
		AkFileLookupTable mSoundBanksTable_;
		AkFileLookupTable mStreamedFilesTable_;
		AkFileLookupTable mExternalFilesTable_;

		SoundBank.AkSoundBank[] mSoundBanks_;
		Dictionary<uint, string> mIdToName_;
		List<KeyValuePair<uint, string>> mIdToNameDups_;

		bool HasExternalFiles { get {
			return this.mExternalFilesTable_ != null;
		} }

		public AkFilePackage(AkFilePackageSettings settings)
		{
			this.Settings = settings;

			this.mLangMap_ = new AkLanguageMap(settings.UseAsciiStrings);

			this.mSoundBanksTable_ = new AkFileLookupTable();
			this.mStreamedFilesTable_ = new AkFileLookupTable();

			if (AkVersion.HasExternalFiles(settings.SdkVersion))
				this.mExternalFilesTable_ = new AkFileLookupTable();
		}

		public IReadOnlyDictionary<uint, string> IdToName { get { return this.mIdToName_; } }
		public IEnumerable<SoundBank.AkSoundBank> SoundBanks { get { return this.mSoundBanks_; } }

		internal void MapIdToName(uint id, string name)
		{
			string existingName;
			if (this.mIdToName_.TryGetValue(id, out existingName))
			{
				if (existingName != name)
					this.mIdToNameDups_.Add(new KeyValuePair<uint, string>(id, name));
			}
			else
				this.mIdToName_.Add(id, name);
		}

		internal AkFileLookupTableEntry FindStreamedFileById(ulong streamedFileId)
		{
			return this.mStreamedFilesTable_.Find(streamedFileId);
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Owner = this;

			if(s.IsWriting)
				this.mHeader_.InitializeSize(this.Settings.SdkVersion, this.mLangMap_.totalMapSize);

			s.Stream(ref this.mHeader_);
			s.Stream(ref this.mLangMap_.totalMapSize);
			s.Stream(ref this.mSoundBanksTable_.totalSize);
			s.Stream(ref this.mStreamedFilesTable_.totalSize);
			if (this.HasExternalFiles) s.Stream(ref this.mExternalFilesTable_.totalSize);
			s.Stream(this.mLangMap_);
			s.Stream(this.mSoundBanksTable_);
			s.Stream(this.mStreamedFilesTable_);
			if (this.HasExternalFiles) s.Stream(this.mExternalFilesTable_);
		}

		public void SerializeSoundBanks(IO.EndianStream s)
		{
			if (s.IsReading)
			{
				this.mSoundBanks_ = new SoundBank.AkSoundBank[this.mSoundBanksTable_.Count];
				this.mIdToName_ = new Dictionary<uint, string>(this.mSoundBanks_.Length);
				this.mIdToNameDups_ = [];
				for (int x = 0; x < this.mSoundBanksTable_.Count; x++)
				{
					var entry = this.mSoundBanksTable_[x];
					this.mSoundBanks_[x] = new SoundBank.AkSoundBank((long)entry.FileSize64, entry.FileOffset, this);
				}
			}

			for (int x = 0; x < this.mSoundBanks_.Length; x++)
				s.Stream(this.mSoundBanks_[x]);

			this.mSoundBanks_.ToString();
		}
		#endregion
	};
}