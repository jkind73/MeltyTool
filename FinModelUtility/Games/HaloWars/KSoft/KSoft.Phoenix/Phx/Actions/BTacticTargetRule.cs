using System.Collections.Generic;

using BDamageTypeID = System.Int32;
using BProtoUnitID = System.Int32; // object type or proto unit

namespace KSoft.Phoenix.Phx
{
	// BTargetRule
	public sealed class BTacticTargetRule
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "TargetRule",
			flags = XML.BCollectionXmlParamsFlags.FORCE_NO_ROOT_ELEMENT_STREAMING
		};

		static readonly Collections.CodeEnum<BTargetRuleFlags> KFlagsProtoEnum = new Collections.CodeEnum<BTargetRuleFlags>();
		static readonly Collections.BBitSetParams KFlagsParams = new Collections.BBitSetParams(() => KFlagsProtoEnum);

		static readonly Collections.CodeEnum<BTargetRuleTargetStates> KTargetStatesProtoEnum = new Collections.CodeEnum<BTargetRuleTargetStates>();
		static readonly Collections.BBitSetParams KTargetStatesParams = new Collections.BBitSetParams(() => KTargetStatesProtoEnum);
		static readonly XML.BBitSetXmlParams KTargetStatesXmlParams = new XML.BBitSetXmlParams("TargetState");
		#endregion

		#region Relation
		BRelationType mRelation_ = BRelationType.ENEMY;
		public BRelationType Relation
		{
			get { return this.mRelation_; }
			set { this.mRelation_ = value; }
		}
		#endregion

		#region SquadMode
		BSquadMode mSquadMode_ = BSquadMode.INVALID;
		public BSquadMode SquadMode
		{
			get { return this.mSquadMode_; }
			set { this.mSquadMode_ = value; }
		}

		public bool AutoTargetSquadMode { get; private set; }
		#endregion

		[Meta.BDamageTypeReference]
		public List<BDamageTypeID> DamageTypes { get; private set; } = [];

		[Meta.UnitReference]
		public List<BProtoUnitID> TargetTypes { get; private set; } = [];

		#region ActionID
		int mActionId_ = TypeExtensions.K_NONE;
		[Meta.BProtoActionReference]
		public int ActionId
		{
			get { return this.mActionId_; }
			set { this.mActionId_ = value; }
		}
		#endregion

		public Collections.BBitSet Flags { get; private set; } = new Collections.BBitSet(KFlagsParams);
		public Collections.BBitSet TargetStates { get; private set; } = new Collections.BBitSet(KTargetStatesParams);

		#region AbilityID
		int mAbilityId_ = TypeExtensions.K_NONE;
		[Meta.BAbilityReference]
		public int AbilityId
		{
			get { return this.mAbilityId_; }
			set { this.mAbilityId_ = value; }
		}

		public bool IsOptionalAbility { get; private set; }
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();
			var td = KSoft.Debug.TypeCheck.CastReference<BTacticData>(s.UserData);

			s.StreamElementEnumOpt("Relation", ref this.mRelation_, e => e != BRelationType.ENEMY);
			if (!s.StreamElementEnumOpt("SquadMode", ref this.mSquadMode_, e => e != BSquadMode.INVALID))
				if (s.StreamElementEnumOpt("AutoTargetSquadMode", ref this.mSquadMode_, e => e != BSquadMode.INVALID))
					this.AutoTargetSquadMode = true;

			s.StreamElements("DamageType", this.DamageTypes, xs, XML.BXmlSerializerInterface.StreamDamageType);
			s.StreamElements("TargetType", this.TargetTypes, xs, XML.BXmlSerializerInterface.StreamUnitId);

			td.StreamId(s, "Action", ref this.mActionId_, TacticDataObjectKind.ACTION);

			XML.XmlUtil.Serialize(s, this.Flags, XML.BBitSetXmlParams.KFlagsAreElementNamesThatMeanTrue);
			XML.XmlUtil.Serialize(s, this.TargetStates, KTargetStatesXmlParams);

			if (!xs.StreamDbid(s, "Ability", ref this.mAbilityId_, DatabaseObjectKind.ABILITY))
				this.IsOptionalAbility = xs.StreamDbid(s, "OptionalAbility", ref this.mAbilityId_, DatabaseObjectKind.ABILITY);
		}
		#endregion
	};
}
