
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoSquadShieldBubble
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "ShieldBubble",
		};
		#endregion

		int mTargetSquadId_ = TypeExtensions.K_NONE;
		[Meta.BProtoSquadReference]
		public int TargetShieldSquadId
		{
			get { return this.mTargetSquadId_; }
			set { this.mTargetSquadId_ = value; }
		}

		int mShieldSquadId_ = TypeExtensions.K_NONE;
		[Meta.BProtoSquadReference]
		public int ShieldSquadId
		{
			get { return this.mShieldSquadId_; }
			set { this.mShieldSquadId_ = value; }
		}

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDbid(s, "target", ref this.mTargetSquadId_, DatabaseObjectKind.SQUAD, false, XML.XmlUtil.K_SOURCE_ATTR);
			xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mShieldSquadId_, DatabaseObjectKind.SQUAD, false, XML.XmlUtil.K_SOURCE_CURSOR);
		}
		#endregion
	};
}