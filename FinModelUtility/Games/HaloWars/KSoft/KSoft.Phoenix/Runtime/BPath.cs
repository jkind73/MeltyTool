
using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.Runtime
{
	partial class CSaveMarker
	{
		public const ushort
			C_SAVE_MARKER_PATH1 = 0x2710
			;
	};

	public sealed class BPath
		: IO.IEndianStreamSerializable
	{
		const int C_MAXIMUM_WAYPOINTS_ = 0x2710;

		public BVector[] waypoints;
		public byte flags;
		public float pathLength;
		public uint creationTime;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamVectorArray16(s, ref this.waypoints, C_MAXIMUM_WAYPOINTS_);
			s.Stream(ref this.flags);
			s.Stream(ref this.pathLength);
			s.Stream(ref this.creationTime);

			s.StreamSignature(CSaveMarker.C_SAVE_MARKER_PATH1);
		}
		#endregion
	};
}