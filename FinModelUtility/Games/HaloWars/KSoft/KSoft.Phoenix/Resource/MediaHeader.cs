using System.Security.Cryptography;

namespace KSoft.Phoenix.Resource
{
	using PhxHash = Security.Cryptography.PhxHash;

	public sealed class MediaHeader
		: IO.IEndianStreamSerializable
	{
		static readonly Memory.Strings.StringStorage KNameStorage = new Memory.Strings.StringStorage(
			Memory.Strings.StringStorageWidthType.UNICODE, Memory.Strings.StringStorageType.C_STRING,
			Shell.EndianFormat.BIG, 32);
		static readonly Memory.Strings.StringStorage KDescStorage = new Memory.Strings.StringStorage(
			Memory.Strings.StringStorageWidthType.UNICODE, Memory.Strings.StringStorageType.C_STRING,
			Shell.EndianFormat.BIG, 128);
		static readonly Memory.Strings.StringStorage KAuthorStorage = new Memory.Strings.StringStorage(
			Memory.Strings.StringStorageWidthType.ASCII, Memory.Strings.StringStorageType.C_STRING, 16);

		const byte K_VERSION_ = 2;
		internal const int K_SIZE_OF = 0x1C0;

		const int K_PADDING_LENGTH_ = 13;

		public ulong id;
		public string name;
		public string description;
		public string author;
		#region DateTime
		PhxSystemtime mDateTime_;
		public PhxSystemtime DateTime { get { return this.mDateTime_; } }
		#endregion
		public ulong authorXuid;
		public float length;
		public short sessionId;
		public int gameType;
		public ulong dataCryptKey;
		public byte[] dataHash = new byte[PhxHash.K_SHA1_SIZE_OF];
		public ulong dataSize;

		public byte[] Hash { get; private set; } = new byte[PhxHash.K_SHA1_SIZE_OF];

		public void UpdateHash(SHA1CryptoServiceProvider sha)
		{
			PhxHash.UInt8(sha, K_VERSION_);
			PhxHash.UInt64(sha, this.id);
			PhxHash.Unicode(sha, this.name, KNameStorage.FixedLength-1);
			PhxHash.Unicode(sha, this.description, KDescStorage.FixedLength-1);
			PhxHash.Ascii(sha, this.author, KAuthorStorage.FixedLength);
			this.DateTime.UpdateHash(sha);
			PhxHash.UInt64(sha, this.authorXuid);
			PhxHash.UInt32(sha, Bitwise.ByteSwap.SingleToUInt32(this.length));
			PhxHash.UInt16(sha, (uint) this.sessionId);
			PhxHash.UInt32(sha, (uint) this.gameType);
			PhxHash.UInt64(sha, this.dataCryptKey);
			sha.TransformBlock(this.dataHash, 0, this.dataHash.Length, null, 0);
			PhxHash.UInt64(sha, this.dataSize, isFinal: true);
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamVersion(K_VERSION_);
			s.Stream(ref this.id);
			s.Stream(ref this.name, KNameStorage);
			s.Stream(ref this.description, KDescStorage);
			s.Stream(ref this.author, KAuthorStorage);
			s.Stream(ref this.mDateTime_);
			s.Stream(ref this.authorXuid);
			s.Stream(ref this.length);
			s.Stream(ref this.sessionId);
			s.Stream(ref this.gameType);
			s.Stream(ref this.dataCryptKey);
			s.Stream(this.dataHash, 0, this.dataHash.Length);
			s.Stream(ref this.dataSize);
			s.Stream(this.Hash, 0, this.Hash.Length);
			s.Pad(K_PADDING_LENGTH_);
		}
		#endregion
	};
}
