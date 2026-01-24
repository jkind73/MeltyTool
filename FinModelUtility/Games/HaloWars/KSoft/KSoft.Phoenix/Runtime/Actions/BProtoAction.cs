
namespace KSoft.Phoenix.Runtime
{
	sealed class BProtoAction
		: IO.IEndianStreamSerializable
	{
		public const int K_MAX_COUNT = 0xC8;

		public float workRate, workRateVariance;
		public int animType;
		public /*float*/uint damagePerAttack;
		public int maxNumAttacksPerAnim;
		public float strafingTurnRate, joinBoardTime;
		public bool disabled;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.workRate); s.Stream(ref this.workRateVariance);
			s.Stream(ref this.animType);
			s.Stream(ref this.damagePerAttack);
			s.Stream(ref this.maxNumAttacksPerAnim);
			s.Stream(ref this.strafingTurnRate); s.Stream(ref this.joinBoardTime);
			s.Stream(ref this.disabled);
			s.StreamSignature(CSaveMarker.PROTO_ACTION);
		}
		#endregion
	};
}