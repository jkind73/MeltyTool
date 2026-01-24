
using BCost = System.Single;

namespace KSoft.Phoenix.Runtime
{
	abstract class BProtoBuildableObject
		: IO.IEndianStreamSerializable
	{
		public BCost[] cost;
		public float buildPoints;

		public bool forbid;

		public abstract void Serialize(IO.EndianStream s);
	};
}