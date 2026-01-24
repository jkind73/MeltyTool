
namespace KSoft.Phoenix.Phx
{
	public enum ReticleAttackGrade
	{
		NO_EFFECT,
		WEAK,
		FAIR,
		GOOD,
		EXTREME,

		K_NUMBER_OF
	};

	public enum DamageDirection
	{
		INVALID = TypeExtensions.K_NONE,

		FULL,
		FRONT_HALF,
		BACK_HALF,
		FRONT,
		BACK,
		LEFT,
		RIGHT,

		K_NUMBER_OF
	};

	public sealed class BDamageType
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams("DamageType",
			XML.BCollectionXmlParamsFlags.REQUIRES_DATA_NAME_PRELOADING);
		public static readonly Engine.XmlFileInfo KXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.DATA,
			FileName = "DamageTypes.xml",
			RootName = KBListXmlParams.rootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo KProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.LISTS,
			KXmlFileInfo);
		#endregion

		bool mAttackRating_;
		public bool AttackRating
		{
			get { return this.mAttackRating_; }
			set { this.mAttackRating_ = value; }
		}

		bool mBaseType_;
		public bool BaseType
		{
			get { return this.mBaseType_; }
			set { this.mBaseType_ = value; }
		}

		bool mShielded_;
		/// <remarks>The last type with this set will be the shielded damage type, or anything named "Shielded" will be</remarks>
		public bool Shielded
		{
			get { return this.mShielded_; }
			set { this.mShielded_ = value; }
		}

		#region BListAutoIdObject Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeOpt("AttackRating", ref this.mAttackRating_, Predicates.IsTrue);
			s.StreamAttributeOpt("BaseType", ref this.mBaseType_, Predicates.IsTrue);
			s.StreamAttributeOpt("Shielded", ref this.mShielded_, Predicates.IsTrue);
		}
		#endregion
	};
}