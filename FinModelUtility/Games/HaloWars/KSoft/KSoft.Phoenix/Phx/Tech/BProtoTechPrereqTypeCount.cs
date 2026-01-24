
namespace KSoft.Phoenix.Phx
{
	// TODO: Nothing in HW uses this
	public sealed class BProtoTechPrereqTypeCount
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "TypeCount",
		};
		#endregion

		#region UnitID
		int mUnitId_ = TypeExtensions.K_NONE;
		[Meta.BProtoObjectReference]
		public int UnitId
		{
			get { return this.mUnitId_; }
			set { this.mUnitId_ = value; }
		}
		#endregion

		#region Operator
		BProtoTechTypeCountOperator mOperator_;
		public BProtoTechTypeCountOperator Operator
		{
			get { return this.mOperator_; }
			set { this.mOperator_ = value; }
		}
		#endregion

		#region Count
		int mCount_;
		public int Count
		{
			get { return this.mCount_; }
			set { this.mCount_ = value; }
		}

		public const int C_MAX_COUNT = 2048;
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDbid(s, "unit", ref this.mUnitId_, DatabaseObjectKind.OBJECT, false, XML.XmlUtil.K_SOURCE_ATTR);
			s.StreamAttributeEnumOpt("operator", ref this.mOperator_, e => e != BProtoTechTypeCountOperator.E);
			s.StreamAttributeOpt("count", ref this.mCount_, Predicates.IsNotZero);
		}
		#endregion
	};
}