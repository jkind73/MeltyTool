
using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	public struct BObjectAttachments
		: IO.IEndianStreamSerializable
	{
		public BVector position, up, forward;
		public BEntityID attachmentObjectId;
		public int toBoneHandle, fromBoneHandle;
		public bool isUnitAttachment
			, useOffset
			;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamV(ref this.position); s.StreamV(ref this.up); s.StreamV(ref this.forward);
			s.Stream(ref this.attachmentObjectId);
			s.Stream(ref this.toBoneHandle); s.Stream(ref this.fromBoneHandle);
			s.Stream(ref this.isUnitAttachment); s.Stream(ref this.useOffset);
		}
		#endregion
	};
}