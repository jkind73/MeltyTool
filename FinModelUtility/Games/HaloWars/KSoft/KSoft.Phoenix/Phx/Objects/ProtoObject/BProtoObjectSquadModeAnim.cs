using System;

namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoObjectSquadModeAnim
		: IO.ITagElementStringNameStreamable
		, IComparable<BProtoObjectSquadModeAnim>
		, IEquatable<BProtoObjectSquadModeAnim>
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "SquadModeAnim",
		};
		#endregion

		#region Mode
		BSquadMode mMode_;
		public BSquadMode Mode
		{
			get { return this.mMode_; }
			set { this.mMode_ = value; }
		}
		#endregion

		#region AnimType
		string mAnimType_;
		[Meta.BAnimTypeReference]
		public string AnimType
		{
			get { return this.mAnimType_; }
			set { this.mAnimType_ = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnum("Mode", ref this.mMode_);
			s.StreamCursor(ref this.mAnimType_);
		}
		#endregion

		#region IComparable Members
		public int CompareTo(BProtoObjectSquadModeAnim other)
		{
			if (this.Mode != other.Mode)
				this.Mode.CompareTo(other.Mode);

			return this.AnimType.CompareTo(other.AnimType);
		}
		#endregion

		#region IEquatable Members
		public bool Equals(BProtoObjectSquadModeAnim other)
		{
			return this.Mode == other.Mode
				&&
				this.AnimType == other.AnimType;
		}
		#endregion
	};
}