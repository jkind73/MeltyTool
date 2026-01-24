
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoBuildingStrength
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "BuildingStrength",
			dataName = "name",
		};
		#endregion

		public BProtoBuildingStrength()
		{
		}

		#region BListObjectBase Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
		}
		#endregion
	};
}