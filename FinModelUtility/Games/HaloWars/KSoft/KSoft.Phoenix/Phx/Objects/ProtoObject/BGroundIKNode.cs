
using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.Phx
{
	public sealed class BGroundIkNode
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "GroundIK",
		};
		#endregion

		#region Name
		string mName_;
		public string Name
		{
			get { return this.mName_; }
			set { this.mName_ = value; }
		}
		#endregion

		#region IKRange
		float mIkRange_;
		public float IkRange
		{
			get { return this.mIkRange_; }
			set { this.mIkRange_ = value; }
		}

		public bool IkRangeIsValid { get { return this.IkRange >= 0.0; } }
		#endregion

		#region LinkCount
		int mLinkCount_;
		public int LinkCount
		{
			get { return this.mLinkCount_; }
			set { this.mLinkCount_ = value; }
		}

		public bool LinkCountIsValid { get { return this.LinkCount >= byte.MinValue && this.LinkCount <= byte.MaxValue; } }
		#endregion

		#region AxisPositioning
		BVector mAxisPositioning_;
		public BVector AxisPositioning
		{
			get { return this.mAxisPositioning_; }
			set { this.mAxisPositioning_ = value; }
		}

		public bool OnLeft { get { return this.mAxisPositioning_.X <= -1.0f; } }
		public bool OnRight { get { return this.mAxisPositioning_.X >= +1.0f; } }

		public bool InFront { get { return this.mAxisPositioning_.Z >= +1.0f; } }
		public bool InBack { get { return this.mAxisPositioning_.Z <= -1.0f; } }
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamCursor(ref this.mName_);
			s.StreamAttribute("ikRange", ref this.mIkRange_);
			s.StreamAttribute("linkCount", ref this.mLinkCount_);
			s.StreamAttributeOpt("x", ref this.mAxisPositioning_.X, Predicates.IsNotZero);
			s.StreamAttributeOpt("z", ref this.mAxisPositioning_.Z, Predicates.IsNotZero);
		}
		#endregion
	};
}