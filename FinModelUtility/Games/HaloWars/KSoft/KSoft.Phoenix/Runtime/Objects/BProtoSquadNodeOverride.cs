
namespace KSoft.Phoenix.Runtime
{
	struct BProtoSquadNodeOverride
		: IO.IEndianStreamSerializable
	{
		public int baseNodeUnitType, nodeUnitType;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.baseNodeUnitType);
			s.Stream(ref this.nodeUnitType);
		}
		#endregion
	};
}