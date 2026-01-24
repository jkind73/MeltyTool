
namespace KSoft.Phoenix.Phx
{
	public sealed class TerrainTileType
		: Collections.BListAutoIdObject
	{
		public const int C_UNDEFINED_INDEX = 0;

		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams("TerrainTileType")
		{
			dataName = "name",
			flags = 0,
		};
		public static readonly Engine.XmlFileInfo KXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.DATA,
			FileName = "TerrainTileTypes.xml",
			RootName = "TerrainTileTypes"//kBListXmlParams.RootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo KProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.LISTS,
			KXmlFileInfo);
		#endregion

		#region EditorColor
		uint mEditorColor_;
		public System.Drawing.Color EditorColor
		{
			get { return System.Drawing.Color.FromArgb((int) this.mEditorColor_); }
			set { this.mEditorColor_ = (uint)value.ToArgb(); }
		}
		#endregion

		#region ImpactEffect
		string mImpactEffect_;
		[Meta.UnusedData]
		[Meta.VisualReference]
		public string ImpactEffect
		{
			get { return this.mImpactEffect_; }
			set { this.mImpactEffect_ = value; }
		}
		#endregion

		#region BListAutoIdObject Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttribute("EditorColor", ref this.mEditorColor_, NumeralBase.HEX);
			s.StreamElementOpt("ImpactEffect", ref this.mImpactEffect_, Predicates.IsNotNullOrEmpty);
		}
		#endregion
	};
}