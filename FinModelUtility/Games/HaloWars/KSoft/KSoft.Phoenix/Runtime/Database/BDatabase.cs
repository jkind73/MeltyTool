using System.Collections.Generic;

namespace KSoft.Phoenix.Runtime
{
	sealed partial class BDatabase
		: IO.IEndianStreamSerializable
	{
		public List<string> Civs = []; // max=0x64
		public List<string> Leaders = []; // max=0x12C
		public List<string> Abilities = []; // max=0x3E8
		public List<string> ProtoVisuals = []; // max=0x2710
		public List<string> Models = []; // max=0x2710
		public List<string> Animations = []; // max=0x2710
		public List<string> TerrainEffects = []; // max=0x1F4
		public List<string> ProtoImpactEffects = []; // max=0x1F4
		public List<string> LightEffects = []; // max=0x3E8
		public List<string> ParticleGateways = []; // max=0x3E8
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
		int NumUniqueProtoObjects; // max=0x64
		public List<CondensedListItemValue32<DataTagValue>> Shapes { get; private set; }
		public List<CondensedListItemValue32<DataTagValue>> PhysicsInfo { get; private set; }
		public List<ProtoIcon> ProtoIcons { get; private set; } // max=0x3E8

		public BDatabase()
		{
			this.Civs = [];
			this.Leaders = [];
			this.Abilities = [];
			this.ProtoVisuals = [];
			this.Models = [];
			this.Animations = [];
			this.TerrainEffects = [];
			this.ProtoImpactEffects = [];
			this.LightEffects = [];
			this.ParticleGateways = [];
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
			BSaveGame.StreamCollection(s, this.Civs);
			BSaveGame.StreamCollection(s, this.Leaders);
			BSaveGame.StreamCollection(s, this.Abilities);
			BSaveGame.StreamCollection(s, this.ProtoVisuals);
			BSaveGame.StreamCollection(s, this.Models);
			BSaveGame.StreamCollection(s, this.Animations);
			BSaveGame.StreamCollection(s, this.TerrainEffects);
			BSaveGame.StreamCollection(s, this.ProtoImpactEffects);
			BSaveGame.StreamCollection(s, this.LightEffects);
			BSaveGame.StreamCollection(s, this.ParticleGateways);
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
			BSaveGame.StreamList(s, this.Tactics, kTacticsListInfo);

			s.Stream(ref this.NumUniqueProtoObjects);
			s.StreamSignature((uint) this.NumUniqueProtoObjects);

			BSaveGame.StreamList(s, this.Shapes, kDataTagsListInfo);
			BSaveGame.StreamList(s, this.PhysicsInfo, kDataTagsListInfo);

			BSaveGame.StreamCollection(s, this.ProtoIcons);

			s.StreamSignature(cSaveMarker.DB);
		}
		#endregion
	};
}