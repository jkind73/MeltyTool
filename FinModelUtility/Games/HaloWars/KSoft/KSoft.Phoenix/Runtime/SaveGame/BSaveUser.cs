
using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.Runtime
{
	sealed class BSaveUser
		: IO.IEndianStreamSerializable
	{
		public int currentPlayer, coopPlayer;
		public BVector hoverPoint, cameraHoverPoint, cameraPosition, 
			cameraForward, cameraRight, cameraUp;
		public float cameraDefaultPitch, cameraDefaultYaw, cameraDefaultZoom,
			cameraPitch, cameraYaw, cameraZoom,
			cameraFov, cameraHoverPointOffsetHeight;
		public bool haveHoverPoint, defaultCamera;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.currentPlayer);
			s.Stream(ref this.coopPlayer);
			s.StreamV(ref this.hoverPoint); s.StreamV(ref this.cameraHoverPoint); s.StreamV(ref this.cameraPosition);
			s.StreamV(ref this.cameraForward); s.StreamV(ref this.cameraRight); s.StreamV(ref this.cameraUp);
			s.Stream(ref this.cameraDefaultPitch); s.Stream(ref this.cameraDefaultYaw); s.Stream(ref this.cameraDefaultZoom);
			s.Stream(ref this.cameraPitch); s.Stream(ref this.cameraYaw); s.Stream(ref this.cameraZoom);
			s.Stream(ref this.cameraFov);
			s.Stream(ref this.cameraHoverPointOffsetHeight);
			s.Stream(ref this.haveHoverPoint);
			s.Stream(ref this.defaultCamera);
			s.StreamSignature(CSaveMarker.USER1);
		}
		#endregion
	};
}