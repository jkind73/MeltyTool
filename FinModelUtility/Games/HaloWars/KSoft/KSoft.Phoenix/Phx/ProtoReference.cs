using System;
using Contracts = System.Diagnostics.Contracts;

namespace KSoft.Phoenix.Phx.Meta
{
	public interface IProtoDataReferenceAttribute
	{
		ProtoDataObjectSourceKind ObjectSourceKind { get; }
		Type ProtoType { get; }
		string ProtoKindName { get; }
		int ProtoKindId { get; }
	};

	[AttributeUsage(K_VALID_ON, AllowMultiple = false)]
	public abstract class ProtoDataReferenceAttribute
		: Attribute
	{
		public const AttributeTargets K_VALID_ON = 0
			| AttributeTargets.Field
			| AttributeTargets.Property
			| AttributeTargets.Parameter
			| AttributeTargets.ReturnValue
			| AttributeTargets.GenericParameter
			;

		public abstract ProtoDataObjectSourceKind ObjectSourceKind { get; }
		public abstract Type ProtoType { get; }
	};

	/// <summary>For fields in ProtoData that are not actually used in any meaningful way</summary>
	[AttributeUsage(ProtoDataReferenceAttribute.K_VALID_ON, AllowMultiple = false)]
	public sealed class UnusedDataAttribute : Attribute
	{
		public UnusedDataAttribute() { }
		public UnusedDataAttribute(string note) { }
	};

	/// <summary>Localized string reference</summary>
	[AttributeUsage(ProtoDataReferenceAttribute.K_VALID_ON, AllowMultiple=false)]
	public sealed class LocStringReferenceAttribute : Attribute;

	#region ProtoFileReferences
	[AttributeUsage(ProtoDataReferenceAttribute.K_VALID_ON, AllowMultiple=false)]
	public abstract class ProtoFileReferenceAttribute : Attribute
	{
		public abstract string FileExtension { get; }
	};

	/// <summary>DDX reference</summary>
	[AttributeUsage(ProtoDataReferenceAttribute.K_VALID_ON, AllowMultiple=false)]
	public sealed class TextureReferenceAttribute : ProtoFileReferenceAttribute
	{
		public override string FileExtension { get { return "ddx"; } }
	};

	/// <summary>.physics reference</summary>
	[AttributeUsage(ProtoDataReferenceAttribute.K_VALID_ON, AllowMultiple = false)]
	public sealed class PhysicsInfoReferenceAttribute : ProtoFileReferenceAttribute
	{
		public override string FileExtension { get { return "physics"; } }
	};

	/// <summary>.TriggerScript reference</summary>
	[AttributeUsage(ProtoDataReferenceAttribute.K_VALID_ON, AllowMultiple = false)]
	public sealed class TriggerScriptReferenceAttribute : ProtoFileReferenceAttribute
	{
		public override string FileExtension { get { return "triggerscript"; } }
	};

	/// <summary>.vis reference</summary>
	[AttributeUsage(ProtoDataReferenceAttribute.K_VALID_ON, AllowMultiple = false)]
	public sealed class VisualReferenceAttribute : ProtoFileReferenceAttribute
	{
		public override string FileExtension { get { return "vis"; } }
	};
	#endregion

	[AttributeUsage(ProtoDataReferenceAttribute.K_VALID_ON, AllowMultiple = false)]
	public sealed class SoundCueReferenceAttribute : Attribute;

	[AttributeUsage(ProtoDataReferenceAttribute.K_VALID_ON, AllowMultiple = false)]
	public sealed class BAnimTypeReferenceAttribute : Attribute;
	[AttributeUsage(ProtoDataReferenceAttribute.K_VALID_ON, AllowMultiple = false)]
	public sealed class AttachmentTypeReferenceAttribute : Attribute;

	[AttributeUsage(ProtoDataReferenceAttribute.K_VALID_ON, AllowMultiple = false)]
	public sealed class CameraEffectReferenceAttribute : Attribute;

	/// <summary>Reference to an Action in a Tactic</summary>
	[AttributeUsage(ProtoDataReferenceAttribute.K_VALID_ON, AllowMultiple = false)]
	public sealed class BProtoActionReferenceAttribute : Attribute;

