
using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.Phx
{
	public sealed partial class BProtoObjectChildObject
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			rootName = "ChildObjects",
			elementName = "Object",
		};
		#endregion

		#region Type
		ChildObjectType mType_ = ChildObjectType.OBJECT;
		public ChildObjectType Type
		{
			get { return this.mType_; }
			set { this.mType_ = value; }
		}
		#endregion

		#region ID
		int mId_ = TypeExtensions.K_NONE;
		public int Id
		{
			get { return this.mId_; }
			set { this.mId_ = value; }
		}
		#endregion

		#region AttachBone
		string mAttachBone_;
		public string AttachBone
		{
			get { return this.mAttachBone_; }
			set { this.mAttachBone_ = value; }
		}
		#endregion

		#region Offset
		BVector mOffset_;
		public BVector Offset
		{
			get { return this.mOffset_; }
			set { this.mOffset_ = value; }
		}
		#endregion

		#region Rotation
		float mRotation_;
		public float Rotation
		{
			get { return this.mRotation_; }
			set { this.mRotation_ = value; }
		}
		#endregion

		#region UserCivID
		int mUserCivId_ = TypeExtensions.K_NONE;
		[Meta.BCivReference]
		public int UserCivId
		{
			get { return this.mUserCivId_; }
			set { this.mUserCivId_ = value; }
		}
		#endregion

		public DatabaseObjectKind TypeObjectKind { get {
			if (this.Type == ChildObjectType.ONE_TIME_SPAWN_SQUAD)
				return DatabaseObjectKind.SQUAD;

			return DatabaseObjectKind.OBJECT;
		} }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttributeEnumOpt("Type", ref this.mType_, e => e != ChildObjectType.OBJECT);

			xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mId_, this.TypeObjectKind, isOptional: false, xmlSource: XML.XmlUtil.K_SOURCE_CURSOR);
			s.StreamAttributeOpt("AttachBone", ref this.mAttachBone_, Predicates.IsNotNullOrEmpty);
			s.StreamBVector("Offset", ref this.mOffset_, xmlSource: XML.XmlUtil.K_SOURCE_ATTR);
			s.StreamAttributeOpt("Rotation", ref this.mRotation_, Predicates.IsNotZero);
			xs.StreamDbid(s, "UserCiv", ref this.mUserCivId_, DatabaseObjectKind.CIV, xmlSource: XML.XmlUtil.K_SOURCE_ATTR);
		}
		#endregion
	};
}