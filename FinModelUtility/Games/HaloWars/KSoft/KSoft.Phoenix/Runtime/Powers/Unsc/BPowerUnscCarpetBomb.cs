using System.Collections.Generic;

using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscCarpetBomb
		: BPower
	{
		public sealed class BBombExplodeInfo
			: IO.IEndianStreamSerializable
		{
			public ulong unknown0, unknown8, unknown10, unknown18;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.unknown0); s.Stream(ref this.unknown8); s.Stream(ref this.unknown10); s.Stream(ref this.unknown18);
			}
			#endregion
		};

		public BBombExplodeInfo[] bombExplodeInfos;
		public List<BEntityID> nudgedUnits = [];
		public BVector startLocation, startDirection, rightVector;
		public sbyte state;
		public bool gotStartLocation, gotStartDirection;
		public double tickLength, nextBombTime, lastBombTime;
		public uint numBombClustersDropped;
		public BProtoObjectID projectileProtoId, impactProtoId, explosionProtoId, bomberProtoId;
		public float initialDelay, fuseTime;
		public uint maxBombs;
		public float maxBombOffset, bombSpacing, lengthMultiplier,
			wedgeLengthMultiplier, wedgeMinOffset, nudgeMultiplier;
		public BPowerHelperBomber bomberData = new BPowerHelperBomber();
		public sbyte losMode;
		public bool reactionPlayed;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			BSaveGame.StreamArray(s, ref this.bombExplodeInfos);
			BSaveGame.StreamCollection(s, this.nudgedUnits);
			s.StreamV(ref this.startLocation); s.StreamV(ref this.startDirection); s.StreamV(ref this.rightVector);
			s.Stream(ref this.state);
			s.Stream(ref this.gotStartLocation); s.Stream(ref this.gotStartDirection);
			s.Stream(ref this.tickLength); s.Stream(ref this.nextBombTime); s.Stream(ref this.lastBombTime);
			s.Stream(ref this.numBombClustersDropped);
			s.Stream(ref this.projectileProtoId); s.Stream(ref this.impactProtoId); s.Stream(ref this.explosionProtoId); s.Stream(ref this.bomberProtoId);
			s.Stream(ref this.initialDelay); s.Stream(ref this.fuseTime);
			s.Stream(ref this.maxBombs);
			s.Stream(ref this.maxBombOffset); s.Stream(ref this.bombSpacing); s.Stream(ref this.lengthMultiplier);
			s.Stream(ref this.wedgeLengthMultiplier); s.Stream(ref this.wedgeMinOffset); s.Stream(ref this.nudgeMultiplier);
			s.Stream(this.bomberData);
			s.Stream(ref this.losMode);
			s.Stream(ref this.reactionPlayed);
		}
		#endregion
	};
}