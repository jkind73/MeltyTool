
namespace KSoft.Wwise.SoundBank
{
	using SourceTypeStreamer = IO.EnumBinaryStreamer<AkBankSourceData.SourceType>;

	sealed class AkBankSourceData
		: IO.IEndianStreamSerializable
	{
		enum AkPluginType
		{
			NONE,
			CODEC,
			SOURCE,
			EFFECT,
			MOTION_DEVICE,
			MOTION_SOURCE,
		};
		public enum SourceType : uint
		{
			DATA,
			STREAMING,
			PREFETCH_STREAMING,

			NOT_INITIALIZED = uint.MaxValue
		};

		const uint AK_PLUGIN_TYPE_MASK_ = 0xF;

		public uint pluginId;
		public SourceType streamType;
		public uint audioFormatSampleRate;
		// uChannelMask : 18
		// uBitsPerSample : 6
		// uBlockAlign : 5
		// uTypeID : 2
		// uInterleaveID : 1
		public uint audioFormatBits;
		public AkMediaInformation mediaInfo;

		public CAkParameterNodeBase parameterNode = new CAkParameterNodeBase();

		public bool Prefetch { get { return this.streamType == SourceType.PREFETCH_STREAMING; } }

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.pluginId);
			s.Stream(ref this.streamType, SourceTypeStreamer.Instance);
			s.Stream(ref this.audioFormatSampleRate);
			s.Stream(ref this.audioFormatBits);
			s.Stream(ref this.mediaInfo.sourceId);
			s.Stream(ref this.mediaInfo.fileId);
			if (this.streamType != SourceType.STREAMING)
			{
				s.Stream(ref this.mediaInfo.fileOffset);
				s.Stream(ref this.mediaInfo.mediaSize);
			}
			s.Stream(ref this.mediaInfo.isLanguageSpecific);

			var ptype = (AkPluginType)(this.pluginId & AK_PLUGIN_TYPE_MASK_);
			if (ptype == AkPluginType.SOURCE || ptype == AkPluginType.MOTION_SOURCE)
				s.Pad32(); // size
#if false
			s.Stream(ParameterNode);
#endif
			// There's more...
		}
		#endregion
	};
}