	#region GameData
	[AttributeUsage(K_VALID_ON, AllowMultiple = false)]
	public abstract class GameDataObjectReferenceAttribute
		: ProtoDataReferenceAttribute
		, IProtoDataReferenceAttribute
	{
		public override ProtoDataObjectSourceKind ObjectSourceKind { get { return ProtoDataObjectSourceKind.GAME_DATA; } }

		public abstract GameDataObjectKind ProtoKind { get; }

		Type IProtoDataReferenceAttribute.ProtoType { get { return this.ProtoType; } }
		string IProtoDataReferenceAttribute.ProtoKindName { get { return this.ProtoKind.ToString(); } }
		int IProtoDataReferenceAttribute.ProtoKindId { get { return (int) this.ProtoKind; } }
	};

	/// <summary>Cost/Resource type reference</summary>
	[AttributeUsage(K_VALID_ON, AllowMultiple = false)]
	public sealed class ResourceReferenceAttribute : GameDataObjectReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(string); } }
		public override GameDataObjectKind ProtoKind { get { return GameDataObjectKind.COST; } }
	};
	[AttributeUsage(K_VALID_ON, AllowMultiple = false)]
	public sealed class PopulationReferenceAttribute : GameDataObjectReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(string); } }
		public override GameDataObjectKind ProtoKind { get { return GameDataObjectKind.POP; } }
	};
	[AttributeUsage(K_VALID_ON, AllowMultiple = false)]
	public sealed class RateReferenceAttribute : GameDataObjectReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(string); } }
		public override GameDataObjectKind ProtoKind { get { return GameDataObjectKind.RATE; } }
	};
	#endregion

	#region HPBar data
	[AttributeUsage(K_VALID_ON, AllowMultiple=false)]
	public abstract class ProtoHpBarObjectReferenceAttribute
		: ProtoDataReferenceAttribute
		, IProtoDataReferenceAttribute
	{
		public override ProtoDataObjectSourceKind ObjectSourceKind { get { return ProtoDataObjectSourceKind.HP_DATA; } }

		public abstract HpBarDataObjectKind ProtoKind { get; }

		Type IProtoDataReferenceAttribute.ProtoType { get { return this.ProtoType; } }
		string IProtoDataReferenceAttribute.ProtoKindName { get { return this.ProtoKind.ToString(); } }
		int IProtoDataReferenceAttribute.ProtoKindId { get { return (int) this.ProtoKind; } }
	};

	[AttributeUsage(K_VALID_ON, AllowMultiple=false)]
	public sealed class BProtoHpBarReferenceAttribute : ProtoHpBarObjectReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BProtoHpBar); } }
		public override HpBarDataObjectKind ProtoKind { get { return HpBarDataObjectKind.HP_BAR; } }
	};

	[AttributeUsage(K_VALID_ON, AllowMultiple=false)]
	public sealed class BProtoHpBarColorStagesReferenceAttribute : ProtoHpBarObjectReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BProtoHpBarColorStages); } }
		public override HpBarDataObjectKind ProtoKind { get { return HpBarDataObjectKind.COLOR_STAGES; } }
	};

	[AttributeUsage(K_VALID_ON, AllowMultiple=false)]
	public sealed class BProtoVeterancyBarReferenceAttribute : ProtoHpBarObjectReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BProtoVeterancyBar); } }
		public override HpBarDataObjectKind ProtoKind { get { return HpBarDataObjectKind.VETERANCY_BAR; } }
	};

	[AttributeUsage(K_VALID_ON, AllowMultiple=false)]
	public sealed class BProtoPieProgressReferenceAttribute : ProtoHpBarObjectReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BProtoPieProgress); } }
		public override HpBarDataObjectKind ProtoKind { get { return HpBarDataObjectKind.PIE_PROGRESS; } }
	};

	[AttributeUsage(K_VALID_ON, AllowMultiple=false)]
	public sealed class BProtoBobbleHeadReferenceAttribute : ProtoHpBarObjectReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BProtoBobbleHead); } }
		public override HpBarDataObjectKind ProtoKind { get { return HpBarDataObjectKind.BOBBLE_HEAD; } }
	};

	[AttributeUsage(K_VALID_ON, AllowMultiple=false)]
	public sealed class BProtoBuildingStrengthReferenceAttribute : ProtoHpBarObjectReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BProtoBuildingStrength); } }
		public override HpBarDataObjectKind ProtoKind { get { return HpBarDataObjectKind.BUILDING_STRENGTH; } }
	};
	#endregion

	#region Proto data
	[AttributeUsage(K_VALID_ON, AllowMultiple = false)]
	public abstract class ProtoReferenceAttribute
		: ProtoDataReferenceAttribute
		, IProtoDataReferenceAttribute
	{
		public override ProtoDataObjectSourceKind ObjectSourceKind { get { return ProtoDataObjectSourceKind.DATABASE; } }

		public abstract DatabaseObjectKind ProtoKind { get; }

		Type IProtoDataReferenceAttribute.ProtoType { get { return this.ProtoType; } }
		string IProtoDataReferenceAttribute.ProtoKindName { get { return this.ProtoKind.ToString(); } }
		int IProtoDataReferenceAttribute.ProtoKindId { get { return (int) this.ProtoKind; } }
	};

	[AttributeUsage(K_VALID_ON, AllowMultiple=false)]
	public sealed class BAbilityReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BAbility); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.ABILITY; } }
	};

	[AttributeUsage(K_VALID_ON, AllowMultiple=false)]
	public sealed class BCivReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BCiv); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.CIV; } }
	};

	[AttributeUsage(K_VALID_ON, AllowMultiple=false)]
	public sealed class BDamageTypeReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BDamageType); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.DAMAGE_TYPE; } }
	};

	[AttributeUsage(K_VALID_ON, AllowMultiple=false)]
	public sealed class BProtoImpactEffectReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BProtoImpactEffect); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.IMPACT_EFFECT; } }
	};

	[AttributeUsage(K_VALID_ON, AllowMultiple=false)]
	public sealed class BLeaderReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BLeader); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.LEADER; } }
	};

	[AttributeUsage(K_VALID_ON, AllowMultiple=false)]
	public sealed class BProtoObjectReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BProtoObject); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.OBJECT; } }
	};

	[AttributeUsage(K_VALID_ON, AllowMultiple=false)]
	public sealed class ObjectTypeReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return null; } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.OBJECT_TYPE; } }
	};

	[AttributeUsage(K_VALID_ON, AllowMultiple=false)]
	public sealed class BProtoPowerReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BProtoPower); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.POWER; } }
	};

	[AttributeUsage(K_VALID_ON, AllowMultiple=false)]
	public sealed class BProtoSquadReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BProtoSquad); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.SQUAD; } }
	};

	[AttributeUsage(K_VALID_ON, AllowMultiple=false)]
	public sealed class BTacticDataReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BTacticData); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.TACTIC; } }
	};

	[AttributeUsage(K_VALID_ON, AllowMultiple=false)]
	public sealed class BProtoTechReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BProtoTech); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.TECH; } }
	};

	[AttributeUsage(K_VALID_ON, AllowMultiple=false)]
	public sealed class TerrainTileTypeReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(TerrainTileType); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.TERRAIN_TILE_TYPE; } }
	};

	/// <summary>Object or ObjectType</summary>
	[AttributeUsage(K_VALID_ON, AllowMultiple=false)]
	public sealed class UnitReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return null; } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.UNIT; } }
	};

	[AttributeUsage(K_VALID_ON, AllowMultiple=false)]
	public sealed class BUserClassReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BUserClass); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.USER_CLASS; } }
	};

	[AttributeUsage(K_VALID_ON, AllowMultiple=false)]
	public sealed class BWeaponTypeReferenceAttribute : ProtoReferenceAttribute
	{
		public override Type ProtoType { get { return typeof(BWeaponType); } }
		public override DatabaseObjectKind ProtoKind { get { return DatabaseObjectKind.WEAPON_TYPE; } }
	};
	#endregion
}

namespace KSoft.Phoenix
{
	partial class TypeExtensionsPhx
	{
		[Contracts.Pure]
		public static string GetExportContractName(this Phx.Meta.IProtoDataReferenceAttribute attr)
		{
			if (attr == null)
				return null;

			return string.Format("{0}.{1}",
				attr.ObjectSourceKind, attr.ProtoKindName);
		}
	};
}
