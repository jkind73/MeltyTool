
namespace KSoft.Phoenix.Runtime
{
	sealed class BSavePlayer
		: IO.IEndianStreamSerializable
	{
		public string name;
		public string displayName;
		public int mpid, scenarioId, civId, teamId, leaderId;
		public ushort difficulty; // BHalfFloat
		public sbyte playerType;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamPascalString32(ref this.name);
			s.StreamPascalWideString32(ref this.displayName);
			s.Stream(ref this.mpid);
			s.Stream(ref this.scenarioId);
			s.Stream(ref this.civId);
			s.Stream(ref this.teamId);
			s.Stream(ref this.leaderId);
			s.Stream(ref this.difficulty);
			s.Stream(ref this.playerType);
			s.StreamSignature(CSaveMarker.SETUP_PLAYER);
		}
		#endregion
	};
}