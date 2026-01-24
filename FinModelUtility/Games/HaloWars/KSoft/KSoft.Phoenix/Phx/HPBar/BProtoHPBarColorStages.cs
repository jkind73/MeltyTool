
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoHpBarColorStages
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "ColorStages",
			dataName = "name",
		};
		#endregion

		public BProtoHpBarColorStages()
		{
		}

		#region BListObjectBase Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
		}
		#endregion
	};
}