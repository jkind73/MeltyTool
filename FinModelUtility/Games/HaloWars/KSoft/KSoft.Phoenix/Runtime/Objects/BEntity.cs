
using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;
using BEntityRef = System.UInt64; // idk, 8 bytes
using BPlayerID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	partial class CSaveMarker
	{
		public const ushort
			ENTITY1 = 0x2710
			;
	};

	public abstract class BEntity
		: IO.IEndianStreamSerializable
	{
		const int C_MAXIMUM_ENTITY_REFS_ = 0x3E8;

		public BVector position, up, forward, velocity;
		public ActionListEntry[] actions;
		public BEntityRef[] entityRefs;
		public BEntityID id;
		public BPlayerID playerId;
		public float yDisplacement,
			obstructionRadiusX, obstructionRadiusY, obstructionRadiusZ;
		#region Flags
		public bool flagCollidable	// 0x9C, 7
			, flagMoving			// 0x9C, 5
			, flagDestroy			// 0x9C, 6
			, flagFirstUpdate		// 0x9C, 4
			, flagTiesToGround		// 0x9C, 3
			, flagUseMaxHeight		// 0x9C, 2
			, flagPhysicsControl	// 0x9C, 1
			, flagRotateObstruction	// 0x9C, 0
			, flagFlying			// 0x9D, 7
			, flagValid				// 0x9D, 6
			, flagNonMobile			// 0x9D, 5
			, flagLockedDown		// 0x9D, 4
			, flagEntityRefsLocked	// 0x9D, 3
			, flagFlyingHeightFixup	// 0x9D, 2
			, flagGarrisoned		// 0x9D, 1
			, flagPassiveGarrisoned	// 0x9D, 0
			, flagMoved				// 0x9E, 6
			, flagTeleported		// 0x9E, 5
			, flagInSniper			// 0x9E, 4
			, flagIsBuilt			// 0x9E, 3
			, flagHasSounds			// 0x9E, 2
			, flagHitched			// 0x9E, 1
			, flagSprinting			// 0x9E, 0
			, flagRecovering		// 0x9E, 7
			, flagInCover			// 0x9F, 6
			, flagSelectable		// 0x9F, 5
			, flagUngarrisonValid	// 0x9F, 4
			, flagGarrisonValid		// 0x9F, 3
			, flagIsPhysicsReplacement	// 0x9F, 2
			, flagIsDoneBuilding	// 0x9F, 1
			;
		#endregion

		public bool HasRefs { get { return this.entityRefs != null; } }

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			s.StreamV(ref this.position); s.StreamV(ref this.up); s.StreamV(ref this.forward); s.StreamV(ref this.velocity);
			BActionManager.StreamActionList(s, ref this.actions);

			bool hasRefs = s.IsReading ? false : this.HasRefs;
			s.Stream(ref hasRefs);
			if (hasRefs)
				BSaveGame.StreamArray16(s, ref this.entityRefs, C_MAXIMUM_ENTITY_REFS_);

			s.Stream(ref this.id);
			s.Stream(ref this.playerId);
			s.Stream(ref this.yDisplacement);
			s.Stream(ref this.obstructionRadiusX); s.Stream(ref this.obstructionRadiusY); s.Stream(ref this.obstructionRadiusZ);

			#region Flags
			s.Stream(ref this.flagCollidable);		s.Stream(ref this.flagMoving);			s.Stream(ref this.flagDestroy);
			s.Stream(ref this.flagFirstUpdate);		s.Stream(ref this.flagTiesToGround);		s.Stream(ref this.flagUseMaxHeight);
			s.Stream(ref this.flagPhysicsControl);	s.Stream(ref this.flagRotateObstruction);s.Stream(ref this.flagFlying);
			s.Stream(ref this.flagValid);			s.Stream(ref this.flagNonMobile);		s.Stream(ref this.flagLockedDown);
			s.Stream(ref this.flagEntityRefsLocked);	s.Stream(ref this.flagFlyingHeightFixup);s.Stream(ref this.flagGarrisoned);
			s.Stream(ref this.flagPassiveGarrisoned);s.Stream(ref this.flagMoved);			s.Stream(ref this.flagTeleported);
			s.Stream(ref this.flagInSniper);			s.Stream(ref this.flagIsBuilt);			s.Stream(ref this.flagHasSounds);
			s.Stream(ref this.flagHitched);			s.Stream(ref this.flagSprinting);		s.Stream(ref this.flagRecovering);
			s.Stream(ref this.flagInCover);			s.Stream(ref this.flagSelectable);		s.Stream(ref this.flagUngarrisonValid);
			s.Stream(ref this.flagGarrisonValid);	s.Stream(ref this.flagIsPhysicsReplacement);	s.Stream(ref this.flagIsDoneBuilding);
			#endregion

			s.StreamSignature(CSaveMarker.ENTITY1);
		}
		#endregion
	};
}