
using BVector = System.Numerics.Vector4;
using BCost = System.Single;
using BEntityID = System.Int32;
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerCovGlassing
		: BPower
	{
		const int C_MAXIMUM_WAYPOINTS_ = 0xC8;
		const int C_MAXIMUM_BEAM_PATH_LENGTH_ = 0xC8;

		public BVector[] waypoints;
		public BEntityID beamId, airImpactObjectId;
		public double nextDamageTime;
		public BVector desiredBeamPosition;
		public BVector[] beamPath;
		public int[] revealedTeamIDs; // BTeamID
		public BCost[] costPerTick;
		public BProtoObjectID projectile;
		public float tickLength, minBeamDistance, maxBeamDistance;
		public uint commandInterval;
		public float maxBeamSpeed;
		public int losMode;
		public bool usePath;
		public float audioReactionTimer;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);
			var sg = s.Owner as BSaveGame;

			BSaveGame.StreamVectorArray(s, ref this.waypoints, C_MAXIMUM_WAYPOINTS_);
			s.Stream(ref this.beamId); s.Stream(ref this.airImpactObjectId);
			s.Stream(ref this.nextDamageTime);
			s.StreamV(ref this.desiredBeamPosition);
			BSaveGame.StreamVectorArray(s, ref this.beamPath, C_MAXIMUM_BEAM_PATH_LENGTH_);
			BSaveGame.StreamArray(s, ref this.revealedTeamIDs);
			sg.StreamBCost(s, ref this.costPerTick);
			s.Stream(ref this.projectile);
			s.Stream(ref this.tickLength); s.Stream(ref this.minBeamDistance); s.Stream(ref this.maxBeamDistance);
			s.Stream(ref this.commandInterval);
			s.Stream(ref this.maxBeamSpeed);
			s.Stream(ref this.losMode);
			s.Stream(ref this.usePath);
			s.Stream(ref this.audioReactionTimer);
		}
		#endregion
	};
}