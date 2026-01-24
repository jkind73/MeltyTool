using System;
using System.Threading.Tasks;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.Phx
{
	public enum DatabaseLoadState
	{
		NOT_LOADED,
		FAILED,
		PRELOADING,
		PRELOADED,
		LOADING,
		LOADED,

		K_NUMBER_OF
	};

	[ProtoDataTypeObjectSourceKind(ProtoDataObjectSourceKind.DATABASE)]
	public abstract partial class BDatabaseBase
		: ObjectModel.BasicViewModel
		, IDisposable
		, IO.ITagElementStringNameStreamable
		, IProtoDataObjectDatabaseProvider
	{
		public const string K_INVALID_STRING = "BORK BORK BORK";

		public ProtoDataObjectDatabase ObjectDatabase { get; private set; }

		#region Xml constants
		internal static readonly XML.BListXmlParams KObjectTypesXmlParams = new XML.BListXmlParams("ObjectType");
		internal static readonly Engine.XmlFileInfo KObjectTypesXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Phoenix.Engine.GameDirectory.DATA,
			FileName = "ObjectTypes.xml",
			RootName = KObjectTypesXmlParams.rootName
		};
		internal static readonly Engine.ProtoDataXmlFileInfo KObjectTypesProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Phoenix.Engine.XmlFilePriority.LISTS,
			KObjectTypesXmlFileInfo);
		#endregion

		#region LoadState
		DatabaseLoadState mLoadState_ = DatabaseLoadState.NOT_LOADED;
		public DatabaseLoadState LoadState
		{
			get
			{
				lock (this.mLoadStateLockee_)
					return this.mLoadState_;
			}
			set
			{
				lock (this.mLoadStateLockee_)
					this.SetFieldEnum(ref this.mLoadState_, value);
			}
		}

		object mLoadStateLockee_ = new object();
		#endregion

		public Engine.PhxEngine Engine { get; private set; }

		public abstract Collections.IProtoEnum GameObjectTypes { get; }
		public abstract Collections.IProtoEnum GameProtoObjectTypes { get; }
		public abstract Collections.IProtoEnum GameScenarioWorlds { get; }

		#region StringTable stuff
		public LocStringTable EnglishStringTable { get; private set; }
			= [];

		/// <summary>Maps a ID to a bit representing if any data references it somewhere. Only updated on data load</summary>
		public Collections.BitSet ReferencedStringIds { get; private set; }
			= new Collections.BitSet(ushort.MaxValue, fixedLength: false);

		internal void AddStringIdReference(int id)
		{
			if (this.ReferencedStringIds.Length < id)
			{
				int bump = id - this.ReferencedStringIds.Length;
				this.ReferencedStringIds.Length += bump + 16;
			}

			this.ReferencedStringIds[id] = true;
		}
		#endregion

		public BGameData GameData { get; private set; }
			 = new BGameData();
		public HpBarData HpBars { get; private set; }
			 = new HpBarData();

		#region DatabaseObjectKind lists
		// #NOTE place new DatabaseObjectKind code here
		public Collections.BListAutoId<		BDamageType> DamageTypes { get; private set; }
			= new Collections.BListAutoId<	BDamageType>();
		public Collections.BListAutoId<		BProtoImpactEffect> ImpactEffects { get; private set; }
			= new Collections.BListAutoId<	BProtoImpactEffect>();
		public Collections.BListAutoId<		BWeaponType> WeaponTypes { get; private set; }
			= new Collections.BListAutoId<	BWeaponType>();
		public Collections.BListAutoId<		BUserClass> UserClasses { get; private set; }
			= new Collections.BListAutoId<	BUserClass>();
		public Collections.BTypeNamesWithCode ObjectTypes { get; private set; }
		public Collections.BListAutoId<		BAbility> Abilities { get; private set; }
			 = new Collections.BListAutoId<	BAbility>();
		public Collections.BListAutoId<		BProtoObject> Objects { get; private set; }
			= new Collections.BListAutoId<	BProtoObject>(BProtoObject.KBListParams);
		public Collections.BListAutoId<		BProtoSquad> Squads { get; private set; }
			= new Collections.BListAutoId<	BProtoSquad>(BProtoSquad.KBListParams);
		public Collections.BListAutoId<		BProtoPower> Powers { get; private set; }
			= new Collections.BListAutoId<	BProtoPower>();
		public Collections.BListAutoId<		BTacticData> Tactics { get; private set; }
			= new Collections.BListAutoId<	BTacticData>();
		public Collections.BListAutoId<		BProtoTech> Techs { get; private set; }
			= new Collections.BListAutoId<	BProtoTech>(BProtoTech.KBListParams);
		public Collections.BListAutoId<		TerrainTileType> TerrainTileTypes { get; private set; }
			= new Collections.BListAutoId<	TerrainTileType>();
		public Collections.BListAutoId<		BCiv> Civs { get; private set; }
			= new Collections.BListAutoId<	BCiv>();
		public Collections.BListAutoId<		BLeader> Leaders { get; private set; }
			= new Collections.BListAutoId<	BLeader>();
		#endregion

		public Collections.BListArray<		BProtoMergedSquads> MergedSquads { get; private set; }
			= new Collections.BListArray<	BProtoMergedSquads>();
		public BProtoShieldBubbleTypes ShieldBubbleTypes { get; private set; }
			= new BProtoShieldBubbleTypes();

		protected BDatabaseBase(Engine.PhxEngine engine, Collections.IProtoEnum gameObjectTypes)
		{
			this.Engine = engine;

			this.ObjectDatabase = new ProtoDataObjectDatabase(this, typeof(DatabaseObjectKind));

			this.ObjectTypes = new Collections.BTypeNamesWithCode(gameObjectTypes);

			this.InitializeDatabaseInterfaces();
		}

		#region IDisposable Members
		public virtual void Dispose()
		{
			Util.DisposeAndNull(ref this.mTriggerSerializer_);
		}
		#endregion

		#region Database interfaces
		// #NOTE place new DatabaseObjectKind code here

		void InitializeDatabaseInterfaces()
		{
			this.DamageTypes.SetupDatabaseInterface();
			this.ImpactEffects.SetupDatabaseInterface();
			this.WeaponTypes.SetupDatabaseInterface();
			this.UserClasses.SetupDatabaseInterface();
			this.Abilities.SetupDatabaseInterface();
			this.Objects.SetupDatabaseInterface();
			this.Squads.SetupDatabaseInterface();
			this.Tactics.SetupDatabaseInterface();
			this.Techs.SetupDatabaseInterface();
			this.TerrainTileTypes.SetupDatabaseInterface();
			this.Powers.SetupDatabaseInterface();
			this.Civs.SetupDatabaseInterface();
			this.Leaders.SetupDatabaseInterface();
		}

		const int K_OBJECT_ID_IS_OBJECT_TYPE_BIT_MASK_ = 1<<30;
		static void ObjectIdIsObjectTypeBitSet(ref int id)
		{
			id |= K_OBJECT_ID_IS_OBJECT_TYPE_BIT_MASK_;
		}
		static bool ObjectIdIsObjectTypeBitGet(ref int id)
		{
			if ((id & K_OBJECT_ID_IS_OBJECT_TYPE_BIT_MASK_) != 0)
			{
				id &= ~K_OBJECT_ID_IS_OBJECT_TYPE_BIT_MASK_;
				return true;
			}
			return false;
		}

		int TryGetIdUnit(string name)
		{
			int id = this.Objects.TryGetId/*WithUndefined*/(name);

			if (id.IsNone())
			{
				if ((id = this.ObjectTypes.TryGetId(name)).IsNotNone())
					ObjectIdIsObjectTypeBitSet(ref id);
				else
					id = this.Objects.TryGetIdWithUndefined(name);
			}

			return id;
		}
		string TryGetNameUnit(int id)
		{
			if (ObjectIdIsObjectTypeBitGet(ref id))
				return this.ObjectTypes.TryGetNameWithUndefined(id);

			return this.Objects.TryGetNameWithUndefined(id);
		}

		public Collections.IBTypeNames GetNamesInterface(GameDataObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != GameDataObjectKind.NONE);

			return this.GameData.GetNamesInterface(kind);
		}
		public Collections.IBTypeNames GetNamesInterface(HpBarDataObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != HpBarDataObjectKind.NONE);

			return this.HpBars.GetNamesInterface(kind);
		}
		public Collections.IBTypeNames GetNamesInterface(DatabaseObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != DatabaseObjectKind.NONE);

			// #NOTE place new DatabaseObjectKind code here

			switch (kind)
			{
			case DatabaseObjectKind.ABILITY:         return this.Abilities;
			case DatabaseObjectKind.CIV:             return this.Civs;
			case DatabaseObjectKind.DAMAGE_TYPE:      return this.DamageTypes;
			case DatabaseObjectKind.IMPACT_EFFECT:    return this.ImpactEffects;
			case DatabaseObjectKind.LEADER:          return this.Leaders;
			case DatabaseObjectKind.OBJECT:          return this.Objects;
			case DatabaseObjectKind.OBJECT_TYPE:      return this.ObjectTypes;
			case DatabaseObjectKind.POWER:           return this.Powers;
			case DatabaseObjectKind.SQUAD:           return this.Squads;
			case DatabaseObjectKind.TACTIC:          return this.Tactics;
			case DatabaseObjectKind.TECH:            return this.Techs;
			case DatabaseObjectKind.TERRAIN_TILE_TYPE: return this.TerrainTileTypes;
			case DatabaseObjectKind.UNIT:            return null; // #TODO?
			case DatabaseObjectKind.USER_CLASS:       return this.UserClasses;
			case DatabaseObjectKind.WEAPON_TYPE:      return this.WeaponTypes;

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}

		public int GetId(GameDataObjectKind kind, string name)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != GameDataObjectKind.NONE);

			var dbi = this.GameData.GetMembersInterface(kind);
			return dbi.TryGetIdWithUndefined(name);
		}
		public int GetId(HpBarDataObjectKind kind, string name)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != HpBarDataObjectKind.NONE);

			var dbi = this.HpBars.GetMembersInterface(kind);
			return dbi.TryGetIdWithUndefined(name);
		}
		public int GetId(DatabaseObjectKind kind, string name)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != DatabaseObjectKind.NONE);

			// #NOTE place new DatabaseObjectKind code here

			switch (kind)
			{
			case DatabaseObjectKind.ABILITY:         return this.Abilities.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.CIV:             return this.Civs.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.DAMAGE_TYPE:      return this.DamageTypes.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.IMPACT_EFFECT:    return this.ImpactEffects.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.LEADER:          return this.Leaders.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.OBJECT:          return this.Objects.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.OBJECT_TYPE:      return this.ObjectTypes.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.POWER:           return this.Powers.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.SQUAD:           return this.Squads.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.TACTIC:          return this.Tactics.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.TECH:            return this.Techs.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.TERRAIN_TILE_TYPE: return this.TerrainTileTypes.TryGetIdWithUndefined(name);
			// TODO: Should just use the Objects DBI AFAICT
			case DatabaseObjectKind.UNIT:       return this.TryGetIdUnit(name);
			case DatabaseObjectKind.USER_CLASS:  return this.UserClasses.TryGetIdWithUndefined(name);
			case DatabaseObjectKind.WEAPON_TYPE: return this.WeaponTypes.TryGetIdWithUndefined(name);

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}
		public string GetName(GameDataObjectKind kind, int id)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != GameDataObjectKind.NONE);

			IProtoDataObjectDatabaseProvider provider = this.GameData;
			return provider.GetName((int)kind, id);
		}
		public string GetName(HpBarDataObjectKind kind, int id)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != HpBarDataObjectKind.NONE);

			IProtoDataObjectDatabaseProvider provider = this.HpBars;
			return provider.GetName((int)kind, id);
		}
		public string GetName(DatabaseObjectKind kind, int id)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != DatabaseObjectKind.NONE);

			// #NOTE place new DatabaseObjectKind code here

			switch (kind)
			{
			case DatabaseObjectKind.ABILITY:         return this.Abilities.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.CIV:             return this.Civs.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.DAMAGE_TYPE:      return this.DamageTypes.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.IMPACT_EFFECT:    return this.ImpactEffects.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.LEADER:          return this.Leaders.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.OBJECT:          return this.Objects.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.OBJECT_TYPE:      return this.ObjectTypes.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.POWER:           return this.Powers.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.SQUAD:           return this.Squads.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.TACTIC:          return this.Tactics.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.TECH:            return this.Techs.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.TERRAIN_TILE_TYPE: return this.TerrainTileTypes.TryGetNameWithUndefined(id);
			// TODO: Should just use the Objects DBI AFAICT
			case DatabaseObjectKind.UNIT:       return this.TryGetNameUnit(id);
			case DatabaseObjectKind.USER_CLASS:  return this.UserClasses.TryGetNameWithUndefined(id);
			case DatabaseObjectKind.WEAPON_TYPE: return this.WeaponTypes.TryGetNameWithUndefined(id);

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}
		#endregion

		#region IProtoDataObjectDatabaseProvider members
		Engine.XmlFileInfo IProtoDataObjectDatabaseProvider.SourceFileReference { get { return null; } }

		Collections.IBTypeNames IProtoDataObjectDatabaseProvider.GetNamesInterface(int objectKind)
		{
			var kind = (DatabaseObjectKind)objectKind;
			return this.GetNamesInterface(kind);
		}

		Collections.IHasUndefinedProtoMemberInterface IProtoDataObjectDatabaseProvider.GetMembersInterface(int objectKind)
		{
			var kind = (DatabaseObjectKind)objectKind;
			return this.GetNamesInterface/*GetMembersInterface*/(kind);
		}
		#endregion

		XML.BTriggerScriptSerializer mTriggerSerializer_;
		internal void InitializeTriggerScriptSerializer()
		{
			this.mTriggerSerializer_ = new XML.BTriggerScriptSerializer(this.Engine);
		}
		public BTriggerSystem LoadScript(string scriptName, BTriggerScriptType type = BTriggerScriptType.TRIGGER_SCRIPT)
		{
			var ctxt = this.mTriggerSerializer_.StreamTriggerScriptGetContext(FA.Read, type, scriptName);
			var task = Task<bool>.Factory.StartNew((state) => {
				var ctxt = state as XML.BTriggerScriptSerializer.StreamTriggerScriptContext;
				return this.mTriggerSerializer_.TryStreamData(ctxt.FileInfo, FA.Read, this.mTriggerSerializer_.StreamTriggerScript, ctxt);
			}, ctxt);

			return task.Result ? ctxt.Script : null;
		}
		public bool LoadScenarioScripts(string scnrPath)
		{
			var ctxt = this.mTriggerSerializer_.StreamTriggerScriptGetContext(FA.Read, BTriggerScriptType.SCENARIO, scnrPath);
			var task = Task<bool>.Factory.StartNew((state) => {
				var ctxt = state as XML.BTriggerScriptSerializer.StreamTriggerScriptContext;
				return this.mTriggerSerializer_.TryStreamData(ctxt.FileInfo, FA.Read, this.mTriggerSerializer_.LoadScenarioScripts, ctxt);
			}, ctxt);

			return task.Result;
		}

		protected abstract XML.BDatabaseXmlSerializerBase NewXmlSerializer();
		private XML.BDatabaseXmlSerializerBase mXmlSerializer_;

		public bool Preload()
		{
			var xs = this.mXmlSerializer_ = this.NewXmlSerializer();//using (var xs = NewXmlSerializer())
			{
				return xs.Preload();
			}
		}

		public bool Load()
		{
			Contract.Assert(this.mXmlSerializer_ != null);

			var xs = this.mXmlSerializer_;//using (var xs = NewXmlSerializer())
			{
				return xs.Load();
			}
		}

		public bool LoadAllTactics()
		{
			Contract.Assert(this.mXmlSerializer_ != null);

			var xs = this.mXmlSerializer_;//using (var xs = NewXmlSerializer())
			{
				return xs.LoadAllTactics();
			}
		}

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (var xs = this.NewXmlSerializer())
			{
				s.SetSerializerInterface(xs);
				xs.Serialize(s);
				s.SetSerializerInterface(null);
			}
		}
		#endregion
	};
}
