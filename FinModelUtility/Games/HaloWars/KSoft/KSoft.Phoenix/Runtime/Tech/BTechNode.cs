
namespace KSoft.Phoenix.Runtime
{
	using BProtoTechStatusStreamer = IO.EnumBinaryStreamer<Phx.BProtoTechStatus>;

	sealed class BTechNode
		: ITechNode
	{
		public float researchPoints;
		public int researchBuilding;
		public Phx.BProtoTechStatus status;
		public bool unique;

		public BTechUniqueNode[] uniqueNodes;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray16(s, ref this.uniqueNodes);
			s.Stream(ref this.researchPoints);
			s.Stream(ref this.researchBuilding);
			s.Stream(ref this.status, BProtoTechStatusStreamer.Instance);
			s.Stream(ref this.unique);
		}
		#endregion

		#region ITechNode Members
		float ITechNode.ResearchPoints
		{
			get { return this.researchPoints; }
			set { this.researchPoints = value; }
		}
		int ITechNode.ResearchBuilding
		{
			get { return this.researchBuilding; }
			set { this.researchBuilding = value; }
		}
		Phx.BProtoTechStatus ITechNode.Status
		{
			get { return this.status; }
			set { this.status = value; }
		}
		bool ITechNode.Unique
		{
			get { return this.unique; }
			set { this.unique = value; }
		}
		#endregion
	};
}