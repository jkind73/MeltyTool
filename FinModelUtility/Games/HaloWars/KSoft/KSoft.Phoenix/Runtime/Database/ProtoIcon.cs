
namespace KSoft.Phoenix.Runtime
{
	partial class BDatabase
	{
		public struct ProtoIcon
			: IO.IEndianStreamSerializable
		{
			public string type, name;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.StreamPascalString32(ref this.type);
				s.StreamPascalString32(ref this.name);
			}
			#endregion
		};
	};
}