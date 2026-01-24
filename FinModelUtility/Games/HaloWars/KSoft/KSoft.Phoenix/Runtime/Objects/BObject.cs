#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using BVector = System.Numerics.Vector4;
using BBitVector32 = System.UInt32;
using BPlayerID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	partial class CSaveMarker
	{
		public const ushort
			OBJECT1 = 0x2710
			;
	};

	public struct BObjectAnimationState
		: IO.IEndianStreamSerializable
	{
		public short animType;
		public short tweenToAnimation;
		public float tweenTime, moveSpeed;
		public int forceAnimId;
		public sbyte state;
		public sbyte exitAction;
		public bool
			flagDirty, flagMoving, flagTurning,
			flagReset, flagApplyInstantly, flagLock,
			flagOverrideExitAction
			;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.animType);
			s.Stream(ref this.tweenToAnimation);
			s.Stream(ref this.tweenTime); s.Stream(ref this.moveSpeed);
			s.Stream(ref this.forceAnimId);
			s.Stream(ref this.state);
			s.Stream(ref this.exitAction);
			s.Stream(ref this.flagDirty);	s.Stream(ref this.flagMoving);			s.Stream(ref this.flagTurning);
			s.Stream(ref this.flagReset);	s.Stream(ref this.flagApplyInstantly);	s.Stream(ref this.flagLock);
			s.Stream(ref this.flagOverrideExitAction);
		}
		#endregion
	};

	public sealed class BObject
		: BEntity
	{
		const int K_UV_OFFSETS_SIZE_ = 0x18;
		const int C_MAXIMUM_OBJECT_ATTACHMENTS_ = 0xFA;
		const int C_MAXIMUM_ADDITIONAL_TEXTURES_ = 0xFA;
		const int C_MAXIMUM_HARDPOINTS_ = 0x64;

		public BVector centerOffset, iconColorSize;
		public byte[] uvOffsets = new byte[K_UV_OFFSETS_SIZE_];
		public uint multiframeTextureIndex;
		public int visualVariationIndex;
		public BVisual visual;
		public float animationRate, radius, moveAnimationPosition, highlightIntensity;
		public uint subUpdateNumber;
		public BBitVector32 playerVisibility, doppleBits;
		public int simX, simZ;
		public float losScalar;
		public int lastSimLos;
		public BObjectAttachments[] objectAttachments;
		public BAdditionalTextures[] additionalTextures;
		public BHardpointState[] hardpointState;
		public BObjectAnimationState animationState;
		public uint animationLockEnds;
		public int protoId;
		public BPlayerID colorPlayerId;
		public uint overrideTint,
			overrideFlashInterval, overrideFlashIntervalTimer, overrideFlashDuration,
			lifespanExpiration;
		public float currentAlphaTime, alphaFadeDuration,
			selectionPulseTime, selectionPulsePercent, selectionFlashTime, selectionPulseSpeed,
			lastRealtime;
		public byte aoTintValue, teamSelectionMask;
		public float losRevealTime;

		#region Flags
		public bool
			flagVisibility, flagLos, flagHasLifespan, flagDopples,
			flagIsFading, flagAnimationDisabled, flagIsRevealer, flagDontInterpolate,
			flagBlockLos, flagCloaked, flagCloakDetected, flagGrayMapDopples,
			flagLosMarked, flagUseLosScalar, flagLosDirty, flagAnimationLocked,
			flagUpdateSquadPositionOnAnimationUnlock, flagExistSoundPlaying, flagNoUpdate, flagSensorLockTagged,
			flagNoReveal, flagBuilt, flagBeingCaptured, flagInvulnerable,
			flagVisibleForOwnerOnly, flagVisibleToAll, flagNearLayer, flagIkDisabled,
			flagHasTrackMask, flagLodFading, flagOccluded, flagFadeOnDeath,
			flagObscurable, flagNoRender, flagTurning, flagAppearsBelowDecals,
			flagSkipMotionExtraction, flagOverrideTint, flagMotionCollisionChecked,
			flagIsDopple, flagIsImpactEffect, flagDebugRenderAreaAttackRange, flagDontLockMovementAnimation,
			flagRemainVisible, flagVisibleForTeamOnly, flagDontAutoAttackMe, flagAlwaysAttackReviveUnits,
			flagNoRenderForOwner, flagNoRenderDuringCinematic, flagUseCenterOffset, flagNotDoppleFriendly,
			flagForceVisibilityUpdateNextFrame, flagTurningRight, flagIsUnderCinematicControl, flagNoWorldUpdate;
		#endregion

		public bool isObstruction; // "mpObstructionNode != NULL"

		public bool HasObjectAttachments { get { return this.objectAttachments != null; } }
		public bool HasAdditionalTextures { get { return this.additionalTextures != null; } }

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.StreamV(ref this.centerOffset); s.StreamV(ref this.iconColorSize);
			s.Stream(this.uvOffsets);
			s.Stream(ref this.multiframeTextureIndex);
			s.Stream(ref this.visualVariationIndex);
			BVisualManager.Stream(s, ref this.visual);
			s.Stream(ref this.animationRate); s.Stream(ref this.radius); s.Stream(ref this.moveAnimationPosition); s.Stream(ref this.highlightIntensity);
			s.Stream(ref this.subUpdateNumber);
			s.Stream(ref this.playerVisibility); s.Stream(ref this.doppleBits);
			s.Stream(ref this.simX); s.Stream(ref this.simZ);
			s.Stream(ref this.losScalar);
			s.Stream(ref this.lastSimLos);

			if (s.StreamCond(this, me => me.HasObjectAttachments))
				BSaveGame.StreamArray(s, ref this.objectAttachments, C_MAXIMUM_OBJECT_ATTACHMENTS_);

			if (s.StreamCond(this, me => me.HasAdditionalTextures))
				BSaveGame.StreamArray(s, ref this.additionalTextures, C_MAXIMUM_ADDITIONAL_TEXTURES_);

			BSaveGame.StreamArray(s, ref this.hardpointState, C_MAXIMUM_HARDPOINTS_);
			s.Stream(ref this.animationState);
			s.Stream(ref this.animationLockEnds);
			s.Stream(ref this.protoId);
			s.Stream(ref this.colorPlayerId);
			s.Stream(ref this.overrideTint);
			s.Stream(ref this.overrideFlashInterval); s.Stream(ref this.overrideFlashIntervalTimer); s.Stream(ref this.overrideFlashDuration);
			s.Stream(ref this.lifespanExpiration);
			s.Stream(ref this.currentAlphaTime); s.Stream(ref this.alphaFadeDuration);
			s.Stream(ref this.selectionPulseTime); s.Stream(ref this.selectionPulsePercent); s.Stream(ref this.selectionFlashTime); s.Stream(ref this.selectionPulseSpeed);
			s.Stream(ref this.lastRealtime);
			s.Stream(ref this.aoTintValue);
			s.Stream(ref this.teamSelectionMask);
			s.Stream(ref this.losRevealTime);

			#region Flags
			s.Stream(ref this.flagVisibility); s.Stream(ref this.flagLos); s.Stream(ref this.flagHasLifespan); s.Stream(ref this.flagDopples);
			s.Stream(ref this.flagIsFading); s.Stream(ref this.flagAnimationDisabled); s.Stream(ref this.flagIsRevealer); s.Stream(ref this.flagDontInterpolate);
			s.Stream(ref this.flagBlockLos); s.Stream(ref this.flagCloaked); s.Stream(ref this.flagCloakDetected); s.Stream(ref this.flagGrayMapDopples);
			s.Stream(ref this.flagLosMarked); s.Stream(ref this.flagUseLosScalar); s.Stream(ref this.flagLosDirty); s.Stream(ref this.flagAnimationLocked);
			s.Stream(ref this.flagUpdateSquadPositionOnAnimationUnlock); s.Stream(ref this.flagExistSoundPlaying); s.Stream(ref this.flagNoUpdate); s.Stream(ref this.flagSensorLockTagged);
			s.Stream(ref this.flagNoReveal); s.Stream(ref this.flagBuilt); s.Stream(ref this.flagBeingCaptured); s.Stream(ref this.flagInvulnerable);
			s.Stream(ref this.flagVisibleForOwnerOnly); s.Stream(ref this.flagVisibleToAll); s.Stream(ref this.flagNearLayer); s.Stream(ref this.flagIkDisabled);
			s.Stream(ref this.flagHasTrackMask); s.Stream(ref this.flagLodFading); s.Stream(ref this.flagOccluded); s.Stream(ref this.flagFadeOnDeath);
			s.Stream(ref this.flagObscurable); s.Stream(ref this.flagNoRender); s.Stream(ref this.flagTurning); s.Stream(ref this.flagAppearsBelowDecals);
			s.Stream(ref this.flagSkipMotionExtraction); s.Stream(ref this.flagOverrideTint); s.Stream(ref this.flagMotionCollisionChecked); s.Stream(ref this.flagIsDopple);
			s.Stream(ref this.flagIsImpactEffect); s.Stream(ref this.flagDebugRenderAreaAttackRange); s.Stream(ref this.flagDontLockMovementAnimation); s.Stream(ref this.flagRemainVisible);
			s.Stream(ref this.flagVisibleForTeamOnly); s.Stream(ref this.flagDontAutoAttackMe); s.Stream(ref this.flagAlwaysAttackReviveUnits); s.Stream(ref this.flagNoRenderForOwner);
			s.Stream(ref this.flagNoRenderDuringCinematic); s.Stream(ref this.flagUseCenterOffset); s.Stream(ref this.flagNotDoppleFriendly); s.Stream(ref this.flagForceVisibilityUpdateNextFrame);
			s.Stream(ref this.flagTurningRight); s.Stream(ref this.flagIsUnderCinematicControl); s.Stream(ref this.flagNoWorldUpdate);
			#endregion

			s.Stream(ref this.isObstruction);

			Contract.Assert(false);// mpPhysicsObject

			s.StreamSignature(CSaveMarker.OBJECT1);
		}
		#endregion
	};
}