using System;

namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoObjectDamageType
		: IO.ITagElementStringNameStreamable
		, IComparable<BProtoObjectDamageType>
		, IEquatable<BProtoObjectDamageType>
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "DamageType",
		};
		#endregion

		#region DamageType
		int mDamageType_ = TypeExtensions.K_NONE;
		[Meta.BDamageTypeReference]
		public int DamageType
		{
			get { return this.mDamageType_; }
			set { this.mDamageType_ = value; }
		}
		#endregion

		#region Direction
		DamageDirection mDirection_ = DamageDirection.INVALID;
		public DamageDirection Direction
		{
			get { return this.mDirection_; }
			set { this.mDirection_ = value; }
		}
		#endregion

		#region Mode
		BSquadMode mMode_ = BSquadMode.NORMAL;
		public BSquadMode Mode
		{
			get { return this.mMode_; }
			set { this.mMode_ = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mDamageType_, DatabaseObjectKind.DAMAGE_TYPE, isOptional: false, xmlSource: XML.XmlUtil.K_SOURCE_CURSOR);
			s.StreamAttributeEnum("direction", ref this.mDirection_);
			s.StreamAttributeEnumOpt("mode", ref this.mMode_, e => e != BSquadMode.NORMAL);
		}
		#endregion

		#region IComparable Members
		public int CompareTo(BProtoObjectDamageType other)
		{
			if (this.DamageType != other.DamageType)
				this.DamageType.CompareTo(other.DamageType);

			if (this.Direction != other.Direction)
				this.Direction.CompareTo(other.Direction);

			return this.Mode.CompareTo(other.Mode);
		}
		#endregion

		#region IEquatable Members
		public bool Equals(BProtoObjectDamageType other)
		{
			return this.DamageType == other.DamageType
				&&
				this.Direction == other.Direction
				&&
				this.Mode == other.Mode;
		}
		#endregion
	};
}