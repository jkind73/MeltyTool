
using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerCovGlassingUser
		: BPowerUser
	{
		public BVector inputDir, lastUpdatePos;
		public BEntityID realBeamId, fakeBeamId, airImpactObjectId;
		public uint timestampNextCommand, commandInterval;
		public float minBeamDistance, maxBeamDistance, maxBeamSpeed;
		public int losMode;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.StreamV(ref this.inputDir); s.StreamV(ref this.lastUpdatePos);
			s.Stream(ref this.realBeamId); s.Stream(ref this.fakeBeamId); s.Stream(ref this.airImpactObjectId);
			s.Stream(ref this.timestampNextCommand); s.Stream(ref this.commandInterval);
			s.Stream(ref this.minBeamDistance); s.Stream(ref this.maxBeamDistance); s.Stream(ref this.maxBeamSpeed);
			s.Stream(ref this.losMode);
		}
		#endregion
	};
}