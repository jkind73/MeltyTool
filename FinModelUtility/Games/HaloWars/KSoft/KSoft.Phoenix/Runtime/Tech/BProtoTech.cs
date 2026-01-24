
namespace KSoft.Phoenix.Runtime
{
	sealed class BProtoTech
		: BProtoBuildableObject
	{
		public float ResearchPoints { get { return this.buildPoints; } }

		public bool ownStaticData, unobtainable, unique,
			shadow, orPrereqs, perpetual,
			noSound, instant;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			var sg = s.Owner as BSaveGame;

			sg.StreamBCost(s, ref this.cost);
			s.Stream(ref this.buildPoints);
			s.Stream(ref this.ownStaticData); s.Stream(ref this.unobtainable); s.Stream(ref this.unique);
			s.Stream(ref this.shadow); s.Stream(ref this.orPrereqs); s.Stream(ref this.perpetual);
			s.Stream(ref this.forbid);
			s.Stream(ref this.noSound); s.Stream(ref this.instant);
		}
		#endregion
	};
}