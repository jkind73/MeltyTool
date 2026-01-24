
namespace KSoft.Phoenix.Runtime
{
	sealed class BConfigSettings
		: IO.IEndianStreamSerializable
	{
		const uint K_VERSION_ = 0;

		public bool 
			noFogMask,			aiDisable,			aiShadow,			noVismap,					noRandomPlayerPlacement, 
			disableOneBuilding,	buildingQueue,		useTestLeaders,		enableFlight,				noBirthAnims, 
			veterancy,			trueLos,			noDestruction,		coopSharedResources,		maxProjectileHeightForDecal,
			enableSubbreakage,	enableThrowPart,	allowAnimIsDirty,	noVictoryCondition,			aiAutoDifficulty,
			demo,				asyncWorldUpdate,	enableHintSystem,	percentFadeTimeCorpseSink,	corpseSinkSpeed,
			corpseMinScale,		blockOutsideBounds,	aiNoAttack,			passThroughOwnVehicles,		enableCapturePointResourceSharing,
			noUpdatePathingQuad,slaveUnitPosition,	turning,			humanAttackMove,			moreNewMovement3,
			overrideGroundIk,	driveWarthog,		enableCorpses,		disablePathingLimits,		disableVelocityMatchingBySquadType,
			activeAbilities,	alphaTest,			noDamage,			ignoreAllPlatoonmates,		classicPlatoonGrouping,
			noShieldDamage,		enableSubUpdating,	mpSubUpdating,		alternateSubUpdating,		dynamicSubUpdateTime,
			decoupledUpdate;

		public float
			platoonRadius,	projectionTime,	overrideGroundIkRange,	overrideGroundIkTiltFactor,	gameSpeed;

		#region IEndianStreamSerializable Members
		void Read(IO.EndianReader s, out float value)
		{
			if (s.ReadBoolean())
				value = s.ReadSingle();
			else
				value = 0f;
		}
		void Write(IO.EndianWriter s, float value)
		{
			bool notZero = value != 0f;
			s.Write(notZero);

			if (notZero) s.Write(value);
		}
		void ReadFloats(IO.EndianReader s)
		{
			this.Read(s, out this.platoonRadius);
			this.Read(s, out this.projectionTime);
			this.Read(s, out this.overrideGroundIkRange);
			this.Read(s, out this.overrideGroundIkTiltFactor);
			this.Read(s, out this.gameSpeed);
		}
		void WriteFloats(IO.EndianWriter s)
		{
			this.Write(s, this.platoonRadius);
			this.Write(s, this.projectionTime);
			this.Write(s, this.overrideGroundIkRange);
			this.Write(s, this.overrideGroundIkTiltFactor);
			this.Write(s, this.gameSpeed);
		}

		public void Serialize(IO.EndianStream s)
		{
			s.StreamVersion(K_VERSION_);

			s.Stream(ref this.noFogMask);			s.Stream(ref this.aiDisable);			s.Stream(ref this.aiShadow);			s.Stream(ref this.noVismap);					s.Stream(ref this.noRandomPlayerPlacement);
			s.Stream(ref this.disableOneBuilding);	s.Stream(ref this.buildingQueue);		s.Stream(ref this.useTestLeaders);	s.Stream(ref this.enableFlight);				s.Stream(ref this.noBirthAnims);
			s.Stream(ref this.veterancy);			s.Stream(ref this.trueLos);				s.Stream(ref this.noDestruction);	s.Stream(ref this.coopSharedResources);		s.Stream(ref this.maxProjectileHeightForDecal);
			s.Stream(ref this.enableSubbreakage);	s.Stream(ref this.enableThrowPart);		s.Stream(ref this.allowAnimIsDirty);	s.Stream(ref this.noVictoryCondition);		s.Stream(ref this.aiAutoDifficulty);
			s.Stream(ref this.demo);					s.Stream(ref this.asyncWorldUpdate);		s.Stream(ref this.enableHintSystem);	s.Stream(ref this.percentFadeTimeCorpseSink);s.Stream(ref this.corpseSinkSpeed);
			s.Stream(ref this.corpseMinScale);		s.Stream(ref this.blockOutsideBounds);	s.Stream(ref this.aiNoAttack);		s.Stream(ref this.passThroughOwnVehicles);	s.Stream(ref this.enableCapturePointResourceSharing);
			s.Stream(ref this.noUpdatePathingQuad);	s.Stream(ref this.slaveUnitPosition);	s.Stream(ref this.turning);			s.Stream(ref this.humanAttackMove);			s.Stream(ref this.moreNewMovement3);
			s.Stream(ref this.overrideGroundIk);		s.Stream(ref this.driveWarthog);			s.Stream(ref this.enableCorpses);	s.Stream(ref this.disablePathingLimits);		s.Stream(ref this.disableVelocityMatchingBySquadType);
			s.Stream(ref this.activeAbilities);		s.Stream(ref this.alphaTest);			s.Stream(ref this.noDamage);			s.Stream(ref this.ignoreAllPlatoonmates);	s.Stream(ref this.classicPlatoonGrouping);
			s.Stream(ref this.noShieldDamage);		s.Stream(ref this.enableSubUpdating);	s.Stream(ref this.mpSubUpdating);	s.Stream(ref this.alternateSubUpdating);		s.Stream(ref this.dynamicSubUpdateTime);
			s.Stream(ref this.decoupledUpdate);

			s.StreamMethods(this.ReadFloats, this.WriteFloats);
		}
		#endregion
	};
}