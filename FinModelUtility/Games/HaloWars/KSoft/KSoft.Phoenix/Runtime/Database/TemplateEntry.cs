
namespace KSoft.Phoenix.Runtime
{
	partial class BDatabase
	{
		public struct TemplateEntry
			: IO.IEndianStreamSerializable
		{
			public string name; // path
			public int modelIndex;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.StreamPascalString32(ref this.name);
				s.Stream(ref this.modelIndex);
			}
			#endregion
		};
	};
}