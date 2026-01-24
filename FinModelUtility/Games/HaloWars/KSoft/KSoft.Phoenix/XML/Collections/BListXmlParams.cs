using Contracts = System.Diagnostics.Contracts;

namespace KSoft.Phoenix.XML
{
	public class BListXmlParams : BCollectionXmlParams
	{
		public /*readonly*/ string dataName;

		#region Flags
		[Contracts.Pure]
		public bool InternDataNames { get { return this.HasFlag(BCollectionXmlParamsFlags.INTERN_DATA_NAMES); } }
		[Contracts.Pure]
		public bool UseInnerTextForData { get { return this.HasFlag(BCollectionXmlParamsFlags.USE_INNER_TEXT_FOR_DATA); } }
		[Contracts.Pure]
		public bool UseElementForData { get { return this.HasFlag(BCollectionXmlParamsFlags.USE_ELEMENT_FOR_DATA); } }
		[Contracts.Pure]
		public bool ToLowerDataNames { get { return this.HasFlag(BCollectionXmlParamsFlags.TO_LOWER_DATA_NAMES); } }
		[Contracts.Pure]
		public bool RequiresDataNamePreloading { get { return this.HasFlag(BCollectionXmlParamsFlags.REQUIRES_DATA_NAME_PRELOADING); } }
		[Contracts.Pure]
		public bool SupportsUpdating { get { return this.HasFlag(BCollectionXmlParamsFlags.SUPPORTS_UPDATING); } }
		[Contracts.Pure]
		public bool DoNotWriteUndefinedData { get { return this.HasFlag(BCollectionXmlParamsFlags.DO_NOT_WRITE_UNDEFINED_DATA); } }
		#endregion

		public BListXmlParams() { }
		/// <summary>Sets RootName to plural of ElementName and sets UseInnerTextForData</summary>
		/// <param name="elementName"></param>
		/// <param name="additionalFlags"></param>
		public BListXmlParams(string elementName, BCollectionXmlParamsFlags additionalFlags = 0) : base(elementName)
		{
			this.flags = additionalFlags;
			this.flags |= BCollectionXmlParamsFlags.USE_INNER_TEXT_FOR_DATA;
		}

		public void StreamDataName<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, ref string name)
			where TDoc : class
			where TCursor : class
		{
			StreamValue(s,
			            this.dataName, ref name,
			            this.UseInnerTextForData,
			            this.UseElementForData,
			            this.InternDataNames,
				false/*ToLowerDataNames*/);
		}
	};
}
