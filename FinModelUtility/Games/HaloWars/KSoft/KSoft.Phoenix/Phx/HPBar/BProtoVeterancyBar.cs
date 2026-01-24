
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoVeterancyBar
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "VeterancyBar",
			dataName = "name",
		};
		#endregion

		public BProtoVeterancyBar()
		{
		}

		#region BListObjectBase Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
		}
		#endregion
	};
}