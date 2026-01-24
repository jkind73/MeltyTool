
namespace KSoft.Phoenix.Phx
{
	public sealed class BHitZone
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "HitZone",
		};
		#endregion

		#region AttachmentName
		string mAttachmentName_;
		public string AttachmentName
		{
			get { return this.mAttachmentName_; }
			set { this.mAttachmentName_ = value; }
		}
		#endregion

		#region Hitpoints
		float mHitpoints_ = PhxUtil.K_INVALID_SINGLE;
		public float Hitpoints
		{
			get { return this.mHitpoints_; }
			set { this.mHitpoints_ = value; }
		}
		#endregion

		#region Shieldpoints
		float mShieldpoints_ = PhxUtil.K_INVALID_SINGLE;
		public float Shieldpoints
		{
			get { return this.mShieldpoints_; }
			set { this.mShieldpoints_ = value; }
		}
		#endregion

		#region Active
		float mActive_;
		public float Active
		{
			get { return this.mActive_; }
			set { this.mActive_ = value; }
		}
		#endregion

		#region HasShields
		float mHasShields_;
		public float HasShields
		{
			get { return this.mHasShields_; }
			set { this.mHasShields_ = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamCursor(ref this.mAttachmentName_);
			s.StreamElementOpt("Hitpoints", ref this.mHitpoints_, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("Shieldpoints", ref this.mShieldpoints_, PhxPredicates.IsNotInvalid);
		}
		#endregion
	};
}