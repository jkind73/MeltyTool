
namespace KSoft.Phoenix.Phx
{
	public sealed class BRumbleEvent
		: IO.ITagElementStringNameStreamable
	{
		#region LeftRumbleType
		BRumbleType mLeftRumbleType_;
		public BRumbleType LeftRumbleType
		{
			get { return this.mLeftRumbleType_; }
			set { this.mLeftRumbleType_ = value; }
		}
		#endregion

		#region RightRumbleType
		BRumbleType mRightRumbleType_;
		public BRumbleType RightRumbleType
		{
			get { return this.mRightRumbleType_; }
			set { this.mRightRumbleType_ = value; }
		}
		#endregion

		#region Duration
		float mDuration_;
		public float Duration
		{
			get { return this.mDuration_; }
			set { this.mDuration_ = value; }
		}
		#endregion

		#region LeftStrength
		float mLeftStrength_;
		public float LeftStrength
		{
			get { return this.mLeftStrength_; }
			set { this.mLeftStrength_ = value; }
		}
		#endregion

		#region RightStrength
		float mRightStrength_;
		public float RightStrength
		{
			get { return this.mRightStrength_; }
			set { this.mRightStrength_ = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnumOpt("LeftRumbleType", ref this.mLeftRumbleType_, e => e != BRumbleType.NONE);
			s.StreamAttributeEnumOpt("RightRumbleType", ref this.mRightRumbleType_, e => e != BRumbleType.NONE);
			s.StreamAttributeOpt("Duration", ref this.mDuration_, Predicates.IsNotZero);
			s.StreamAttributeOpt("LeftStrength", ref this.mLeftStrength_, Predicates.IsNotZero);
			s.StreamAttributeOpt("RightStrength", ref this.mRightStrength_, Predicates.IsNotZero);
		}
		#endregion
	};
}