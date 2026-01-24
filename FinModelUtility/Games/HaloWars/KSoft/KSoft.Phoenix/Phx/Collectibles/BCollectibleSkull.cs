
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoSkull
		: DatabaseNamedObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "Skull",
			dataName = K_XML_ATTR_NAME,
		};
		#endregion

		#region ObjectDBID
		int mObjectDbid_ = TypeExtensions.K_NONE;
		[Meta.BProtoObjectReference]
		public int ObjectDbid
		{
			get { return this.mObjectDbid_; }
			set { this.mObjectDbid_ = value; }
		}
		#endregion

		public Collections.BListArray<BCollectibleSkullEffect> Effects { get; private set; }

		#region DisplayImageOn
		string mDisplayImageOn_;
		[Meta.TextureReference]
		public string DisplayImageOn
		{
			get { return this.mDisplayImageOn_; }
			set { this.mDisplayImageOn_ = value; }
		}
		#endregion

		#region DisplayImageOff
		string mDisplayImageOff_;
		[Meta.TextureReference]
		public string DisplayImageOff
		{
			get { return this.mDisplayImageOff_; }
			set { this.mDisplayImageOff_ = value; }
		}
		#endregion

		#region DisplayImageLocked
		string mDisplayImageLocked_;
		[Meta.TextureReference]
		public string DisplayImageLocked
		{
			get { return this.mDisplayImageLocked_; }
			set { this.mDisplayImageLocked_ = value; }
		}
		#endregion

		#region Hidden
		bool mHidden_;
		public bool Hidden
		{
			get { return this.mHidden_; }
			set { this.mHidden_ = value; }
		}
		#endregion

		public BProtoSkull()
		{
			var textData = this.CreateDatabaseObjectUserInterfaceTextData();
			textData.HasDescriptionId = true;
			textData.HasDisplayNameId = true;

			this.Effects = new Collections.BListArray<BCollectibleSkullEffect>();
		}

		#region ITagElementStreamable<string> Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttribute("objectdbid", ref this.mObjectDbid_);
			XML.XmlUtil.Serialize(s, this.Effects, BCollectibleSkullEffect.KBListXmlParams);
			s.StreamElementOpt("DisplayImageOn", ref this.mDisplayImageOn_, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("DisplayImageOff", ref this.mDisplayImageOff_, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("DisplayImageLocked", ref this.mDisplayImageLocked_, Predicates.IsNotNullOrEmpty);
			s.StreamElementNamedFlag("Hidden", ref this.mHidden_);
		}
		#endregion
	};
}