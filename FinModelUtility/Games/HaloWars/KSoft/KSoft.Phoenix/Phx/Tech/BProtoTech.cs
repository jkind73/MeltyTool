//#define TECH_NEEDS_ToLowerDataNames

namespace KSoft.Phoenix.Phx
{
	/* Deprecated fields:
	 * - type: This attribute is no longer a thing.
	*/

	public sealed class BProtoTech
		: DatabaseIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams("Tech")
		{
			rootName = "TechTree",
			dataName = "name",
			flags = 0
#if TECH_NEEDS_ToLowerDataNames
				| XML.BCollectionXmlParamsFlags.ToLowerDataNames
#endif
				| XML.BCollectionXmlParamsFlags.REQUIRES_DATA_NAME_PRELOADING
				| XML.BCollectionXmlParamsFlags.SUPPORTS_UPDATING
		};
		public static readonly Collections.BListAutoIdParams KBListParams
#if TECH_NEEDS_ToLowerDataNames
			= new Collections.BListAutoIdParams()
		{
			ToLowerDataNames = kBListXmlParams.ToLowerDataNames,
		};
#else
			= null;
#endif

		public static readonly Engine.XmlFileInfo KXmlFileInfo = new Engine.XmlFileInfo
		{
			Location = Engine.ContentStorage.GAME,
			Directory = Engine.GameDirectory.DATA,
			FileName = "Techs.xml",
			RootName = KBListXmlParams.rootName
		};
		public static readonly Engine.XmlFileInfo KXmlFileInfoUpdate = new Engine.XmlFileInfo
		{
			Location = Engine.ContentStorage.UPDATE,
			Directory = Engine.GameDirectory.DATA,
			FileName = "Techs_Update.xml",
			RootName = KBListXmlParams.rootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo KProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.PROTO_DATA,
			KXmlFileInfo,
			KXmlFileInfoUpdate);

		static readonly Collections.CodeEnum<BProtoTechFlags> KFlagsProtoEnum = new Collections.CodeEnum<BProtoTechFlags>();
		static readonly Collections.BBitSetParams KFlagsParams = new Collections.BBitSetParams(() => KFlagsProtoEnum);
		#endregion

		#region Alpha
		BProtoTechAlphaMode mAlphaMode_ = BProtoTechAlphaMode.NONE;
		public BProtoTechAlphaMode AlphaMode
		{
			get { return this.mAlphaMode_; }
			set { this.mAlphaMode_ = value; }
		}
		#endregion

		public Collections.BBitSet Flags { get; private set; }

		#region Icon
		string mIcon_;
		[Meta.TextureReference]
		public string Icon
		{
			get { return this.mIcon_; }
			set { this.mIcon_ = value; }
		}
		#endregion

		#region ResearchCompleteSound
		string mResearchCompleteSound_;
		[Meta.SoundCueReference]
		public string ResearchCompleteSound
		{
			get { return this.mResearchCompleteSound_; }
			set { this.mResearchCompleteSound_ = value; }
		}
		#endregion

		#region ResearchAnim
		string mResearchAnim_;
		[Meta.BAnimTypeReference]
		public string ResearchAnim
		{
			get { return this.mResearchAnim_; }
			set { this.mResearchAnim_ = value; }
		}
		#endregion

		public BProtoTechPrereqs Prereqs { get; private set; }
		public Collections.BListArray<BProtoTechEffect> Effects { get; private set; }

		#region StatsObjectID
		int mStatsObjectId_ = TypeExtensions.K_NONE;
		[Meta.BProtoObjectReference]
		public int StatsObjectId
		{
			get { return this.mStatsObjectId_; }
			set { this.mStatsObjectId_ = value; }
		}
		#endregion

		public bool HasPrereqs { get { return this.Prereqs != null && this.Prereqs.IsNotEmpty; } }

		public BProtoTech() : base(BResource.KBListTypeValuesParams, BResource.KBListTypeValuesXmlParamsCostLowercaseType)
		{
			var textData = this.CreateDatabaseObjectUserInterfaceTextData();
			textData.HasDisplayNameId = true;
			textData.HasRolloverTextId = true;
			textData.HasPrereqTextId = true;

			this.Flags = new Collections.BBitSet(KFlagsParams);
			this.Prereqs = new BProtoTechPrereqs();
			this.Effects = new Collections.BListArray<BProtoTechEffect>();
		}

		#region IXmlElementStreamable Members
		protected override void StreamDbId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			// This isn't always used, nor unique.
			// In fact, the engine doesn't even use it beyond reading it!
			s.StreamElementOpt("DBID", this, obj => obj.DbId, Predicates.IsNotNone);
		}

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			var xs = s.GetSerializerInterface();

			int alpha = (int) this.mAlphaMode_;
			s.StreamAttributeOpt("Alpha", ref alpha, Predicates.IsNotNone);
			this.mAlphaMode_ = (BProtoTechAlphaMode)alpha;

			XML.XmlUtil.Serialize(s, this.Flags, XML.BBitSetXmlParams.KFlagsSansRoot);

			if (s.IsReading)
			{
				using (var bm = s.EnterCursorBookmarkOpt("Status")) if (bm.IsNotNull)
				{
					string statusValue = null;
					s.ReadCursor(ref statusValue);
					if (string.Equals(statusValue, "Unobtainable", System.StringComparison.OrdinalIgnoreCase))
						this.Flags.Set((int)BProtoTechFlags.UNOBTAINABLE);
				}
			}

			s.StreamStringOpt("Icon", ref this.mIcon_, toLower: false, type: XML.XmlUtil.K_SOURCE_ELEMENT);
			s.StreamStringOpt("ResearchCompleteSound", ref this.mResearchCompleteSound_, toLower: false, type: XML.XmlUtil.K_SOURCE_ELEMENT);
			s.StreamStringOpt("ResearchAnim", ref this.mResearchAnim_, toLower: false, type: XML.XmlUtil.K_SOURCE_ELEMENT);
			using (var bm = s.EnterCursorBookmarkOpt("Prereqs", this, v => v.HasPrereqs)) if (bm.IsNotNull)
			{
				this.Prereqs.Serialize(s);
			}
			XML.XmlUtil.Serialize(s, this.Effects, BProtoTechEffect.KBListXmlParams);
			xs.StreamDbid(s, "StatsObject", ref this.mStatsObjectId_, DatabaseObjectKind.OBJECT);
		}
		#endregion
	};
}