
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoImpactEffect
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams("TerrainEffect")
		{
			dataName = "name",
			flags = 0
		};
		public static readonly Engine.XmlFileInfo KXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.DATA,
			FileName = "ImpactEffects.xml",
			RootName = "ImpactEffects"//kBListXmlParams.RootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo KProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.LISTS,
			KXmlFileInfo);
		#endregion

		#region Limit
		int mLimit_ = 2;
		public int Limit
		{
			get { return this.mLimit_; }
			set { this.mLimit_ = value; }
		}
		#endregion

		#region Lifespan
		float mLifespan_ = 3.0f;
		public float Lifespan
		{
			get { return this.mLifespan_; }
			set { this.mLifespan_ = value; }
		}
		#endregion

		#region FileName
		string mFileName_;
		public string FileName
		{
			get { return this.mFileName_; }
			set { this.mFileName_ = value; }
		}
		#endregion

		#region BListAutoIdObject Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeOpt("limit", ref this.mLimit_, x => x != 2);
			s.StreamAttributeOpt("lifespan", ref this.mLifespan_, x => x != 3.0f);
			s.StreamCursor(ref this.mFileName_);
		}
		#endregion
	};
}