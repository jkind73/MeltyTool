using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Wwise.SoundBank
{
	using HircTypeStreamer8 = IO.EnumBinaryStreamer<HircType, byte>;
	using HircTypeStreamer32 = IO.EnumBinaryStreamer<HircType, uint>;

	partial class AkSoundBankObjectBase
	{
		static readonly Values.GroupTagData32 KHierarchySignature =
					new Values.GroupTagData32("HIRC", "audiokinetic_hierarchy"); // BankHierarchyChunkID

		static AkSoundBankObjectBase NewHirc(uint generatorVersion)
		{
			return new AkSoundBankHierarchy();
		}
	};

	sealed class AkSoundBankHierarchy
		: AkSoundBankObjectBase
	{
		struct AkbkSubHircSection
			: IO.IEndianStreamSerializable
		{
			public HircType type;
			public uint sectionSize;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				uint sdkVer = (s.Owner as AkSoundBank).SdkVersion;

				s.Stream(ref this.type, AkVersion.HircTypeIs8Bit(sdkVer)
					? HircTypeStreamer8.Instance
					: HircTypeStreamer32.Instance);
				s.Stream(ref this.sectionSize);
			}
			#endregion
		};

		Dictionary<HircType, Dictionary<uint, AkSoundBankHierarchyObjectBase>> mObjects_ =
			new Dictionary<HircType, Dictionary<uint, AkSoundBankHierarchyObjectBase>>();
		Dictionary<uint, AkSoundBankHierarchyObjectBase> mIdToObject_ =
			new Dictionary<uint, AkSoundBankHierarchyObjectBase>();

		public void CopyObjectsTo(FilePackage.AkFilePackageExtractor extractor)
		{
			foreach (var kv in this.mObjects_)
			{
				var type = kv.Key;

				if (type == HircType.ATTENUATION)
					continue;

				Dictionary<uint, AkSoundBankHierarchyObjectBase> dic;
				if (!extractor.mObjects.TryGetValue(type, out dic))
					extractor.mObjects.Add(type, dic = new Dictionary<uint, AkSoundBankHierarchyObjectBase>());

				foreach (var obj in kv.Value)
				{
					if (dic.ContainsKey(obj.Key))
					{
						extractor.mDupObjects.Add(obj.Key);
						continue;
					}

					dic.Add(obj.Key, obj.Value);
					extractor.mIdToObject.Add(obj.Key, obj.Value);
				}
			}
		}

		void MapObject(HircType type, AkSoundBankHierarchyObjectBase obj)
		{
			Dictionary<uint, AkSoundBankHierarchyObjectBase> dic;
			if (!this.mObjects_.TryGetValue(type, out dic))
				this.mObjects_.Add(type, dic = new Dictionary<uint, AkSoundBankHierarchyObjectBase>());

			dic.Add(obj.id, obj);
			this.mIdToObject_.Add(obj.id, obj);
		}

		#region IEndianStreamSerializable Members
		void SerializeItem(IO.EndianStream s, AkbkSubHircSection section)
		{
			Contract.Assert(s.IsReading);

			using (s.EnterVirtualBufferWithBookmark(section.sectionSize))
			{
				var obj = AkSoundBankHierarchyObjectBase.New(section.type);
				if (obj != null)
				{
					s.Stream(obj);

					this.MapObject(section.type, obj);
				}
			}
		}
		void FromStream(IO.EndianStream s, AkSubchunkHeader header)
		{
			var bank = s.Owner as AkSoundBank;

			using (s.EnterVirtualBufferWithBookmark(header.chunkSize))
			{
				for (int x = 0, numHircItems = s.Reader.ReadInt32(); x < numHircItems; x++)
				{
					var section = new AkbkSubHircSection(); section.Serialize(s);

					this.SerializeItem(s, section);
				}
			}
		}
		public override void Serialize(IO.EndianStream s, AkSubchunkHeader header)
		{
			if (s.IsReading)
				this.FromStream(s, header);
		}
		#endregion

		internal void PrepareForExtraction(AkSoundBank bank)
		{
			foreach (var kv in this.mObjects_)
			{
				if (kv.Key != HircType.SOUND)
					continue;

				foreach(var dic in kv.Value)
					((AkSoundBankHierarchySound)dic.Value).PrepareForExtraction(bank);
			}
		}
	};
}