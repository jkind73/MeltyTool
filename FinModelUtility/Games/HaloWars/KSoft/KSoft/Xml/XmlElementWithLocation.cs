using System.Xml;

namespace KSoft.Xml
{
	class XmlElementWithLocation : XmlElement, IXmlLineInfo, Text.ITextLineInfo
	{
		readonly Text.TextLineInfo mLineInfo_;

		internal XmlElementWithLocation(string prefix, string localName, string namespaceUri, XmlDocumentWithLocation document)
			: base(prefix, localName, namespaceUri, document)
		{
			this.mLineInfo_ = document.CurrentLineInfo;
		}

		internal Text.TextLineInfo LineInfo { get { return this.mLineInfo_; } }

		public bool HasLineInfo { get { return this.mLineInfo_.HasLineInfo; } }
		public int LineNumber	{ get { return this.mLineInfo_.LineNumber; } }
		public int LinePosition	{ get { return this.mLineInfo_.LinePosition; } }

		#region IXmlLineInfo Members
		bool IXmlLineInfo.HasLineInfo()	{ return this.mLineInfo_.HasLineInfo; }
		int IXmlLineInfo.LineNumber		{ get { return this.mLineInfo_.LineNumber; } }
		int IXmlLineInfo.LinePosition	{ get { return this.mLineInfo_.LinePosition; } }
		#endregion

		XmlAttributeWithLocation GetAttributeWithLocation(string name)
		{
			var attr = this.Attributes[name];

			return (XmlAttributeWithLocation)attr;
		}

		public Text.TextLineInfo GetAttributeLineInfo(string name)
		{
			var attrWithLocation = this.GetAttributeWithLocation(name);

			return attrWithLocation != null
				? attrWithLocation.LineInfo
				: Text.TextLineInfo.Empty;
		}
	};
}