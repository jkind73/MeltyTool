using System.Collections.Generic;

using BVector = System.Numerics.Vector4;
using BCost = System.Single;
using BCueIndex = System.Int32;
using BEntityID = System.Int32;
using BProtoObjectID = System.Int32;
using BTeamID = System.Int32;
using BWaveGravityBall = System.UInt64; // idk, 8 bytes
using BQueuedObject = System.UInt64; // idk, 8 bytes

namespace KSoft.Phoenix.Runtime
{
	class BPowerCovDebris : BPower
	{
		public double nextTickTime;
		public BWaveGravityBall realGravityBall;
		public BVector desiredBallPosition;
		public List<BEntityID> capturedUnits = [];
		public float explodeCooldownLeft;
		public BEntityID[] unitsToPull;
		public BQueuedObject[] queuedPickupObjects;
		public BCost[] costPerTick;
		public float tickLength;
		public BProtoObjectID ballProtoId, lightningProtoId, lightningBeamVisualProtoId,
			debrisProtoId, explodeProtoId, pickupAttachmentProtoId;
		public float audioReactionTimer;
		public uint leaderAnimOrderId;
		public float maxBallSpeedStagnant, maxBallSpeedPulling, explodeTime,
			pullingRange, explosionForceOnDebris, healthToCapture,
			nudgeStrength, initialLateralPullStrength, capturedRadialSpacing,
			capturedSpringStrength, capturedSpringDampening, capturedSpringRestLength,
			capturedMinLateralSpeed, ripAttachmentChancePulling, pickupObjectRate,
			debrisAngularDamping, currentExplosionDamageBank, maxPossibleExplosionDamageBank,
			maxExplosionDamageBankPerCaptured, explosionDamageBankPerTick;
		public uint commandInterval;
		public float minBallDistance, maxBallDistance;
		public int lightningPerTick, maxCapturedObjects;
		public byte nudgeChancePulling, throwPartChancePulling, lightningChancePulling;
		public BCueIndex explodeSound;
		public float minDamageBankPercentToThrow;
		public BTeamID[] revealedTeamIDs;
		BProtoActionTacticUnion unknown0_, unknown1_, unknown2_;
		public bool completedInitialization, throwUnitsOnExplosion;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			var sg = s.Owner as BSaveGame;

			s.Stream(ref this.nextTickTime);
			s.Stream(ref this.realGravityBall);
			s.StreamV(ref this.desiredBallPosition);
			BSaveGame.StreamCollection(s, this.capturedUnits);
			s.Stream(ref this.explodeCooldownLeft);
			BSaveGame.StreamArray(s, ref this.unitsToPull);
			BSaveGame.StreamArray(s, ref this.queuedPickupObjects);
			sg.StreamBCost(s, ref this.costPerTick);
			s.Stream(ref this.tickLength);
			s.Stream(ref this.ballProtoId); s.Stream(ref this.lightningProtoId); s.Stream(ref this.lightningBeamVisualProtoId);
			s.Stream(ref this.debrisProtoId); s.Stream(ref this.explodeProtoId); s.Stream(ref this.pickupAttachmentProtoId);
			s.Stream(ref this.audioReactionTimer);
			s.Stream(ref this.leaderAnimOrderId);

			s.Stream(ref this.maxBallSpeedStagnant); s.Stream(ref this.maxBallSpeedPulling); s.Stream(ref this.explodeTime);
			s.Stream(ref this.pullingRange); s.Stream(ref this.explosionForceOnDebris); s.Stream(ref this.healthToCapture);
			s.Stream(ref this.nudgeStrength); s.Stream(ref this.initialLateralPullStrength); s.Stream(ref this.capturedRadialSpacing);
			s.Stream(ref this.capturedSpringStrength); s.Stream(ref this.capturedSpringDampening); s.Stream(ref this.capturedSpringRestLength);
			s.Stream(ref this.capturedMinLateralSpeed); s.Stream(ref this.ripAttachmentChancePulling); s.Stream(ref this.pickupObjectRate);
			s.Stream(ref this.debrisAngularDamping); s.Stream(ref this.currentExplosionDamageBank); s.Stream(ref this.maxPossibleExplosionDamageBank);
			s.Stream(ref this.maxExplosionDamageBankPerCaptured); s.Stream(ref this.explosionDamageBankPerTick);
			s.Stream(ref this.commandInterval);
			s.Stream(ref this.minBallDistance); s.Stream(ref this.maxBallDistance);
			s.Stream(ref this.lightningPerTick); s.Stream(ref this.maxCapturedObjects);
			s.Stream(ref this.nudgeChancePulling); s.Stream(ref this.throwPartChancePulling); s.Stream(ref this.lightningChancePulling);
			s.Stream(ref this.explodeSound);
			s.Stream(ref this.minDamageBankPercentToThrow);
			BSaveGame.StreamArray(s, ref this.revealedTeamIDs);
			s.Stream(ref this.unknown0_); s.Stream(ref this.unknown1_); s.Stream(ref this.unknown2_);
			s.Stream(ref this.completedInitialization); s.Stream(ref this.throwUnitsOnExplosion);
		}
		#endregion
	};
}
