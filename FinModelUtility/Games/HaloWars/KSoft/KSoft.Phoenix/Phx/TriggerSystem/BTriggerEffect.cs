
namespace KSoft.Phoenix.Phx
{
	/// <remarks>Effect's <see cref="TriggerScriptIdObject.Id"/> is ignored by runtime (deprecated or editor only?)</remarks>
	public sealed class BTriggerEffect
		: TriggerScriptObjectWithArgs
	{
		#region Xml constants
		public const string K_XML_ROOT_NAME_ON_TRUE = "TriggerEffectsOnTrue";
		public const string K_XML_ROOT_NAME_ON_FALSE = "TriggerEffectsOnFalse";

		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			rootName = null,
			elementName = "Effect",
			dataName = K_XML_ATTR_TYPE,
		};
		#endregion

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);
		}
	};
}