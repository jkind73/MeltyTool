
namespace KSoft.Wwise.FilePackage
{
	public struct AkLanguageMapEntry
		: IO.IEndianStreamSerializable
	{
		/// <summary>Size of this struct on disk</summary>
		public const int K_SIZE_OF = sizeof(uint) * 2;

		public uint offset;
		public uint id;

		public string value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.offset);
			s.Stream(ref this.id);
		}
		#endregion
	};
}