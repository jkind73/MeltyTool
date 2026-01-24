
namespace KSoft.Phoenix.Resource
{
	public enum ResourceTagPlatformId : byte
	{
		ANY,
		PC,
		XBOX,
		_64bit,
	};

	public sealed class ResourceTagHeader
		: IO.IEndianStreamSerializable
	{
		public const ulong K_CHUNK_ID = 0x00000000714BFE00;

		const ushort K_SIGNATURE_ = 0x714C;
		const byte K_MAJOR_VERSION_ = 0x11;
		const byte K_MINOR_VERSION_ = 0x0;

		public Shell.ProcessorSize CreatorPointerSize { get; private set; }

		public ushort headerSize;
		public ushort dataSize;
		public uint headerAdler32;

		public ulong tagTimeStamp; // FILETIME
		public Values.KGuid tagGuid;
		Values.PtrHandle tagMachineNameOffset_;
		Values.PtrHandle tagUserNameOffset_;

		Values.PtrHandle sourceFileName_;
		public byte[] sourceDigest = new byte[Security.Cryptography.PhxHash.K_SHA1_SIZE_OF];
		public ulong sourceFileSize;
		public ulong sourceFileTimeStamp;

		Values.PtrHandle creatorToolCommandLine_;
		public byte creatorToolVersion;

		private byte mPlatformId_;
		public ResourceTagPlatformId PlatformId
		{
			get { return (ResourceTagPlatformId) this.mPlatformId_; }
			set { this.mPlatformId_ = (byte)value; }
		}

		public ResourceTagHeader(Shell.ProcessorSize pointerSize = Shell.ProcessorSize.X32)
		{
			if (pointerSize == Shell.ProcessorSize.X32)
			{
				this.tagMachineNameOffset_ = Values.PtrHandle.InvalidHandle32;
				this.tagUserNameOffset_ = Values.PtrHandle.InvalidHandle32;
				this.creatorToolCommandLine_ = Values.PtrHandle.InvalidHandle32;
			}
			else
			{
				this.tagMachineNameOffset_ = Values.PtrHandle.InvalidHandle64;
				this.tagUserNameOffset_ = Values.PtrHandle.InvalidHandle64;
				this.creatorToolCommandLine_ = Values.PtrHandle.InvalidHandle64;
			}
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			using (s.BeginEndianSwitch(Shell.EndianFormat.LITTLE))
			{
				s.VirtualAddressTranslationInitialize(this.CreatorPointerSize);
				s.VirtualAddressTranslationPushPosition();
				this.SerializeBody(s);
				s.VirtualAddressTranslationPop();
			}
		}

		private void SerializeBody(IO.EndianStream s)
		{
			s.StreamSignature(K_SIGNATURE_);
			s.StreamVersion(K_MAJOR_VERSION_);
			s.StreamVersion(K_MINOR_VERSION_);

			s.Stream(ref this.headerSize);
			s.Stream(ref this.dataSize);
			s.Stream(ref this.headerAdler32);

			s.Stream(ref this.tagTimeStamp);
			s.Stream(ref this.tagGuid);

			s.StreamVirtualAddress(ref this.tagMachineNameOffset_);
			s.StreamVirtualAddress(ref this.tagUserNameOffset_);

			s.StreamVirtualAddress(ref this.sourceFileName_);
			s.Stream(this.sourceDigest);
			s.Stream(ref this.sourceFileSize);
			s.Stream(ref this.sourceFileTimeStamp);

			s.StreamVirtualAddress(ref this.creatorToolCommandLine_);
			s.Stream(ref this.creatorToolVersion);

			s.Stream(ref this.mPlatformId_);
			s.Pad(sizeof(byte) + sizeof(uint));
		}

		public bool StreamTagMachineName(IO.EndianStream s, ref string value)
		{
			return PhxUtil.StreamPointerizedCString(s, ref this.tagMachineNameOffset_, ref value);
		}

		public bool StreamTagUserName(IO.EndianStream s, ref string value)
		{
			return PhxUtil.StreamPointerizedCString(s, ref this.tagUserNameOffset_, ref value);
		}

		public bool StreamSourceFileNamee(IO.EndianStream s, ref string value)
		{
			return PhxUtil.StreamPointerizedCString(s, ref this.sourceFileName_, ref value);
		}

		public bool StreamCreatorToolCommandLine(IO.EndianStream s, ref string value)
		{
			return PhxUtil.StreamPointerizedCString(s, ref this.creatorToolCommandLine_, ref value);
		}
		#endregion
	};
}