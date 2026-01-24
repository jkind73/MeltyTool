
namespace KSoft.Phoenix.Phx
{
	public sealed class BTrigger
		: TriggerScriptIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams("Trigger")
		{
			dataName = DatabaseNamedObject.K_XML_ATTR_NAME_N,
		};

		const string K_XML_ATTR_ACTIVE_ = "Active";
		const string K_XML_ATTR_EVALUATE_FREQUENCY_ = "EvaluateFrequency";
		const string K_XML_ATTR_EVAL_LIMIT_ = "EvalLimit";
		const string K_XML_ATTR_CONDITIONAL_TRIGGER_ = "ConditionalTrigger";
	#endregion

	bool mActive_;

		int mEvaluateFrequency_;

		int mEvalLimit_;

		bool mConditionalTrigger_;

		public Collections.BListAutoId<BTriggerCondition> Conditions { get; private set; } = new Collections.BListAutoId<BTriggerCondition>();

		/// <summary>True if <see cref="Conditions"/> are OR, false if they're AND</summary>
		public bool OrConditions { get; set; }
		public Collections.BListAutoId<BTriggerEffect> EffectsOnTrue { get; private set; } = new Collections.BListAutoId<BTriggerEffect>();
		public Collections.BListAutoId<BTriggerEffect> EffectsOnFalse { get; private set; } = new Collections.BListAutoId<BTriggerEffect>();

		void StreamConditions<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var kAndParams = BTriggerCondition.KBListXmlParamsAnd;

			if (s.IsReading)
			{
				if (this.OrConditions = !s.ElementsExists(kAndParams.rootName))
					XML.XmlUtil.Serialize(s, this.Conditions, BTriggerCondition.KBListXmlParamsOr);
				else
					XML.XmlUtil.Serialize(s, this.Conditions, kAndParams);
			}
			else if (s.IsWriting)
			{
				// Even if there are no conditions, the runtime expects there to be an empty And tag :|
				// Well, technically we could use an empty Or tag as well, but it wouldn't be consistent
				// with the engine. The runtime will assume the the TS is bad if neither tag is present
				if (this.Conditions.Count == 0)
					s.WriteElement(kAndParams.rootName);
				else
					XML.XmlUtil.Serialize(s,
					                      this.Conditions,
					                      this.OrConditions ? BTriggerCondition.KBListXmlParamsOr : kAndParams);
			}
		}
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttribute(K_XML_ATTR_ACTIVE_, ref this.mActive_);
			s.StreamAttribute(K_XML_ATTR_EVALUATE_FREQUENCY_, ref this.mEvaluateFrequency_);
			s.StreamAttribute(K_XML_ATTR_EVAL_LIMIT_, ref this.mEvalLimit_);
			s.StreamAttribute(K_XML_ATTR_CONDITIONAL_TRIGGER_, ref this.mConditionalTrigger_);

			// These tags must exist no matter what :|
			using (s.EnterCursorBookmark(BTriggerCondition.K_XML_ROOT_NAME))
				this.StreamConditions(s);

			using (s.EnterCursorBookmark(BTriggerEffect.K_XML_ROOT_NAME_ON_TRUE))
				XML.XmlUtil.Serialize(s, this.EffectsOnTrue, BTriggerEffect.KBListXmlParams);

			using (s.EnterCursorBookmark(BTriggerEffect.K_XML_ROOT_NAME_ON_FALSE))
				XML.XmlUtil.Serialize(s, this.EffectsOnFalse, BTriggerEffect.KBListXmlParams);
		}
	};
}
