
namespace KSoft.Phoenix.Runtime
{
	partial class BDatabase
	{
		public struct DataTagValue
			: IO.IEndianStreamSerializable
		{
			public string name;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.StreamPascalString32(ref this.name);
			}
			#endregion
		};
		static readonly CondensedListInfo KDataTagsListInfo = new CondensedListInfo()
		{
			SerializeCapacity=true,
			IndexSize=sizeof(int),
			MaxCount=0x3E8,
		};
	};
}