
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscDisruptionUser
		: BPowerUser
	{
		public int losMode;
		public BProtoObjectID disruptionObjectProtoId;
		public BPowerHelperHudSounds hudSounds = new BPowerHelperHudSounds();

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref this.losMode);
			s.Stream(ref this.disruptionObjectProtoId);
			s.Stream(this.hudSounds);
		}
		#endregion
	};
}