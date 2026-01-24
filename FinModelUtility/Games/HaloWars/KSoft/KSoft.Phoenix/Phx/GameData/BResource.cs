
namespace KSoft.Phoenix.Phx
{
	public sealed class BResource
		: Collections.BListAutoIdObject
	{
		// fucking squads.xml and techs.xml uses a lower-case type name :|
		const bool K_USE_LOWERCASE_COST_TYPE_HACK_ = true;

		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams("Resource",
			additionalFlags: XML.BCollectionXmlParamsFlags.DO_NOT_WRITE_UNDEFINED_DATA);

		public static readonly Collections.BTypeValuesParams<float> KBListTypeValuesParams = new
			Collections.BTypeValuesParams<float>(db => db.GameData.Resources) { kTypeGetInvalid = PhxUtil.KGetInvalidSingle };
		public static readonly XML.BTypeValuesXmlParams<float> KBListTypeValuesXmlParams = new
			XML.BTypeValuesXmlParams<float>("Resource", "Type");
		public static readonly XML.BTypeValuesXmlParams<float> KBListTypeValuesXmlParamsCost = new
			XML.BTypeValuesXmlParams<float>("Cost", "ResourceType");
#pragma warning disable 0429
		public static readonly XML.BTypeValuesXmlParams<float> KBListTypeValuesXmlParamsCostLowercaseType = !K_USE_LOWERCASE_COST_TYPE_HACK_
			? KBListTypeValuesXmlParamsCost
			: new XML.BTypeValuesXmlParams<float>("Cost", "ResourceType".ToLowerInvariant()
			);
#pragma warning restore 0429
		public static readonly XML.BTypeValuesXmlParams<float> KBListTypeValuesXmlParamsAddResource = new
			XML.BTypeValuesXmlParams<float>("AddResource", null, XML.BCollectionXmlParamsFlags.USE_INNER_TEXT_FOR_DATA);
		#endregion

		bool mDeductable_;
		public bool Deductable
		{
			get { return this.mDeductable_; }
			set { this.mDeductable_ = value; }
		}

		public BResource() { }
		internal BResource(bool deductable) {
			this.mDeductable_ = deductable; }

		#region BListAutoIdObject Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttribute("Deductable", ref this.mDeductable_);
		}
		#endregion
	};
}