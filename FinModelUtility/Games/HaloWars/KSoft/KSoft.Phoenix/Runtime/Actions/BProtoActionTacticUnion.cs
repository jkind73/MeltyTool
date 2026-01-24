
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	// lol idk
	struct BProtoActionTacticUnion
		: IO.IEndianStreamSerializable
	{
		public bool protoAction;
		public sbyte playerId;
		public BProtoObjectID protoObjectId;
		public sbyte protoActionIndex;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.protoAction);
			s.Stream(ref this.playerId);
			s.Stream(ref this.protoObjectId);
			s.Stream(ref this.protoActionIndex);
		}
		#endregion
	};
}