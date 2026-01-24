using System.Xml;

namespace KSoft.Xml
{
	class XmlAttributeWithLocation : XmlAttribute, IXmlLineInfo, Text.ITextLineInfo
	{
		readonly Text.TextLineInfo mLineInfo_;

		internal XmlAttributeWithLocation(string prefix, string localName, string namespaceUri, XmlDocumentWithLocation document)
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
	};
}