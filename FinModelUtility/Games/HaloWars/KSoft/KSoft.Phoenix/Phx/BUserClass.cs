
namespace KSoft.Phoenix.Phx
{
	public sealed class BUserClass
		: Collections.BListAutoIdObject
		, IDatabaseIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams("UserClass")
		{
			dataName = "Name",
			flags = 0
		};
		public static readonly Engine.XmlFileInfo KXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.DATA,
			FileName = "UserClasses.xml",
			RootName = KBListXmlParams.rootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo KProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.LISTS,
			KXmlFileInfo);
		#endregion

		int mDbId_ = TypeExtensions.K_NONE;
		public int DbId { get { return this.mDbId_; } }

		public Collections.BListAutoId<BUserClassField> Fields { get; private set; } = new Collections.BListAutoId<BUserClassField>();

		#region BListAutoIdObject Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttribute("DBID", ref this.mDbId_);
			XML.XmlUtil.Serialize(s, this.Fields, BUserClassField.KBListXmlParams);
		}
		#endregion
	};
}
