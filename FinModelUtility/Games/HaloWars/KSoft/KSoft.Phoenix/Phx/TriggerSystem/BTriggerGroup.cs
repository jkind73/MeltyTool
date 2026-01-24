
namespace KSoft.Phoenix.Phx
{
	public sealed class BTriggerGroup
		: TriggerScriptIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			rootName = "TriggerGroups",
			elementName = "Group",
			dataName = DatabaseNamedObject.K_XML_ATTR_NAME_N,
		};
	#endregion

	//string mValue;

	public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			//s.StreamCursor(mode, ref mValue);
		}
	};
}