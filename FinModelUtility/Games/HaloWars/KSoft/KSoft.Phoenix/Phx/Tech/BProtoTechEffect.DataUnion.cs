#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using Interop = System.Runtime.InteropServices;

namespace KSoft.Phoenix.Phx
{
	partial class BProtoTechEffect
	{
		[Interop.StructLayout(Interop.LayoutKind.Explicit, Size = K_SIZE_OF)]
		struct DataUnion
		{
			internal const int K_SIZE_OF = 12 // type, 1st and 2nd param
				+ 4 // pad
				+ sizeof(ulong) // object reference
				;
			/// <summary>Offset of the first parameter</summary>
			const int K_FIRST_PARAM_ = 4;
			/// <summary>Offset of the second parameter</summary>
			const int K_SECOND_PARAM_ = 8;
			const int K_STRING_PARAM_ = 16;

			[Interop.FieldOffset(0)] public BObjectDataType SubType;
			[Interop.FieldOffset(K_FIRST_PARAM_)] public int ID;
			[Interop.FieldOffset(K_SECOND_PARAM_)] public int ID2;
			[Interop.FieldOffset(K_STRING_PARAM_)] public string StringValue;

			[Interop.FieldOffset(K_FIRST_PARAM_)] public int Cost_Type;
			[Interop.FieldOffset(K_SECOND_PARAM_)] public int Cost_UnitType; // proto object or type ID

			[Interop.FieldOffset(K_FIRST_PARAM_)] public BProtoObjectCommandType CommandType;
			[Interop.FieldOffset(K_SECOND_PARAM_)] public int CommandData;
			[Interop.FieldOffset(K_SECOND_PARAM_)] public BSquadMode CommandDataSM;

			[Interop.FieldOffset(K_FIRST_PARAM_)] public int DmgMod_WeapType;
			[Interop.FieldOffset(K_SECOND_PARAM_)] public int DmgMod_DmgType;

			[Interop.FieldOffset(K_SECOND_PARAM_)] public int TrainLimitType; // proto object or squad ID

			[Interop.FieldOffset(K_FIRST_PARAM_)] public int FromTypeID;
			[Interop.FieldOffset(K_SECOND_PARAM_)] public int ToTypeID;

			[Interop.FieldOffset(K_FIRST_PARAM_)] public BProtoTechEffectSetAgeLevel SetAgeLevel;

			[Interop.FieldOffset(K_STRING_PARAM_)] public string TurretRate_HardpointName;

			[Interop.FieldOffset(K_FIRST_PARAM_)] public BObjectDataIconType Icon_Type;
			[Interop.FieldOffset(K_STRING_PARAM_)] public string Icon_Name;

			[Interop.FieldOffset(K_STRING_PARAM_)] public string HPBar_Name;

			public void Initialize()
			{
				this.SubType = BObjectDataType.INVALID;
				this.ID = this.ID2 = TypeExtensions.K_NONE;
				this.StringValue = null;
			}

