
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoTechEffectTarget
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams("Target")
		{
			rootName = null,
			flags = 0
		};
		#endregion

		BProtoTechEffectTargetType mType_ = BProtoTechEffectTargetType.NONE;
		public BProtoTechEffectTargetType Type
		{
			get { return this.mType_; }
			set { this.mType_ = value; }
		}

		int mValueId_ = TypeExtensions.K_NONE;
		public int ValueId
		{
			get { return this.mValueId_; }
			set { this.mValueId_ = value; }
		}

		public DatabaseObjectKind ObjectKind { get {
			switch (this.mType_)
			{
			case BProtoTechEffectTargetType.PROTO_UNIT:
				return DatabaseObjectKind.UNIT;
			case BProtoTechEffectTargetType.PROTO_SQUAD:
				return DatabaseObjectKind.SQUAD;
			case BProtoTechEffectTargetType.TECH:
				return DatabaseObjectKind.TECH;

			default:
				return DatabaseObjectKind.NONE;
			}
		} }

		#region ITagElementStreamable<string> Members
		void StreamValueId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs)
			where TDoc : class
			where TCursor : class
		{
			DatabaseObjectKind kind = this.ObjectKind;

			if (kind != DatabaseObjectKind.NONE)
				xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mValueId_, kind, false, XML.XmlUtil.K_SOURCE_CURSOR);
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttributeEnum("type", ref this.mType_);
			this.StreamValueId(s, xs);
		}
		#endregion
	};
}