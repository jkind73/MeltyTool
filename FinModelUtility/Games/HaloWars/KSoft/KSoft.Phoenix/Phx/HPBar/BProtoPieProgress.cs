
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoPieProgress
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "PieProgress",
			dataName = "name",
		};
		#endregion

		public BProtoPieProgress()
		{
		}

		#region BListObjectBase Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
		}
		#endregion
	};
}