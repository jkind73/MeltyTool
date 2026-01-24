
namespace KSoft.Phoenix.Runtime
{
	partial class BDatabase
	{
		public struct ProtoSquadEntry
			: IO.IEndianStreamSerializable
		{
			public string name;
			public bool flagObjectProtoSquad;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.StreamPascalString32(ref this.name);
				s.Stream(ref this.flagObjectProtoSquad);
			}
			#endregion
		};
	};
}