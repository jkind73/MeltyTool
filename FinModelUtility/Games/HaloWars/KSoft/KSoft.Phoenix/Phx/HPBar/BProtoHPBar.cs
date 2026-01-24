
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoHpBar
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "HPBar",
			dataName = "name",
		};
		#endregion

		public BProtoHpBar()
		{
		}

		#region BListObjectBase Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
		}
		#endregion
	};
}