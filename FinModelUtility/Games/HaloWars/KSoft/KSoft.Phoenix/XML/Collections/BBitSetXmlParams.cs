
namespace KSoft.Phoenix.XML
{
	public sealed class BBitSetXmlParams
		: BListXmlParams
	{
		// enabled flags are written as xml elements without any attributes or text. Eg:
		// <InfiniteUses /> means the BPowerFlags.InfiniteUses bit is set
		public bool elementItselfMeansTrue;

		private BBitSetXmlParams()
		{
		}
		/// <summary>Sets ElementName, defaults to InnerText data usage and data interning</summary>
		/// <param name="elementName">Name of the xml element which represents the type (enum) value</param>
		public BBitSetXmlParams(string elementName)
		{
			this.elementName = elementName;

			this.flags = 0
			             | BCollectionXmlParamsFlags.USE_INNER_TEXT_FOR_DATA
			             | BCollectionXmlParamsFlags.INTERN_DATA_NAMES;
		}

		public static readonly BBitSetXmlParams KFlagsSansRoot = new BBitSetXmlParams("Flag");
		public static readonly BBitSetXmlParams KFlagsAreElementNamesThatMeanTrue = new BBitSetXmlParams()
		{
			elementItselfMeansTrue = true,
		};
	};
}