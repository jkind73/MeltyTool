
using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerTransport
		: BPower
	{
		public BVector pickupLocation;
		public BEntityID[] squadsToTransport;
		public bool gotPickupLocation;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.StreamV(ref this.pickupLocation);
			BSaveGame.StreamArray(s, ref this.squadsToTransport);
			s.Stream(ref this.gotPickupLocation);
		}
		#endregion
	};
}