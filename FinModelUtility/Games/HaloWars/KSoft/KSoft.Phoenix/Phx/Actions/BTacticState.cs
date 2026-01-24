
namespace KSoft.Phoenix.Phx
{
	/*public*/ sealed class BTacticState // suicide grunts use this...name and action are omitted, so fuck this
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "State",
			dataName = "Name",
			flags = XML.BCollectionXmlParamsFlags.USE_ELEMENT_FOR_DATA |
				XML.BCollectionXmlParamsFlags.FORCE_NO_ROOT_ELEMENT_STREAMING,
		};
	#endregion
  };
}