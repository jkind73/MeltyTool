
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoShieldBubbleTypes
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		const string K_XML_ROOT_ = "ShieldBubbleTypes";
		#endregion

		int mDefaultShieldSquadId_ = TypeExtensions.K_NONE;
		[Meta.BProtoSquadReference]
		public int DefaultShieldSquadId
		{
			get { return this.mDefaultShieldSquadId_; }
			set { this.mDefaultShieldSquadId_ = value; }
		}

		public Collections.BListArray<BProtoSquadShieldBubble> ProtoShieldIDs { get; private set; } = new Collections.BListArray<BProtoSquadShieldBubble>();

		public bool IsNotEmpty { get {
			return this.DefaultShieldSquadId.IsNotNone()
				|| !this.ProtoShieldIDs.IsEmpty;
		} }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			using (var bm = s.EnterCursorBookmarkOpt(K_XML_ROOT_)) if (bm.IsNotNull)
			{
				xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mDefaultShieldSquadId_, DatabaseObjectKind.SQUAD, false, XML.XmlUtil.K_SOURCE_CURSOR);
				XML.XmlUtil.Serialize(s, this.ProtoShieldIDs, BProtoSquadShieldBubble.KBListXmlParams);
			}
		}
		#endregion
	};
}
