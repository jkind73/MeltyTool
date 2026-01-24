
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoObjectSound
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "Sound",
		};
		#endregion

		#region Sound
		string mSound_;
		public string Sound
		{
			get { return this.mSound_; }
			set { this.mSound_ = value; }
		}
		#endregion

		#region Type
		BObjectSoundType mType_ = BObjectSoundType.NONE;
		public BObjectSoundType Type
		{
			get { return this.mType_; }
			set { this.mType_ = value; }
		}
		#endregion

		#region SquadID
		int mSquadId_ = TypeExtensions.K_NONE;
		[Meta.BProtoSquadReference]
		public int SquadId
		{
			get { return this.mSquadId_; }
			set { this.mSquadId_ = value; }
		}
		#endregion

		#region Action
		string mAction_;
		[Meta.BProtoActionReference]
		public string Action
		{
			get { return this.mAction_; }
			set { this.mAction_ = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamCursor(ref this.mSound_);

			if (s.StreamAttributeEnumOpt("Type", ref this.mType_, e => e != BObjectSoundType.NONE))
			{
				xs.StreamDbid(s, "Squad", ref this.mSquadId_, DatabaseObjectKind.SQUAD, xmlSource: XML.XmlUtil.K_SOURCE_ATTR);
				s.StreamAttributeOpt("Action", ref this.mAction_, Predicates.IsNotNullOrEmpty);
			}
		}
		#endregion
	};
}