
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoSquadUnit
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams("Unit")
		{
			flags = 0
		};
		#endregion

		#region Count
		int mCount_;
		public int Count
		{
			get { return this.mCount_; }
			set { this.mCount_ = value; }
		}
		#endregion

		#region UnitID
		int mUnitId_;
		[Meta.BProtoObjectReference]
		public int UnitId
		{
			get { return this.mUnitId_; }
			set { this.mUnitId_ = value; }
		}
		#endregion

		#region UnitRole
		BUnitRole mUnitRole_;
		public BUnitRole UnitRole
		{
			get { return this.mUnitRole_; }
			set { this.mUnitRole_ = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttribute("count", ref this.mCount_);
			s.StreamAttributeEnumOpt("role", ref this.mUnitRole_, e => e != BUnitRole.NORMAL);
			xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mUnitId_, DatabaseObjectKind.OBJECT, false, XML.XmlUtil.K_SOURCE_CURSOR);
		}
		#endregion
	};
}