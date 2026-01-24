
namespace KSoft.Phoenix.Phx
{
	public sealed class BTriggerProtoCondition
		: TriggerSystemProtoObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams("Condition")
		{
			dataName = DatabaseNamedObject.K_XML_ATTR_NAME_N,
			flags = 0,
		};

		const string K_XML_ATTR_ASYNC_ = "Async";
		const string K_XML_ATTR_ASYNC_PARAMETER_KEY_ = "AsyncParameterKey"; // really a sbyte
		#endregion

		bool mAsync_;
		public bool Async { get { return this.mAsync_; } }

		int mAsyncParameterKey_;
		public int AsyncParameterKey { get { return this.mAsyncParameterKey_; } }

		public BTriggerProtoCondition() { }
		public BTriggerProtoCondition(BTriggerSystem root, BTriggerCondition instance) : base(root, instance)
		{
			this.mAsync_ = instance.Async;
			this.mAsyncParameterKey_ = instance.AsyncParameterKey;
		}

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			if(s.StreamAttributeOpt(K_XML_ATTR_ASYNC_, ref this.mAsync_, Predicates.IsTrue))
				s.StreamAttribute(K_XML_ATTR_ASYNC_PARAMETER_KEY_, ref this.mAsyncParameterKey_);
		}
	};
}