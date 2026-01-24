
namespace KSoft.Phoenix.Runtime
{
	abstract class BProtoObjectBase
		: BProtoBuildableObject
	{
		public short baseType;

		public bool available;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.baseType);
		}
		#endregion
	};
}