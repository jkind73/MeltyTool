using System.Collections.Generic;

namespace KSoft.Phoenix.Runtime
{
	sealed partial class BDatabase
		: IO.IEndianStreamSerializable
	{
		public List<string> civs = []; // max=0x64
		public List<string> leaders = []; // max=0x12C
		public List<string> abilities = []; // max=0x3E8
		public List<string> protoVisuals = []; // max=0x2710
		public List<string> models = []; // max=0x2710
		public List<string> animations = []; // max=0x2710
		public List<string> terrainEffects = []; // max=0x1F4
		public List<string> protoImpactEffects = []; // max=0x1F4
		public List<string> lightEffects = []; // max=0x3E8
		public List<string> particleGateways = []; // max=0x3E8
		public List<GenericProtoObjectEntry> GenericProtoObjects { get; private set; } // max=0x4E20
		public List<ProtoSquadEntry> ProtoSquads { get; private set; } // max=0x4E20
		public List<string> ProtoTechs { get; private set; } // max=0x2710
		public List<string> ProtoPowers { get; private set; } // max=0x3E8
		public List<string> ProtoObjects { get; private set; } // max=0x4E20 includes objecttypes
		public List<string> Resources { get; private set; } // max=0xC8
		public List<string> Rates { get; private set; } // max=0xC8
		public List<string> Populations { get; private set; } // max=0xC8
		public List<string> WeaponTypes { get; private set; } // max=0x2710
		public List<string> DamageTypes { get; private set; } // max=0xC8
		public List<TemplateEntry> Templates { get; private set; } // max=0x3E8
		public List<string> AnimTypes { get; private set; } // max=0x3E8
		public List<string> EffectTypes { get; private set; } // max=0x7D0
		public List<string> Actions { get; private set; } // max=0xFA
		public List<CondensedListItem16<Tactic>> Tactics { get; private set; }
		int numUniqueProtoObjects_; // max=0x64
		public List<CondensedListItemValue32<DataTagValue>> Shapes { get; private set; }
		public List<CondensedListItemValue32<DataTagValue>> PhysicsInfo { get; private set; }
		public List<ProtoIcon> ProtoIcons { get; private set; } // max=0x3E8

		public BDatabase()
		{
			this.civs = [];
			this.leaders = [];
			this.abilities = [];
			this.protoVisuals = [];
			this.models = [];
			this.animations = [];
			this.terrainEffects = [];
			this.protoImpactEffects = [];
			this.lightEffects = [];
			this.particleGateways = [];
			this.GenericProtoObjects = [];
			this.ProtoSquads = [];
			this.ProtoTechs = [];
			this.ProtoPowers = [];
			this.ProtoObjects = [];
			this.Resources = [];
			this.Rates = [];
			this.Populations = [];
			this.WeaponTypes = [];
			this.DamageTypes = [];
			this.Templates = [];
			this.AnimTypes = [];
			this.EffectTypes = [];
			this.Actions = [];
			this.Tactics = [];
			this.Shapes = [];
			this.PhysicsInfo = [];
			this.ProtoIcons = [];
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamCollection(s, this.civs);
			BSaveGame.StreamCollection(s, this.leaders);
			BSaveGame.StreamCollection(s, this.abilities);
			BSaveGame.StreamCollection(s, this.protoVisuals);
			BSaveGame.StreamCollection(s, this.models);
			BSaveGame.StreamCollection(s, this.animations);
			BSaveGame.StreamCollection(s, this.terrainEffects);
			BSaveGame.StreamCollection(s, this.protoImpactEffects);
			BSaveGame.StreamCollection(s, this.lightEffects);
			BSaveGame.StreamCollection(s, this.particleGateways);
			BSaveGame.StreamCollection(s, this.GenericProtoObjects);
			BSaveGame.StreamCollection(s, this.ProtoSquads);
			BSaveGame.StreamCollection(s, this.ProtoTechs);
			BSaveGame.StreamCollection(s, this.ProtoPowers);
			BSaveGame.StreamCollection(s, this.ProtoObjects);
			BSaveGame.StreamCollection(s, this.Resources);
			BSaveGame.StreamCollection(s, this.Rates);
			BSaveGame.StreamCollection(s, this.Populations);
			BSaveGame.StreamCollection(s, this.WeaponTypes);
			BSaveGame.StreamCollection(s, this.DamageTypes);
			BSaveGame.StreamCollection(s, this.Templates);
			BSaveGame.StreamCollection(s, this.AnimTypes);
			BSaveGame.StreamCollection(s, this.EffectTypes);
			BSaveGame.StreamCollection(s, this.Actions);
			BSaveGame.StreamList(s, this.Tactics, KTacticsListInfo);

			s.Stream(ref this.numUniqueProtoObjects_);
			s.StreamSignature((uint) this.numUniqueProtoObjects_);

			BSaveGame.StreamList(s, this.Shapes, KDataTagsListInfo);
			BSaveGame.StreamList(s, this.PhysicsInfo, KDataTagsListInfo);

			BSaveGame.StreamCollection(s, this.ProtoIcons);

			s.StreamSignature(CSaveMarker.DB);
		}
		#endregion
	};
}