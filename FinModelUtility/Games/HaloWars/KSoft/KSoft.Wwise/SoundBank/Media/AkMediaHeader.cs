
namespace KSoft.Wwise.SoundBank
{
	public struct AkMediaHeader
		: IO.IEndianStreamSerializable
	{
		/// <summary>Size of this struct on disk</summary>
		public const int K_SIZE_OF = sizeof(uint) * 3;

		public uint id, offset, size;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.id);
			s.Stream(ref this.offset);
			s.Stream(ref this.size);
		}
		#endregion
	};
}