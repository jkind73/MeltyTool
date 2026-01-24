
namespace KSoft.Wwise.FilePackage
{
	struct AkFilePackageHeader
		: IO.IEndianStreamSerializable
	{
		const uint K_SIZE_OF_HEADER_ = AkSubchunkHeader.K_SIZE_OF + sizeof(int);

		static readonly Values.GroupTagData32 KSignature = new Values.GroupTagData32("AKPK", "audiokinetic_package");
		const uint K_VERSION_ = 1;

		public uint headerSize;

		public void InitializeSize(uint sdkVersion, uint langMapTotalSize)
		{
			this.headerSize = 0;
			this.headerSize += K_SIZE_OF_HEADER_;
			this.headerSize += sizeof(uint); // field for lang map size
			this.headerSize += sizeof(uint); // field for LUT size (sound banks)
			this.headerSize += sizeof(uint);    // field for LUT size (streamed files)
			if (AkVersion.HasExternalFiles(sdkVersion))
				this.headerSize += sizeof(uint); // field for LUT size (external files)
			this.headerSize += langMapTotalSize;
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamSignature(KSignature.TagString, Memory.Strings.StringStorage.AsciiString);
			s.Stream(ref this.headerSize);
			s.StreamVersion(K_VERSION_);
		}
		#endregion
	};
}