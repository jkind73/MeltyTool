
namespace KSoft.Phoenix.Phx
{
	public sealed class BInfectionMap
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			rootName = "InfectionMap",
			elementName = "InfectionMapEntry",
		};
		#endregion

		int mBaseObjectId_ = TypeExtensions.K_NONE;
		int mInfectedObjectId_ = TypeExtensions.K_NONE;
		int mInfectedSquadId_ = TypeExtensions.K_NONE;

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDbid(s, "base", ref this.mBaseObjectId_, DatabaseObjectKind.OBJECT, false, XML.XmlUtil.K_SOURCE_ATTR);
			xs.StreamDbid(s, "infected", ref this.mInfectedObjectId_, DatabaseObjectKind.OBJECT, false, XML.XmlUtil.K_SOURCE_ATTR);
			if(xs.Database.Engine.Build != Engine.PhxEngineBuild.ALPHA)
				xs.StreamDbid(s, "infectedSquad", ref this.mInfectedSquadId_, DatabaseObjectKind.SQUAD, false, XML.XmlUtil.K_SOURCE_ATTR);
		}
		#endregion
	};
}