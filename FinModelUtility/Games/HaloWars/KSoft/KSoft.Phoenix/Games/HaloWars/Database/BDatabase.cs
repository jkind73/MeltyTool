
namespace KSoft.Phoenix.HaloWars
{
	public sealed partial class BDatabase
		: Phx.BDatabaseBase
	{
		static readonly Collections.CodeEnum<BCodeObjectType> KGameObjectTypes = new Collections.CodeEnum<BCodeObjectType>();
		static readonly Collections.CodeEnum<BCodeProtoObject> KGameProtoObjectTypes = new Collections.CodeEnum<BCodeProtoObject>();
		static readonly Collections.CodeEnum<BScenarioWorld> KGameScenarioWorlds = new Collections.CodeEnum<BScenarioWorld>();

		public override Collections.IProtoEnum GameObjectTypes { get { return KGameObjectTypes; } }
		public override Collections.IProtoEnum GameProtoObjectTypes { get { return KGameProtoObjectTypes; } }
		public override Collections.IProtoEnum GameScenarioWorlds { get { return KGameScenarioWorlds; } }

		[Phx.Meta.BProtoPowerReference]
		public int RepairPowerId { get; private set; }
		[Phx.Meta.BProtoPowerReference]
		public int RallyPointPowerId { get; private set; }
		[Phx.Meta.BProtoPowerReference]
		public int HookRepairPowerId { get; private set; }
		[Phx.Meta.BProtoPowerReference]
		public int UnscOdstDropPowerId { get; private set; }

		public BDatabase(Engine.PhxEngine engine) : base(engine, KGameObjectTypes)
		{
			this.RepairPowerId = this.RallyPointPowerId = this.HookRepairPowerId = this.UnscOdstDropPowerId =
				TypeExtensions.K_NONE;
		}

		internal void SetupDbiDs()
		{
			this.RepairPowerId = this.GetId(Phx.DatabaseObjectKind.POWER, "_Repair");
			this.RallyPointPowerId = this.GetId(Phx.DatabaseObjectKind.POWER, "_RallyPoint");
			this.HookRepairPowerId = this.GetId(Phx.DatabaseObjectKind.POWER, "HookRepair");
			this.UnscOdstDropPowerId = this.GetId(Phx.DatabaseObjectKind.POWER, "UnscOdstDrop");
		}
	};
}