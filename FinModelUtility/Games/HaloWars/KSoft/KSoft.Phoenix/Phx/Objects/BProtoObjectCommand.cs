namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoObjectCommand
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "Command",
		};
		#endregion

		#region Position
		int mPosition_;
		public int Position
		{
			get { return this.mPosition_; }
			set { this.mPosition_ = value; }
		}
		#endregion

		#region CommandType
		BProtoObjectCommandType mCommandType_ = BProtoObjectCommandType.INVALID;
		public BProtoObjectCommandType CommandType
		{
			get { return this.mCommandType_; }
			set { this.mCommandType_ = value; }
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

		#region SquadMode
		BSquadMode mSquadMode_ = BSquadMode.INVALID;
		public BSquadMode SquadMode
		{
			get { return this.mSquadMode_; }
			set { this.mSquadMode_ = value; }
		}
		#endregion

		#region AutoClose
		bool mAutoClose_;
		public bool AutoClose
		{
			get { return this.mAutoClose_; }
			set { this.mAutoClose_ = value; }
		}
		#endregion

		public bool IsValid { get {
			return this.CommandType != BProtoObjectCommandType.INVALID
				&&
				this.Position >= 0
				&&
				this.IsCommandDataValid;
		} }

		public bool IsCommandDataValid { get {
			if (this.CommandType.RequiresValidId())
				return this.Id.IsNotNone();

			if (this.CommandType == BProtoObjectCommandType.CHANGE_MODE)
				return this.SquadMode != BSquadMode.INVALID;

			return true;
		} }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttribute("Position", ref this.mPosition_);

			s.StreamAttributeEnum("Type", ref this.mCommandType_);
			switch (this.mCommandType_)
			{
			case BProtoObjectCommandType.RESEARCH: // proto tech
				xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mId_, DatabaseObjectKind.TECH, false, XML.XmlUtil.K_SOURCE_CURSOR);
				break;
			case BProtoObjectCommandType.TRAIN_UNIT: // proto object
			case BProtoObjectCommandType.BUILD:
			case BProtoObjectCommandType.BUILD_OTHER:
				xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mId_, DatabaseObjectKind.OBJECT, false, XML.XmlUtil.K_SOURCE_CURSOR);
				break;
			case BProtoObjectCommandType.TRAIN_SQUAD: // proto squad
				xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mId_, DatabaseObjectKind.SQUAD, false, XML.XmlUtil.K_SOURCE_CURSOR);
				break;

			case BProtoObjectCommandType.CHANGE_MODE: // unused
				s.StreamCursorEnum(ref this.mSquadMode_);
				break;

			case BProtoObjectCommandType.ABILITY:
				xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mId_, DatabaseObjectKind.ABILITY, false, XML.XmlUtil.K_SOURCE_CURSOR);
				break;
			case BProtoObjectCommandType.POWER:
				xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mId_, DatabaseObjectKind.POWER, false, XML.XmlUtil.K_SOURCE_CURSOR);
				break;
			}

			s.StreamAttributeOpt("AutoClose", ref this.mAutoClose_, Predicates.IsTrue);
		}
		#endregion
	};
}
