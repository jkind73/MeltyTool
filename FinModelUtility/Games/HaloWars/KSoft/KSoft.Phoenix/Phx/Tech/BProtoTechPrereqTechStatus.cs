
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoTechPrereqTechStatus
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "TechStatus",
		};
		#endregion

		#region TechStatus
		BProtoTechStatus mTechStatus_ = BProtoTechStatus.INVALID;
		[Meta.UnusedData("Not actually parsed by the engine")]
		public BProtoTechStatus TechStatus
		{
			get { return this.mTechStatus_; }
			set { this.mTechStatus_ = value; }
		}

		static System.Predicate<BProtoTechStatus> bProtoTechStatusIsNotInvalid_ = (BProtoTechStatus v) => v != BProtoTechStatus.INVALID;
		#endregion

		#region TechID
		int mTechId_ = TypeExtensions.K_NONE;
		[Meta.BProtoTechReference]
		public int TechId
		{
			get { return this.mTechId_; }
			set { this.mTechId_ = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttributeEnumOpt("status", ref this.mTechStatus_, bProtoTechStatusIsNotInvalid_);
			xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mTechId_, DatabaseObjectKind.TECH, false, XML.XmlUtil.K_SOURCE_CURSOR);
		}
		#endregion
	};
}