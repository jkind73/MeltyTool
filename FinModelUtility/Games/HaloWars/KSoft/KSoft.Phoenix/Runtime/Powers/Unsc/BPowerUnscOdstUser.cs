
using BVector = System.Numerics.Vector4;
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscOdstUser
		: BPowerUser
	{
		public string helpString;
		public BPowerHelperHudSounds hudSounds = new BPowerHelperHudSounds();
		public int losMode;
		public BProtoObjectID odstProtoSquadId, odstProtoObjectId;
		public int canFire;
		public BVector validDropLocation;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.StreamPascalWideString32(ref this.helpString);
			s.Stream(this.hudSounds);
			s.Stream(ref this.losMode);
			s.Stream(ref this.odstProtoSquadId); s.Stream(ref this.odstProtoObjectId);
			s.Stream(ref this.canFire);
			s.StreamV(ref this.validDropLocation);
		}
		#endregion
	};
}