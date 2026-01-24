
using BVec2 = System.Numerics.Vector2;

namespace KSoft.Phoenix.Runtime
{
	public struct BAdditionalTextures
		: IO.IEndianStreamSerializable
	{
		public int renderType, texture;
		public BVec2 texUvOfs;
		public float texUvScale, texInten, texScrollSpeed;
		// relative in gamefiles, TexTimeout - gWorld->getGametimeFloat()
		public float texTimeoutOffset;
		public bool modulateOffset
			, modulateIntensity
			, shouldBeCopied
			, texClamp
			, texScrollLoop
			;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.renderType);
			s.Stream(ref this.texture);
			s.StreamV(ref this.texUvOfs);
			s.Stream(ref this.texUvScale);
			s.Stream(ref this.texInten);
			s.Stream(ref this.texScrollSpeed);
			s.Stream(ref this.texTimeoutOffset);
			s.Stream(ref this.modulateOffset);
			s.Stream(ref this.modulateIntensity);
			s.Stream(ref this.shouldBeCopied);
			s.Stream(ref this.texClamp);
			s.Stream(ref this.texScrollLoop);
		}
		#endregion
	};
}