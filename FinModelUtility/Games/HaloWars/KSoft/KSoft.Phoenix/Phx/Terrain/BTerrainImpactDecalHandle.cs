
using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.Phx
{
	public sealed class BTerrainImpactDecalHandle
		: IO.ITagElementStringNameStreamable
	{
		public enum OrientationType
		{
			RANDOM,
			FORWARD,
		};

		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "ImpactDecal",
		};
		#endregion

		#region Size
		BVector mSize_ = new BVector(2.0f, 0, 2.0f, 0);
		public BVector Size
		{
			get { return this.mSize_; }
			set { this.mSize_ = value; }
		}
		#endregion

		#region TimeFullyOpaque
		float mTimeFullyOpaque_ = 5.0f;
		public float TimeFullyOpaque
		{
			get { return this.mTimeFullyOpaque_; }
			set { this.mTimeFullyOpaque_ = value; }
		}
		#endregion

		#region FadeOutTime
		float mFadeOutTime_ = 10.0f;
		public float FadeOutTime
		{
			get { return this.mFadeOutTime_; }
			set { this.mFadeOutTime_ = value; }
		}
		#endregion

		#region Orientation
		OrientationType mOrientation_ = OrientationType.RANDOM;
		public OrientationType Orientation
		{
			get { return this.mOrientation_; }
			set { this.mOrientation_ = value; }
		}
		#endregion

		#region TextureName
		string mTextureName_;
		[Meta.TextureReference]
		public string TextureName
		{
			get { return this.mTextureName_; }
			set { this.mTextureName_ = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOpt("sizeX", ref this.mSize_.X, f => f != 2.0f);
			s.StreamAttributeOpt("sizeZ", ref this.mSize_.Z, f => f != 2.0f);
			s.StreamAttributeOpt("timeFullyOpaque", ref this.mTimeFullyOpaque_, f => f != 5.0f);
			s.StreamAttributeOpt("fadeOutTime", ref this.mFadeOutTime_, f => f != 10.0f);
			s.StreamAttributeEnumOpt("orientation", ref this.mOrientation_, e => e != OrientationType.RANDOM);
			s.StreamCursor(ref this.mTextureName_);
		}
		#endregion
	};
}