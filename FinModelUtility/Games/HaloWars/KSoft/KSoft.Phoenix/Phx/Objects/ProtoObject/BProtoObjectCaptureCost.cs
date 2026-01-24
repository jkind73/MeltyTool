
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoObjectCaptureCost
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "CaptureCost",
		};
		#endregion

		#region CivID
		int mCivId_ = TypeExtensions.K_NONE;
		[Meta.BCivReference]
		public int CivId
		{
			get { return this.mCivId_; }
			set { this.mCivId_ = value; }
		}
		#endregion

		#region ResourceType
		int mResourceType_ = TypeExtensions.K_NONE;
		[Meta.ResourceReference]
		public int ResourceType
		{
			get { return this.mResourceType_; }
			set { this.mResourceType_ = value; }
		}
		#endregion

		#region Cost
		float mCost_;
		public float Cost
		{
			get { return this.mCost_; }
			set { this.mCost_ = value; }
		}
		#endregion

		public bool AppliesToAllCivs { get { return this.CivId.IsNone(); } }
		/// <summary>Does the engine not ignore the XML data of this bit?</summary>
		public bool IsNotIgnored { get {
			return this.ResourceType >= 0
				&&
				this.Cost != 0.0f;
		} }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDbid(s, "Civ", ref this.mCivId_, DatabaseObjectKind.CIV, xmlSource: XML.XmlUtil.K_SOURCE_ATTR);

			if (!xs.StreamTypeName(s, BResource.KBListTypeValuesXmlParamsCost.dataName, ref this.mResourceType_, GameDataObjectKind.COST, isOptional: false, xmlSource: XML.XmlUtil.K_SOURCE_ATTR))
				s.ThrowReadException(new System.IO.InvalidDataException(string.Format(
					"ProtoObject's {0} XML doesn't define a {1}",
					KBListXmlParams.elementName, BResource.KBListTypeValuesXmlParamsCost.dataName)));

			s.StreamCursor(ref this.mCost_);
		}
		#endregion
	};
}