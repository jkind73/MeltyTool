
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerHelperBomber
		: IO.IEndianStreamSerializable
	{
		public BEntityID bomberId;
		public float bomberFlyinDistance, bomberFlyinHeight, bomberBombHeight,
			bomberSpeed, bombTime, flyoutTime, additionalHeight;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.bomberId);
			s.Stream(ref this.bomberFlyinDistance); s.Stream(ref this.bomberFlyinHeight); s.Stream(ref this.bomberBombHeight);
			s.Stream(ref this.bomberSpeed); s.Stream(ref this.bombTime); s.Stream(ref this.flyoutTime); s.Stream(ref this.additionalHeight);
		}
		#endregion
	};
}