using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	// TODO: change to struct?
	public sealed class BTargetPriority
		: IO.ITagElementStringNameStreamable
		, IEqualityComparer<BTargetPriority>
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "TargetPriority",
		};
		#endregion

		int mUnitTypeId_ = TypeExtensions.K_NONE;
		[Meta.UnitReference]
		public int UnitTypeId { get { return this.mUnitTypeId_; } }

		float mPriority_ = PhxUtil.K_INVALID_SINGLE;
		public float Priority { get { return this.mPriority_; } }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDbid(s, "type", ref this.mUnitTypeId_, DatabaseObjectKind.UNIT, false, XML.XmlUtil.K_SOURCE_ATTR);
			s.StreamCursor(ref this.mPriority_);
		}
		#endregion

		#region IEqualityComparer<BTargetPriority> Members
		public bool Equals(BTargetPriority x, BTargetPriority y)
		{
			return x.UnitTypeId == y.UnitTypeId && x.Priority == y.Priority;
		}

		public int GetHashCode(BTargetPriority obj)
		{
			return obj.UnitTypeId.GetHashCode() ^ obj.Priority.GetHashCode();
		}
		#endregion
	};
}