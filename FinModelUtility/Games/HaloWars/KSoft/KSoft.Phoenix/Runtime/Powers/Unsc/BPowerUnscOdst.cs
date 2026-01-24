
using BProtoObjectID = System.Int32;
using BAIMissionID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscOdst
		: BPower
	{
		public struct BodstDrop
			: IO.IEndianStreamSerializable
		{
			// A BVector field?
			public ulong unknown0, unknown8;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.unknown0); s.Stream(ref this.unknown8);
			}
			#endregion
		};

		public float squadSpawnDelay;
		public BodstDrop[] activeDrops;
		public BProtoObjectID projectileProtoId, odstProtoSquadId;
		public BAIMissionID addToMissionId;
		public bool readyForShutdown;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref this.squadSpawnDelay);
			BSaveGame.StreamArray(s, ref this.activeDrops);
			s.Stream(ref this.projectileProtoId); s.Stream(ref this.odstProtoSquadId);
			s.Stream(ref this.addToMissionId);
			s.Stream(ref this.readyForShutdown);
		}
		#endregion
	};
}