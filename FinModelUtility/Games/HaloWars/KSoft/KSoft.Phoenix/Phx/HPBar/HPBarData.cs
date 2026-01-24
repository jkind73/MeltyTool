using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Phx
{
	[ProtoDataTypeObjectSourceKind(ProtoDataObjectSourceKind.HP_DATA)]
	public sealed class HpBarData
		: IO.ITagElementStringNameStreamable
		, IProtoDataObjectDatabaseProvider
	{
		public ProtoDataObjectDatabase ObjectDatabase { get; private set; }

		#region Xml constants
		const string K_XML_ROOT_ = "HPBarDefinition";

		public static readonly Engine.XmlFileInfo KXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.DATA,
			FileName = "HPBars.xml",
			RootName = K_XML_ROOT_
		};
		public static readonly Engine.ProtoDataXmlFileInfo KProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.GAME_DATA,
			KXmlFileInfo);
		#endregion

		public Collections.BListAutoId<BProtoHpBar> HpBars { get; private set; }
		public Collections.BListAutoId<BProtoHpBarColorStages> ColorStages { get; private set; }
		public Collections.BListAutoId<BProtoVeterancyBar> VeterancyBars { get; private set; }
		public Collections.BListAutoId<BProtoPieProgress> PieProgress { get; private set; }
		public Collections.BListAutoId<BProtoBobbleHead> BobbleHeads { get; private set; }
		public Collections.BListAutoId<BProtoBuildingStrength> BuildingStrengths { get; private set; }

		public HpBarData()
		{
			this.ObjectDatabase = new ProtoDataObjectDatabase(this, typeof(HpBarDataObjectKind));

			this.HpBars = new Collections.BListAutoId<BProtoHpBar>();
			this.ColorStages = new Collections.BListAutoId<BProtoHpBarColorStages>();
			this.VeterancyBars = new Collections.BListAutoId<BProtoVeterancyBar>();
			this.PieProgress = new Collections.BListAutoId<BProtoPieProgress>();
			this.BobbleHeads = new Collections.BListAutoId<BProtoBobbleHead>();
			this.BuildingStrengths = new Collections.BListAutoId<BProtoBuildingStrength>();

			this.InitializeDatabaseInterfaces();
		}

		public void Clear()
		{
			this.HpBars.Clear();
			this.ColorStages.Clear();
			this.VeterancyBars.Clear();
			this.PieProgress.Clear();
			this.BobbleHeads.Clear();
			this.BuildingStrengths.Clear();
		}

		#region Database interfaces
		void InitializeDatabaseInterfaces()
		{
			this.HpBars.SetupDatabaseInterface();
			this.ColorStages.SetupDatabaseInterface();
			this.VeterancyBars.SetupDatabaseInterface();
			this.PieProgress.SetupDatabaseInterface();
			this.BobbleHeads.SetupDatabaseInterface();
			this.BuildingStrengths.SetupDatabaseInterface();
		}

		internal Collections.IBTypeNames GetNamesInterface(HpBarDataObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != HpBarDataObjectKind.NONE);

			switch (kind)
			{
			case HpBarDataObjectKind.HP_BAR:            return this.HpBars;
			case HpBarDataObjectKind.COLOR_STAGES:      return this.ColorStages;
			case HpBarDataObjectKind.VETERANCY_BAR:     return this.VeterancyBars;
			case HpBarDataObjectKind.PIE_PROGRESS:      return this.PieProgress;
			case HpBarDataObjectKind.BOBBLE_HEAD:       return this.BobbleHeads;
			case HpBarDataObjectKind.BUILDING_STRENGTH: return this.BuildingStrengths;

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}

		internal Collections.IHasUndefinedProtoMemberInterface GetMembersInterface(HpBarDataObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != HpBarDataObjectKind.NONE);

			switch (kind)
			{
			case HpBarDataObjectKind.HP_BAR:            return this.HpBars;
			case HpBarDataObjectKind.COLOR_STAGES:      return this.ColorStages;
			case HpBarDataObjectKind.VETERANCY_BAR:     return this.VeterancyBars;
			case HpBarDataObjectKind.PIE_PROGRESS:      return this.PieProgress;
			case HpBarDataObjectKind.BOBBLE_HEAD:       return this.BobbleHeads;
			case HpBarDataObjectKind.BUILDING_STRENGTH: return this.BuildingStrengths;

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}
		#endregion

		#region ITagElementStreamable<string> Members
		/// <remarks>For streaming directly from hpbars.xml</remarks>
		internal void StreamHpBarData<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			XML.XmlUtil.Serialize(s, this.HpBars, BProtoHpBar.KBListXmlParams);
			XML.XmlUtil.Serialize(s, this.ColorStages, BProtoHpBarColorStages.KBListXmlParams);
			XML.XmlUtil.Serialize(s, this.VeterancyBars, BProtoVeterancyBar.KBListXmlParams);
			XML.XmlUtil.Serialize(s, this.PieProgress, BProtoPieProgress.KBListXmlParams);
			XML.XmlUtil.Serialize(s, this.BobbleHeads, BProtoBobbleHead.KBListXmlParams);
			XML.XmlUtil.Serialize(s, this.BuildingStrengths, BProtoBuildingStrength.KBListXmlParams);
		}

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark(K_XML_ROOT_))
				this.StreamHpBarData(s);
		}
		#endregion

		#region IProtoDataObjectDatabaseProvider members
		Engine.XmlFileInfo IProtoDataObjectDatabaseProvider.SourceFileReference { get { return KXmlFileInfo; } }

		Collections.IBTypeNames IProtoDataObjectDatabaseProvider.GetNamesInterface(int objectKind)
		{
			var kind = (HpBarDataObjectKind)objectKind;
			return this.GetNamesInterface(kind);
		}

		Collections.IHasUndefinedProtoMemberInterface IProtoDataObjectDatabaseProvider.GetMembersInterface(int objectKind)
		{
			var kind = (HpBarDataObjectKind)objectKind;
			return this.GetMembersInterface(kind);
		}
		#endregion
	};
}