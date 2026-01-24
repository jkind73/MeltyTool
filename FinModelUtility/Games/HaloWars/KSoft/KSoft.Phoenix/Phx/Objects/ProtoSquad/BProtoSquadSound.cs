
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoSquadSound
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
		BSquadSoundType mType_ = BSquadSoundType.NONE;
		public BSquadSoundType Type
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

		#region WorldID
		// #NOTE assumes 0 is the World enum "None" member
		const int C_WORLD_ID_NONE_ = 0;

		int mWorldId_ = C_WORLD_ID_NONE_;
		public int WorldId
		{
			get { return this.mWorldId_; }
			set { this.mWorldId_ = value; }
		}
		#endregion

		#region CastingUnitOnly
		bool mCastingUnitOnly_;
		public bool CastingUnitOnly
		{
			get { return this.mCastingUnitOnly_; }
			set { this.mCastingUnitOnly_ = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamCursor(ref this.mSound_);

			if (s.StreamAttributeEnumOpt("Type", ref this.mType_, e => e != BSquadSoundType.NONE))
			{
				// #NOTE Engine, in debug builds, asserts Squad is valid when specified
				xs.StreamDbid(s, "Squad", ref this.mSquadId_, DatabaseObjectKind.SQUAD, xmlSource: XML.XmlUtil.K_SOURCE_ATTR);
				// #NOTE Engine, in debug builds, asserts the world ID is not cWorldIdNone when the World value is defined.
				// It doesn't explicitly parse None, but defaults to None when it doesn't recognize the provided value
				s.StreamProtoEnum("World", ref this.mWorldId_, xs.Database.GameScenarioWorlds, xmlSource: XML.XmlUtil.K_SOURCE_ATTR
					, isOptionalDefaultValue: C_WORLD_ID_NONE_);
				s.StreamAttributeOpt("CastingUnitOnly", ref this.mCastingUnitOnly_, Predicates.IsTrue);
			}
		}
		#endregion
	};
}