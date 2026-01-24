
namespace KSoft.Phoenix.Phx
{
	public sealed class BSweetSpotIkNode
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "SweetSpotIK",
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

		#region LinkCount
		int mLinkCount_;
		public int LinkCount
		{
			get { return this.mLinkCount_; }
			set { this.mLinkCount_ = value; }
		}

		public bool LinkCountIsValid { get { return this.LinkCount >= byte.MinValue && this.LinkCount <= byte.MaxValue; } }
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamCursor(ref this.mName_);
			s.StreamAttribute("linkCount", ref this.mLinkCount_);
		}
		#endregion
	};
}