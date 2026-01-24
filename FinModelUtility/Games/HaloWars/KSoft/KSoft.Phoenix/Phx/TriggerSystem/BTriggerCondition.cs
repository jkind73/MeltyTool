
namespace KSoft.Phoenix.Phx
{
	public sealed class BTriggerCondition
		: TriggerScriptObjectWithArgs
	{
		#region Xml constants
		public const string K_XML_ROOT_NAME = "TriggerConditions";

		public static readonly XML.BListXmlParams KBListXmlParamsAnd = new XML.BListXmlParams
		{
			rootName = "And",
			elementName = "Condition",
			dataName = K_XML_ATTR_TYPE,
		};
		public static readonly XML.BListXmlParams KBListXmlParamsOr = new XML.BListXmlParams
		{
			rootName = "Or",
			elementName = "Condition",
			dataName = K_XML_ATTR_TYPE,
		};

		const string K_XML_ATTR_INVERT_ = "Invert";
		const string K_XML_ATTR_ASYNC_ = "Async"; // engine treats this as optional, but not the key
		const string K_XML_ATTR_ASYNC_PARAMETER_KEY_ = "AsyncParameterKey"; // really a sbyte
		#endregion

		bool mInvert_;

		bool mAsync_;
		public bool Async { get { return this.mAsync_; } }

		int mAsyncParameterKey_; // References a Parameter (via SigID). Runtime then takes that parameter's BTriggerVarID
		public int AsyncParameterKey { get { return this.mAsyncParameterKey_; } }

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttribute(K_XML_ATTR_INVERT_, ref this.mInvert_);
			s.StreamAttribute(K_XML_ATTR_ASYNC_, ref this.mAsync_);
			s.StreamAttribute(K_XML_ATTR_ASYNC_PARAMETER_KEY_, ref this.mAsyncParameterKey_);
		}
	};
}