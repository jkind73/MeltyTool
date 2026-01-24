
namespace KSoft.Phoenix.Phx
{
	public sealed class BCollectiblesManager
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public const string K_XML_ROOT_NAME = "CollectiblesDefinitions";

		public static readonly Engine.XmlFileInfo KXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.DATA,
			FileName = "Skulls.xml",
			RootName = K_XML_ROOT_NAME
		};
		#endregion

		int mXmlVersion_ = TypeExtensions.K_NONE;
		public BCollectiblesSkullManager SkullManager { get; private set; } = new BCollectiblesSkullManager();

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamElementOpt("CollectiblesXMLVersion", ref this.mXmlVersion_, Predicates.IsNotNone);
			this.SkullManager.Serialize(s);
		}
		#endregion
	};
}
