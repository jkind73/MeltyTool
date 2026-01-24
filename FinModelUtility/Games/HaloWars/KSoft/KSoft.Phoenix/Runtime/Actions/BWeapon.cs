
namespace KSoft.Phoenix.Runtime
{
	// WTF: Some player's cov_bldg_turret_01's ProtoAction's DamagePerAttack
	// and some other object's (same player) Weapon's DamagePerSecond where
	// being read and somehow getting at least .125 to .25 added to them 
	// So I'm reading them as raw instead

	sealed class BWeapon
		: IO.IEndianStreamSerializable
	{
		public const int K_MAX_COUNT = 0xC8;

		public /*float*/uint damagePerSecond;
		public float doTrate, doTduration, 
			maxRange, minRange, aoeRadius, 
			aoePrimaryTargetFactor, aoeDistanceFactor, aoeDamageFactor, 
			accuracy, movingAccuracy, maxDeviation,
			movingMaxDeviation, accuracyDistanceFactor, accuracyDeviationFactor,
			maxVelocityLead, maxDamagePerRam, reflectDamageFactor, 
			airBurstSpan;
		public int projectileObjectId, impactEffectProtoId;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.damagePerSecond); s.Stream(ref this.doTrate); s.Stream(ref this.doTduration); 
			s.Stream(ref this.maxRange); s.Stream(ref this.minRange); s.Stream(ref this.aoeRadius); 
			s.Stream(ref this.aoePrimaryTargetFactor); s.Stream(ref this.aoeDistanceFactor); s.Stream(ref this.aoeDamageFactor); 
			s.Stream(ref this.accuracy); s.Stream(ref this.movingAccuracy); s.Stream(ref this.maxDeviation);
			s.Stream(ref this.movingMaxDeviation); s.Stream(ref this.accuracyDistanceFactor); s.Stream(ref this.accuracyDeviationFactor);
			s.Stream(ref this.maxVelocityLead); s.Stream(ref this.maxDamagePerRam); s.Stream(ref this.reflectDamageFactor); 
			s.Stream(ref this.airBurstSpan);
			s.Stream(ref this.projectileObjectId); s.Stream(ref this.impactEffectProtoId);
			s.StreamSignature(CSaveMarker.WEAPON);
		}
		#endregion
	};
}