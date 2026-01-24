
namespace KSoft.Phoenix.Phx
{
	public sealed class BTriggerProtoEffect
		: TriggerSystemProtoObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams("Effect")
		{
			dataName = DatabaseNamedObject.K_XML_ATTR_NAME_N,
			flags = 0,
		};
		#endregion

		public BTriggerProtoEffect() { }
		public BTriggerProtoEffect(BTriggerSystem root, BTriggerEffect instance) : base(root, instance)
		{
		}
	};
}