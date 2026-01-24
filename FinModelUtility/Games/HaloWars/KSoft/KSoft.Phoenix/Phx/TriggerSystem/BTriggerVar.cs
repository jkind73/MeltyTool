
namespace KSoft.Phoenix.Phx
{
	public sealed class BTriggerVar
		: TriggerScriptIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams("TriggerVar")
		{
			dataName = DatabaseNamedObject.K_XML_ATTR_NAME_N,
		};

		const string K_XML_ATTR_TYPE_ = "Type";
		const string K_XML_ATTR_IS_NULL_ = "IsNull";
		#endregion

		BTriggerVarType mType_ = BTriggerVarType.NONE;
		public BTriggerVarType Type { get { return this.mType_; } }

		bool mIsNull_;
		public bool IsNull { get { return this.mIsNull_; } }

		//string mValue;

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttributeEnum(K_XML_ATTR_TYPE_, ref this.mType_);
			s.StreamAttribute    (K_XML_ATTR_IS_NULL_, ref this.mIsNull_);
			//s.StreamCursor(ref mValue);
		}
	};
}