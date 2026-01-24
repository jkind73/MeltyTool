
namespace KSoft.Wwise
{
	struct AkSubchunkHeader
		: IO.IEndianStreamSerializable
	{
		/// <summary>Size of this struct on disk</summary>
		public const int K_SIZE_OF = sizeof(uint) * 2;

		public uint tag, chunkSize;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamTagBigEndian(ref this.tag);
			s.Stream(ref this.chunkSize);
		}
		#endregion

		public override int GetHashCode() { return (int) this.tag; }
	};
}