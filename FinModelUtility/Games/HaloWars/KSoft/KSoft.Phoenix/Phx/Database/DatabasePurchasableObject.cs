
namespace KSoft.Phoenix.Phx
{
	public abstract class DatabasePurchasableObject
		: DatabaseNamedObject
	{
		XML.BTypeValuesXmlParams<float> mResourceCostXmlParams_;

		public Collections.BTypeValuesSingle ResourceCost { get; private set; }

		#region BuildTime
		float mBuildTime_ = PhxUtil.K_INVALID_SINGLE;
		public float BuildTime
		{
			get { return this.mBuildTime_; }
			set { this.mBuildTime_ = value; }
		}
		#endregion

		#region ResearchTime
		float mResearchTime_ = PhxUtil.K_INVALID_SINGLE;
		public float ResearchTime
		{
			get { return this.mResearchTime_; }
			set { this.mResearchTime_ = value; }
		}
		#endregion

		/// <summary>Time, in seconds, it takes to build or research this object</summary>
		public float PurchaseTime { get {
			return this.mBuildTime_ != PhxUtil.K_INVALID_SINGLE
				? this.mBuildTime_
				: this.mResearchTime_;
		} }

		protected DatabasePurchasableObject(Collections.BTypeValuesParams<float> rsrcCostParams, XML.BTypeValuesXmlParams<float> rsrcCostXmlParams)
		{
			this.mResourceCostXmlParams_ = rsrcCostXmlParams;

			this.ResourceCost = new Collections.BTypeValuesSingle(rsrcCostParams);
		}

		#region IXmlElementStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			XML.XmlUtil.Serialize(s, this.ResourceCost, this.mResourceCostXmlParams_);
			s.StreamElementOpt("BuildPoints", ref this.mBuildTime_, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("ResearchPoints", ref this.mResearchTime_, PhxPredicates.IsNotInvalid);
		}
		#endregion
	};
}