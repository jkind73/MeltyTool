using Contracts = System.Diagnostics.Contracts;

namespace KSoft.Phoenix.XML
{
	public abstract class BCollectionXmlParams
	{
		/// <summary>Root element name in the XML</summary>
		public /*readonly*/ string rootName;
		/// <summary>Name of the elements, that appear under the root element, and host our values</summary>
		public /*readonly*/ string elementName;

		/// <summary>Do we explicitly filter the XML tags to match <see cref="elementName"/>?</summary>
		public bool UseElementName { get { return this.elementName != null; } }

		#region Flags
		public /*readonly*/ BCollectionXmlParamsFlags flags;

		[Contracts.Pure]
		protected bool HasFlag(BCollectionXmlParamsFlags flag) { return (this.flags & flag) == flag; }

		public void SetForceNoRootElementStreaming(bool isSet)
		{
			if (isSet)
				this.flags |= BCollectionXmlParamsFlags.FORCE_NO_ROOT_ELEMENT_STREAMING;
			else
				this.flags &= ~BCollectionXmlParamsFlags.FORCE_NO_ROOT_ELEMENT_STREAMING;
		}
		#endregion

		public string GetOptionalRootName()
		{
			if (!this.HasFlag(BCollectionXmlParamsFlags.FORCE_NO_ROOT_ELEMENT_STREAMING))
				return this.rootName;

			return null;
		}

		protected BCollectionXmlParams() {}
		/// <summary>Sets RootName to plural of ElementName</summary>
		/// <param name="elementName"></param>
		protected BCollectionXmlParams(string elementName)
		{
			this.rootName = elementName + "s";
			this.elementName = elementName;
		}

		#region IO.TagElementStream util
		protected static void StreamValue<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string valueName, ref string value,
			bool useInnerText, bool useElement, bool internValue, bool toLower)
			where TDoc : class
			where TCursor : class
		{
				 if (useInnerText)		s.StreamCursor(ref value);
			else if (useElement)		s.StreamElement(valueName, ref value);
			else if (valueName != null)	s.StreamAttribute(valueName, ref value);

			if (s.IsReading)
			{
				if (toLower) value = value.ToLowerInvariant();
				if (internValue) value = string.Intern(value);
			}
		}
		protected static void StreamValue<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string valueName, ref int value,
			bool useInnerText, bool useElement)
			where TDoc : class
			where TCursor : class
		{
				 if (useInnerText)		s.StreamCursor(ref value);
			else if (useElement)		s.StreamElement(valueName, ref value);
			else if (valueName != null)	s.StreamAttribute(valueName, ref value);
		}
		#endregion
	};
}