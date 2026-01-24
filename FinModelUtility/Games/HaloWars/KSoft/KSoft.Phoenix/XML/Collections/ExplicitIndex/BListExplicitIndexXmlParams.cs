
namespace KSoft.Phoenix.XML
{
	public class BListExplicitIndexXmlParams<T> : BListXmlParams
	{
		/// <summary>The index base offset as it appears in the XML</summary>
		/// <example>If this is 1, then the XML values are 1, 2, 3, etc.</example>
		/// <remarks>In-memory, everything is always at base-0</remarks>
		public int indexBase = 1;

		public BListExplicitIndexXmlParams() { }
		/// <summary>Sets ElementName and sets DataName (defaults to attribute usage)</summary>
		/// <param name="elementName"></param>
		/// <param name="indexName"></param>
		public BListExplicitIndexXmlParams(string elementName, string indexName) : base(elementName)
		{
			this.rootName = null;
			this.dataName = indexName;
			this.flags = 0;
		}

		public void StreamExplicitIndex<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, ref int index)
			where TDoc : class
			where TCursor : class
		{
			// 'rebase' the index to how the XML defs expect it
			if (s.IsWriting)
				index += this.indexBase;

			StreamValue(s,
			            this.dataName, ref index,
			            this.UseInnerTextForData,
			            this.UseElementForData);

			// Undo any rebasing
			/*if (s.IsReading)*/ index -= this.indexBase;
		}
	};
}
