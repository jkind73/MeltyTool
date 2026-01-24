
namespace KSoft.Wwise.SoundBank
{
	public struct AkPlaylistItem
		: IO.IEndianStreamSerializable
	{
		/// <summary>Size of this struct on disk</summary>
		internal const uint K_SIZE_OF = 5;

		public uint id;
		public sbyte weight;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.id);
			s.Stream(ref this.weight);
		}
		#endregion
	};
}