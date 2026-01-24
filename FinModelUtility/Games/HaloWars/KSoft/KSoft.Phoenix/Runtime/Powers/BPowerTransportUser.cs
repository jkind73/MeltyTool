
using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerTransportUser
		: BPowerUser
	{
		public BPowerHelperHudSounds hudSounds = new BPowerHelperHudSounds();
		public BEntityID[] squadsToTransport;
		public BEntityID[] targetedSquads;
		public int losMode;
		public bool gotPickupLocation;
		public BVector pickupLocation;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(this.hudSounds);
			BSaveGame.StreamArray(s, ref this.squadsToTransport);
			BSaveGame.StreamArray(s, ref this.targetedSquads);
			s.Stream(ref this.losMode);
			s.Stream(ref this.gotPickupLocation);
			s.StreamV(ref this.pickupLocation);
		}
		#endregion
	};
}