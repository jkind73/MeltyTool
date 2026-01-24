using System.Collections.Generic;

using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;
using BPlayerID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	using BPowerTypeStreamer = IO.EnumBinaryStreamer<Phx.BPowerType, uint>;

	sealed class BUser
		: IO.IEndianStreamSerializable
	{
		public struct HudItemEnabledStates
			: IO.IEndianStreamSerializable
		{
	  public bool minimap, resources, time,
				powerStatus, units, dpadHelp,
				buttonHelp, reticle, score,
				unitStats, circleMenuExtraInfo;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.minimap); s.Stream(ref this.resources); s.Stream(ref this.time);
				s.Stream(ref this.powerStatus); s.Stream(ref this.units); s.Stream(ref this.dpadHelp);
				s.Stream(ref this.buttonHelp); s.Stream(ref this.reticle); s.Stream(ref this.score);
				s.Stream(ref this.unitStats); s.Stream(ref this.circleMenuExtraInfo);
			}
			#endregion
		};

		public sealed class BObjectiveArrow
			: IO.IEndianStreamSerializable
		{
			public bool haveArrow;

			public BVector origin, target;
			public float offset;
			public BEntityID objectId, locationObjectId;
			public BPlayerID playerId;
			public bool flagVisible, flagUseTarget, flagTargetDirty,
				flagForceTargetVisible;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.haveArrow);
				if (!this.haveArrow) return;

				s.StreamV(ref this.origin); s.StreamV(ref this.target);
				s.Stream(ref this.offset);
				s.Stream(ref this.objectId); s.Stream(ref this.locationObjectId);
				s.Stream(ref this.playerId);
				s.Stream(ref this.flagVisible); s.Stream(ref this.flagUseTarget); s.Stream(ref this.flagTargetDirty);
				s.Stream(ref this.flagForceTargetVisible);
				s.StreamSignature(CSaveMarker.ARROW);
			}
			#endregion
		};

		public int userMode, subMode;
		#region UserMode==16
		public float uiPowerRadius;
		public int uiProtoPowerId;
		public float uiModeRestoreCameraZoomMin, uiModeRestoreCameraZoomMax, uiModeRestoreCameraZoom,
			uiModeRestoreCameraPitchMin, uiModeRestoreCameraPitchMax, uiModeRestoreCameraPitch,
			uiModeRestoreCameraYaw;
		public bool flagUiModeRestoreCameraZoomMin, flagUiModeRestoreCameraZoomMax, flagUiModeRestoreCameraZoom,
			flagUiModeRestoreCameraPitchMin, flagUiModeRestoreCameraPitchMax, flagUiModeRestoreCameraPitch,
			flagUiModeRestoreCameraYaw,
			flagCameraScrollEnabled, flagCameraYawEnabled, flagCameraZoomEnabled, flagCameraAutoZoomInstantEnabled,
			flagCameraAutoZoomEnabled, flagRestoreCameraEnableUserScroll, flagRestoreCameraEnableUserYaw, flagRestoreCameraEnableUserZoom,
			flagRestoreCameraEnableAutoZoomInstant, flagRestoreCameraEnableAutoZoom;
		public Phx.BPowerType powerType;
		public BPowerUser powerUser;
		#endregion
		public BEntityID[] selectionList;

		public float cameraZoomMin, cameraZoomMax,
			cameraPitchMin, cameraPitchMax,
			cameraPitch, cameraYaw,
			cameraZoom, cameraFov;
		public BVector hoverPoint, cameraHoverPoint;
		public float cameraHoverPointOffsetHeight;
		public BVector lastCameraLoc, lastCameraHoverPoint;
		public bool haveHoverPoint, hoverPointOverTerrain;
		public HudItemEnabledStates hudItemEnabled = new HudItemEnabledStates();
		public List<BObjectiveArrow> objectiveArrows = [];

		#region IEndianStreamSerializable Members
		void SerializeUserMode16(IO.EndianStream s)
		{
			s.Stream(ref this.uiPowerRadius);
			s.Stream(ref this.uiProtoPowerId);
			s.Stream(ref this.uiModeRestoreCameraZoomMin); s.Stream(ref this.uiModeRestoreCameraZoomMax); s.Stream(ref this.uiModeRestoreCameraZoom);
			s.Stream(ref this.uiModeRestoreCameraPitchMin); s.Stream(ref this.uiModeRestoreCameraPitchMax); s.Stream(ref this.uiModeRestoreCameraPitch);
			s.Stream(ref this.uiModeRestoreCameraYaw);

			s.Stream(ref this.flagUiModeRestoreCameraZoomMin); s.Stream(ref this.flagUiModeRestoreCameraZoomMax); s.Stream(ref this.flagUiModeRestoreCameraZoom);
			s.Stream(ref this.flagUiModeRestoreCameraPitchMin); s.Stream(ref this.flagUiModeRestoreCameraPitchMax); s.Stream(ref this.flagUiModeRestoreCameraPitch);
			s.Stream(ref this.flagUiModeRestoreCameraYaw);
			s.Stream(ref this.flagCameraScrollEnabled); s.Stream(ref this.flagCameraYawEnabled); s.Stream(ref this.flagCameraZoomEnabled); s.Stream(ref this.flagCameraAutoZoomInstantEnabled);
			s.Stream(ref this.flagCameraAutoZoomEnabled); s.Stream(ref this.flagRestoreCameraEnableUserScroll); s.Stream(ref this.flagRestoreCameraEnableUserYaw); s.Stream(ref this.flagRestoreCameraEnableUserZoom);
			s.Stream(ref this.flagRestoreCameraEnableAutoZoomInstant); s.Stream(ref this.flagRestoreCameraEnableAutoZoom);

			s.Stream(ref this.powerType, BPowerTypeStreamer.Instance);
			s.Stream(ref this.powerUser,
				() => BPowerUser.FromType(this.powerType));
		}
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.userMode); s.Stream(ref this.subMode);
			if (this.userMode == 16)
				this.SerializeUserMode16(s);

			BSaveGame.StreamArray16(s, ref this.selectionList);
			s.Stream(ref this.cameraZoomMin); s.Stream(ref this.cameraZoomMax);
			s.Stream(ref this.cameraPitchMin); s.Stream(ref this.cameraPitchMax);
			s.Stream(ref this.cameraPitch); s.Stream(ref this.cameraYaw);
			s.Stream(ref this.cameraZoom); s.Stream(ref this.cameraFov);
			s.StreamV(ref this.hoverPoint); s.StreamV(ref this.cameraHoverPoint);
			s.Stream(ref this.cameraHoverPointOffsetHeight);
			s.StreamV(ref this.lastCameraLoc); s.StreamV(ref this.lastCameraHoverPoint);
			s.Stream(ref this.haveHoverPoint); s.Stream(ref this.hoverPointOverTerrain);
			s.Stream(ref this.hudItemEnabled);
			BSaveGame.StreamCollection(s, this.objectiveArrows);
		}
		#endregion
	};
}
