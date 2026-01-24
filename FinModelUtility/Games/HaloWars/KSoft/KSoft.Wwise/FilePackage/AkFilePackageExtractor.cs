using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Wwise.FilePackage
{
	public sealed class AkFilePackageExtractor
	{
		public string PackageFileName { get; private set; }
		public AkFilePackage Package { get; private set; }

		public IReadOnlyDictionary<uint, string> EventToSoundNameMap { get; private set; }

		public AkFilePackageExtractor(string packageFileName, AkFilePackage package,
			IReadOnlyDictionary<uint, string> eventToSoundNameMap)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(packageFileName));
			Contract.Requires<ArgumentNullException>(package != null);
			Contract.Requires<ArgumentNullException>(eventToSoundNameMap != null);

			this.PackageFileName = packageFileName;
			this.Package = package;
			this.EventToSoundNameMap = eventToSoundNameMap;
		}

		internal Dictionary<SoundBank.HircType, Dictionary<uint, SoundBank.AkSoundBankHierarchyObjectBase>> mObjects =
			new Dictionary<SoundBank.HircType, Dictionary<uint, SoundBank.AkSoundBankHierarchyObjectBase>>();

		internal Dictionary<uint, SoundBank.AkSoundBankHierarchyObjectBase> mIdToObject =
			new Dictionary<uint, SoundBank.AkSoundBankHierarchyObjectBase>();

		internal HashSet<uint> mDupObjects = [];

		internal Dictionary<uint, SoundBank.MediaReference> mUntouched =
			new Dictionary<uint, SoundBank.MediaReference>();

		Dictionary<uint, SoundBank.AkSoundBank> mIdToBank_ =
			new Dictionary<uint, SoundBank.AkSoundBank>();

		bool mPreparedForExtraction_;
		public void PrepareForExtraction()
		{
			if (this.mPreparedForExtraction_)
				return;

			foreach (var bank in this.Package.SoundBanks)
			{
				this.mIdToBank_.Add(bank.Id, bank);
				bank.CopyObjectsTo(this);
			}

			this.BuildSoundNames();

			foreach (var bank in this.Package.SoundBanks)
				bank.PrepareForExtraction();

			this.mPreparedForExtraction_ = true;
		}

		public void BuildSoundNames()
		{
			Dictionary<uint, SoundBank.AkSoundBankHierarchyObjectBase> events;
			if (!this.mObjects.TryGetValue(SoundBank.HircType.EVENT, out events))
			{
				Debug.Trace.FilePackage.TraceInformation("{0} - No events?",
				                                         this.PackageFileName);
				return;
			}

			foreach (var kv in events)
			{
				var e = kv.Value as SoundBank.AkSoundBankHierarchyEvent;

				if (!this.EventToSoundNameMap.TryGetValue(e.id, out e.name))
					continue;

				foreach (var actionId in e.actionList)
				{
					var action = this.mIdToObject[actionId] as SoundBank.AkSoundBankHierarchyAction;
					if (action.type != SoundBank.AkActionType.PLAY)
						continue;

					var target = this.mIdToObject[action.targetId];
					if (target is SoundBank.AkSoundBankHierarchySound)
					{
						(target as SoundBank.AkSoundBankHierarchySound).name = e.name
							.Replace("play_", "");
					}
					else if (target is SoundBank.AkSoundBankHierarchyRanSeqCntr)
					{
						var ranSeq = target as SoundBank.AkSoundBankHierarchyRanSeqCntr;
						if (ranSeq.playlist != null)
						{
							foreach (var item in ranSeq.playlist)
							{
								SoundBank.AkSoundBankHierarchyObjectBase itemObj;
								if (this.mIdToObject.TryGetValue(item.id, out itemObj) &&
									itemObj is SoundBank.AkSoundBankHierarchySound)
								{
									(itemObj as SoundBank.AkSoundBankHierarchySound).name = e.name
										.Replace("play_", "") + "_" + item.id.ToString("X8");
								}
								else
								{
									Debug.Trace.FilePackage.TraceInformation("{0} - {1}: couldn't name item {2} {3}",
									                                         this.PackageFileName, e.name, item.id.ToString("X8"), item.GetType().Name);
								}
							}
						}
						else
						{
							Debug.Trace.FilePackage.TraceInformation("{0} - {1}: couldn't name playlist {2} {3}",
							                                         this.PackageFileName, e.name, target.id.ToString("X8"), SoundBank.HircType.RAN_SEQ_CNTR.ToString());
						}
					}
					else
					{
						Debug.Trace.FilePackage.TraceInformation("{0} - {1}: couldn't name {2} {3}",
						                                         this.PackageFileName, e.name, target.id.ToString("X8"), target.ToString());
					}
				}
			}
		}

		public void ExtractSounds(string path, System.IO.StreamWriter towav, IO.EndianReader pckReader,
			bool overwriteExisting = false)
		{
			Dictionary<uint, SoundBank.AkSoundBankHierarchyObjectBase> sounds;
			if (!this.mObjects.TryGetValue(SoundBank.HircType.SOUND, out sounds))
			{
				Debug.Trace.FilePackage.TraceInformation("{0} - No sounds to extract?",
				                                         this.PackageFileName);
				return;
			}

			foreach (var kv in sounds)
			{
				var snd = kv.Value as SoundBank.AkSoundBankHierarchySound;
				if (snd.name == null && this.mDupObjects.Contains(kv.Key))
					continue;

				uint srcId = snd.source.mediaInfo.sourceId;
				this.mUntouched.Remove(srcId);

				if (snd.source.mediaInfo.fileId == 0)
				{
					towav.WriteLine("REM NoData {0}", snd.name);
					continue;
				}

				string dir = null;
				string filename = (snd.name ?? ("unknown_" + kv.Key.ToString("X8"))) + ".xma";

				uint bankId = snd.bankId;
				string bankName;
				if (!this.Package.IdToName.TryGetValue(bankId, out bankName))
					bankName = bankId.ToString("X8");

				SoundBank.AkSoundBankData bankData = null;
				bool streamed = snd.source.streamType != SoundBank.AkBankSourceData.SourceType.DATA;
				if (!streamed)
					bankData = this.mIdToBank_[bankId].mData;

				dir = System.IO.Path.Combine(path, bankName);
				System.IO.Directory.CreateDirectory(dir);

				string fullPath = System.IO.Path.Combine(dir, filename);
				towav.WriteLine("towav.exe {0}", fullPath);

				if (System.IO.File.Exists(fullPath) && !overwriteExisting)
					continue;

				using (var fs = System.IO.File.Create(fullPath))
				{
					if (streamed)
					{
						var file = this.Package.FindStreamedFileById(snd.source.mediaInfo.fileId);
						pckReader.Seek(file.FileOffset);
						byte[] data = pckReader.ReadBytes((int)file.FileSize32);

						fs.Write(data, 0, data.Length);
					}
					else
					{
						fs.Write(bankData.buffer, (int)snd.source.mediaInfo.fileOffset, (int)snd.source.mediaInfo.mediaSize);
					}
				}
			}
			//////////////////////////////////////////////////////////////////////////
			foreach (var kv in this.mUntouched)
			{
				var mr = kv.Value;

				string name = "unknown2_" + mr.media.id.ToString("X8");
				if (mr.media.size == 0)
				{
					towav.WriteLine("REM NoData2 {0}", name);
					continue;
				}

				string filename = name + ".xma";

				uint bankId = mr.bankId;
				string bankName;
				if (!this.Package.IdToName.TryGetValue(bankId, out bankName))
					bankName = bankId.ToString("X8");

				SoundBank.AkSoundBankData bankData = this.mIdToBank_[bankId].mData;

				string dir = System.IO.Path.Combine(path, bankName);
				System.IO.Directory.CreateDirectory(dir);

				string fullPath = System.IO.Path.Combine(dir, filename);
				towav.WriteLine("towav.exe {0}", fullPath);

				if (System.IO.File.Exists(fullPath))
					continue;

				using (var fs = System.IO.File.Create(fullPath))
				{
					fs.Write(bankData.buffer, (int)mr.media.offset, (int)mr.media.size);
				}
			}
		}
	};
}
