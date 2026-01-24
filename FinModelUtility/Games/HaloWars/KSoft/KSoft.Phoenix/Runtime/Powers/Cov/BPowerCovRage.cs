
using BVector = System.Numerics.Vector4;
using BCost = System.Single;
using BCueIndex = System.Int32;
using BEntityID = System.Int32;
using BProtoObjectID = System.Int32;
using BObjectTypeID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerCovRage
		: BPower
	{
		public sealed class BInterpTable
			: IO.IEndianStreamSerializable
		{
			public float[] keys;
			public uint[] values; // Type

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				BSaveGame.StreamArray16(s, ref this.keys);
				BSaveGame.StreamArray16(s, ref this.values);
			}
			#endregion
		};

		public sealed class BCameraEffectData
			: IO.IEndianStreamSerializable
		{
			public string name;
			public BInterpTable colorTransformRTable = new BInterpTable(), colorTransformGTable = new BInterpTable(), 
				colorTransformBTable = new BInterpTable();
			public BInterpTable colorTransformFactorTable = new BInterpTable(), 
				blurFactorTable = new BInterpTable(), // same data gets written 3x :s
				fovTable = new BInterpTable(), zoomTable = new BInterpTable(), yawTable = new BInterpTable(),
				pitchTable = new BInterpTable();
			public bool radialBlur, use3DPosition, modeCameraEffect,
				userHoverPointAs3DPosition;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.StreamPascalString32(ref this.name);
				s.Stream(this.colorTransformRTable); s.Stream(this.colorTransformGTable); s.Stream(this.colorTransformBTable);
				s.Stream(this.colorTransformFactorTable);
				s.Stream(this.blurFactorTable); s.Stream(this.blurFactorTable); s.Stream(this.blurFactorTable); // yes, 3x
				s.Stream(this.fovTable); s.Stream(this.zoomTable); s.Stream(this.yawTable);
				s.Stream(this.pitchTable);
				s.Stream(ref this.radialBlur); s.Stream(ref this.use3DPosition); s.Stream(ref this.modeCameraEffect);
				s.Stream(ref this.userHoverPointAs3DPosition);
			}
			#endregion
		};

		public double nextTickTime;
		public BEntityID targettedSquad;
		public BVector lastDirectionInput, teleportDestination, positionInput;
		public float timeUntilTeleport, timeUntilRetarget;
		public BCueIndex attackSound;
		public BParametricSplineCurve jumpSplineCurve = new BParametricSplineCurve();
		public BCameraEffectData cameraEffectData = new BCameraEffectData();
		public BCost[] costPerTick, costPerTickAttacking, costPerJump;
		public float tickLength, damageMultiplier, damageTakenMultiplier, 
			speedMultiplier, nudgeMultiplier, scanRadius;
		public BProtoObjectID projectileObject, handAttachObject, teleportAttachObject;
		public float audioReactionTimer, teleportTime,
			teleportLateralDistance, teleportJumpDistance, timeBetweenRetarget, 
			motionBlurAmount, motionBlurDistance, motionBlurTime,
			distanceVsAngleWeight, healPerKillCombatValue, auraRadius, auraDamageBonus;
		public BProtoObjectID auraAttachObjectSmall, auraAttachObjectMedium, auraAttachObjectLarge, 
			healAttachObject;
		public BEntityID[] squadsInAura;
		public BObjectTypeID filterTypeId;
		public bool completedInitialization, hasSuccessfullyAttacked, usePather;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);
			var sg = s.Owner as BSaveGame;

			s.Stream(ref this.nextTickTime);
			s.Stream(ref this.targettedSquad);
			s.StreamV(ref this.lastDirectionInput); s.StreamV(ref this.teleportDestination); s.StreamV(ref this.positionInput);
			s.Stream(ref this.timeUntilTeleport); s.Stream(ref this.timeUntilRetarget);
			s.Stream(ref this.attackSound);
			s.Stream(this.jumpSplineCurve);
			s.Stream(this.cameraEffectData);
			sg.StreamBCost(s, ref this.costPerTick); sg.StreamBCost(s, ref this.costPerTickAttacking); sg.StreamBCost(s, ref this.costPerJump);
			s.Stream(ref this.tickLength); s.Stream(ref this.damageMultiplier); s.Stream(ref this.damageTakenMultiplier);
			s.Stream(ref this.speedMultiplier); s.Stream(ref this.nudgeMultiplier); s.Stream(ref this.scanRadius);
			s.Stream(ref this.projectileObject); s.Stream(ref this.handAttachObject); s.Stream(ref this.teleportAttachObject);
			s.Stream(ref this.audioReactionTimer); s.Stream(ref this.teleportTime);
			s.Stream(ref this.teleportLateralDistance); s.Stream(ref this.teleportJumpDistance); s.Stream(ref this.timeBetweenRetarget);
			s.Stream(ref this.motionBlurAmount); s.Stream(ref this.motionBlurDistance); s.Stream(ref this.motionBlurTime);
			s.Stream(ref this.distanceVsAngleWeight); s.Stream(ref this.healPerKillCombatValue); s.Stream(ref this.auraRadius); s.Stream(ref this.auraDamageBonus);
			s.Stream(ref this.auraAttachObjectSmall); s.Stream(ref this.auraAttachObjectMedium); s.Stream(ref this.auraAttachObjectLarge);
			s.Stream(ref this.healAttachObject);
			BSaveGame.StreamArray(s, ref this.squadsInAura);
			s.Stream(ref this.filterTypeId);
			s.Stream(ref this.completedInitialization); s.Stream(ref this.hasSuccessfullyAttacked); s.Stream(ref this.usePather);
		}
		#endregion
	};
}