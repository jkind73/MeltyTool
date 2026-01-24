
namespace KSoft.Phoenix.Phx
{
	public sealed class BUserClassField
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams()
		{
			elementName = "Fields",
			dataName = "Name",
		};
		#endregion

		BTriggerVarType mType_;

		#region IXmlElementStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeEnum("Type", ref this.mType_);
		}
		#endregion
	};
}