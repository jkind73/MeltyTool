
using BProtoSquadID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	partial class CSaveMarker
	{
		public const ushort
			PROTO_SQUAD1 = 0x2710
			;
	};

	sealed class BProtoSquad
		: BProtoObjectBase
	{
		public BProtoSquadID protoId;
		public float maxHp, maxSp, maxAmmo; // SP=shield points?
		public int level, techLevel, displayNameIndex,
			circleMenuIconId, altCircleMenuIconId, hpBar;
		public bool oneTimeSpawnUsed, kbAware;
		public bool hasOverrideNodes;
		public BProtoSquadNodeOverride[] overrideNodes;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);
			var sg = s.Owner as BSaveGame;

			s.Stream(ref this.protoId);
			s.Stream(ref this.buildPoints);
			sg.StreamBCost(s, ref this.cost);
			s.Stream(ref this.maxHp); s.Stream(ref this.maxSp); s.Stream(ref this.maxAmmo);
			s.Stream(ref this.level); s.Stream(ref this.techLevel); s.Stream(ref this.displayNameIndex);
			s.Stream(ref this.circleMenuIconId); s.Stream(ref this.altCircleMenuIconId); s.Stream(ref this.hpBar);
			s.Stream(ref this.available); s.Stream(ref this.forbid);
			s.Stream(ref this.oneTimeSpawnUsed); s.Stream(ref this.kbAware);
			s.Stream(ref this.hasOverrideNodes);
			if (this.hasOverrideNodes) BSaveGame.StreamArray(s, ref this.overrideNodes);
			s.StreamSignature(CSaveMarker.PROTO_SQUAD1);
		}
		#endregion
	};
}