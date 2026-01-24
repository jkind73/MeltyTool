
namespace KSoft.Phoenix.Runtime
{
	partial class BDatabase
	{
		public struct GenericProtoObjectEntry
			: IO.IEndianStreamSerializable
		{
			public string name;
			public int id;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.StreamPascalString32(ref this.name);
				s.Stream(ref this.id);
			}
			#endregion
		};
	};
}