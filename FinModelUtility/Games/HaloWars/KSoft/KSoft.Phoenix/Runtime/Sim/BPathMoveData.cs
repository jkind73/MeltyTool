
using BPathLevel = System.Byte;

namespace KSoft.Phoenix.Runtime
{
	public sealed class BPathMoveData
		: IO.IEndianStreamSerializable
	{
		internal static readonly FreeListInfo KFreeListInfo = new FreeListInfo(CSaveMarker.PATH_MOVE_DATA)
		{
			MaxCount = 0x4E20,
		};

		public BPath Path { get; private set; } = new BPath();
		public int currentWaypoint;
		public uint pathTime;
		public int linkedPath = TypeExtensions.K_NONE;
		public BPathLevel pathLevel;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(this.Path);
			s.Stream(ref this.currentWaypoint);
			s.Stream(ref this.pathTime);
			BSaveGame.StreamFreeListItemPtr(s, ref this.linkedPath);
			s.Stream(ref this.pathLevel);
		}
		#endregion
	};
}