			public void StreamCost<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs, bool isResourceOptional = false)
				where TDoc : class
				where TCursor : class
			{
				xs.StreamTypeName(s, "Resource", ref this.Cost_Type, GameDataObjectKind.COST, isResourceOptional, XML.XmlUtil.K_SOURCE_ATTR);
				bool streamedUnitType = xs.StreamDbid(s, "UnitType", ref this.Cost_UnitType, DatabaseObjectKind.OBJECT, true, XML.XmlUtil.K_SOURCE_ATTR);
				// #HACK deal with hand edited data in Halo Wars
				if (!streamedUnitType && s.IsReading)
					xs.StreamDbid(s, "unitType", ref this.Cost_UnitType, DatabaseObjectKind.OBJECT, true, XML.XmlUtil.K_SOURCE_ATTR);
			}
			void StreamCommandData<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs)
				where TDoc : class
				where TCursor : class
			{
				const string attrName = "CommandData";

				switch (this.CommandType)
				{
				case BProtoObjectCommandType.RESEARCH: // proto tech
					xs.StreamDbid(s, attrName, ref this.CommandData, DatabaseObjectKind.TECH, false, XML.XmlUtil.K_SOURCE_ATTR);
					break;
				case BProtoObjectCommandType.TRAIN_UNIT: // proto object
				case BProtoObjectCommandType.BUILD:
				case BProtoObjectCommandType.BUILD_OTHER:
					xs.StreamDbid(s, attrName, ref this.CommandData, DatabaseObjectKind.OBJECT, false, XML.XmlUtil.K_SOURCE_ATTR);
					break;
				case BProtoObjectCommandType.TRAIN_SQUAD: // proto squad
					xs.StreamDbid(s, attrName, ref this.CommandData, DatabaseObjectKind.SQUAD, false, XML.XmlUtil.K_SOURCE_ATTR);
					break;

				case BProtoObjectCommandType.CHANGE_MODE: // unused
					s.StreamAttributeEnum(attrName, ref this.CommandDataSM);
					break;

				case BProtoObjectCommandType.ABILITY:
					xs.StreamDbid(s, attrName, ref this.CommandData, DatabaseObjectKind.ABILITY, false, XML.XmlUtil.K_SOURCE_ATTR);
					break;
				case BProtoObjectCommandType.POWER:
					xs.StreamDbid(s, attrName, ref this.CommandData, DatabaseObjectKind.POWER, false, XML.XmlUtil.K_SOURCE_ATTR);
					break;
				}
			}
			public void StreamCommand<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs)
				where TDoc : class
				where TCursor : class
			{
				// #NOTE engine parses this as "CommandType", but its parser ignores case
				if (s.StreamAttributeEnumOpt("commandType", ref this.CommandType, e => e != BProtoObjectCommandType.INVALID))
					this.StreamCommandData(s, xs);
			}
			public void StreamDamageModifier<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs)
				where TDoc : class
				where TCursor : class
			{
				xs.StreamDbid(s, "WeaponType", ref this.DmgMod_WeapType, DatabaseObjectKind.WEAPON_TYPE, false, XML.XmlUtil.K_SOURCE_ATTR);
				xs.StreamDbid(s, "DamageType", ref this.DmgMod_DmgType, DatabaseObjectKind.DAMAGE_TYPE, false, XML.XmlUtil.K_SOURCE_ATTR);
			}
			public void StreamTrainLimit<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs, DatabaseObjectKind kind)
				where TDoc : class
				where TCursor : class
			{
				// #NOTE engine parses these as "UnitType" and "SquadType", but its parser ignores case

				if (kind == DatabaseObjectKind.OBJECT)
					xs.StreamDbid(s, "unitType", ref this.TrainLimitType, kind, false, XML.XmlUtil.K_SOURCE_ATTR);
				else if (kind == DatabaseObjectKind.SQUAD)
					xs.StreamDbid(s, "squadType", ref this.TrainLimitType, kind, false, XML.XmlUtil.K_SOURCE_ATTR);
			}
			public void StreamIcon<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs)
				where TDoc : class
				where TCursor : class
			{
				// #NOTE engine parses this as "IconType", but its parser ignores case
				s.StreamAttributeEnum("iconType", ref this.Icon_Type);
				// #NOTE engine parses this as "IconType", but its parser ignores case
				s.StreamString("iconName", ref this.Icon_Name, false);
			}
		};

		#region ID variants
		[Meta.BWeaponTypeReference]
		public int WeaponTypeId { get {
			Contract.Requires(this.Type == BProtoTechEffectType.DATA);
			Contract.Requires(this.SubType == BObjectDataType.DAMAGE_MODIFIER);
			return this.mDu_.DmgMod_WeapType;
		} }
		[Meta.BDamageTypeReference]
		public int DamageTypeId { get {
			Contract.Requires(this.Type == BProtoTechEffectType.DATA);
			Contract.Requires(this.SubType == BObjectDataType.DAMAGE_MODIFIER);
			return this.mDu_.DmgMod_DmgType;
		} }

		[Meta.RateReference]
		public int RateId { get {
			Contract.Requires(this.Type == BProtoTechEffectType.DATA);
			Contract.Requires(this.SubType == BObjectDataType.RATE_AMOUNT || this.SubType == BObjectDataType.RATE_MULTIPLIER);
			return this.mDu_.ID;
		} }

		[Meta.PopulationReference]
		public int PopId { get {
			Contract.Requires(this.Type == BProtoTechEffectType.DATA);
			Contract.Requires(this.SubType == BObjectDataType.POP_CAP || this.SubType == BObjectDataType.POP_MAX);
			return this.mDu_.ID;
		} }

		[Meta.BProtoPowerReference]
		public int PowerId { get {
			Contract.Requires(this.Type == BProtoTechEffectType.DATA);
			Contract.Requires(this.SubType == BObjectDataType.POWER_RECHARGE_TIME || this.SubType == BObjectDataType.POWER_USE_LIMIT || this.SubType == BObjectDataType.POWER_LEVEL);
			return this.mDu_.ID;
		} }

		public int TransformUnitId { get {
			Contract.Requires(this.Type == BProtoTechEffectType.TRANSFORM_UNIT);
			return this.mDu_.ToTypeID;
		} }
		public int TransformProtoFromId { get {
			Contract.Requires(this.Type == BProtoTechEffectType.TRANSFORM_PROTO_UNIT || this.Type == BProtoTechEffectType.TRANSFORM_PROTO_SQUAD);
			return this.mDu_.FromTypeID;
		} }
		public int TransformProtoToId { get {
			Contract.Requires(this.Type == BProtoTechEffectType.TRANSFORM_PROTO_UNIT || this.Type == BProtoTechEffectType.TRANSFORM_PROTO_SQUAD);
			return this.mDu_.ToTypeID;
		} }
		[Meta.BProtoObjectReference]
		public int BuildObjectId { get {
			Contract.Requires(this.Type == BProtoTechEffectType.BUILD);
			return this.mDu_.ToTypeID;
		} }
		[Meta.BProtoPowerReference]
		public int GodPowerId { get {
			Contract.Requires(this.Type == BProtoTechEffectType.GOD_POWER);
			return this.mDu_.ID;
		} }
		[Meta.BProtoTechReference]
		public int TechStatusTechId { get {
			Contract.Requires(this.Type == BProtoTechEffectType.TECH_STATUS);
			return this.mDu_.ID;
		} }
		[Meta.BAbilityReference]
		public int AbilityId { get {
			Contract.Requires(this.Type == BProtoTechEffectType.ABILITY);
			return this.mDu_.ID;
		} }
		public int AttachSquadTypeObjectId { get {
			Contract.Requires(this.Type == BProtoTechEffectType.ATTACH_SQUAD);
			return this.mDu_.ID;
		} }
		#endregion
	};
}