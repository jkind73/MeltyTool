using System.Xml;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	partial class XmlElementStream
	{
		#region ReadElement impl
		protected override string GetInnerText(XmlElement n)
		{
			var textNode = this.GetInnerTextNode(n);
			if (textNode != null)
			{
				this.ReadErrorNode = textNode; // #REVIEW: which is more informative, using the element (n) or text_node?
				// TextNode's actual text
				return textNode.Value;
			}

			return null;
		}

		private XmlNode GetInnerTextNode(XmlElement n)
		{
			if (!n.HasChildNodes)
				return null;

			var textNode = n.LastChild;
			if (textNode.NodeType == XmlNodeType.Text)
				return textNode;

			textNode = n.FirstChild;
			if (textNode.NodeType == XmlNodeType.Text)
				return textNode;

			foreach (XmlNode node in n.ChildNodes)
			{
				if (node.NodeType == XmlNodeType.Text)
					return node;
			}

			return null;
		}
		#endregion

		#region ReadElement
		public override void ReadElementBegin(string name, out XmlElement oldCursor)
		{
			this.ValidateReadPermission();

			XmlElement n = this.Cursor[name];
			if (n == null)
				this.ThrowReadException(new System.Collections.Generic.KeyNotFoundException(
					                        "Element doesn't exist: " + name));

			oldCursor = this.Cursor;
			// update the error state with the node we're about to read from
			this.ReadErrorNode = n;
			this.Cursor = n;
		}
		public override void ReadElementEnd(ref XmlElement oldCursor)
		{
			this.RestoreCursor(ref oldCursor);
		}

		protected override XmlElement GetElement(string name)
		{
			this.ValidateReadPermission();

			XmlElement n = this.Cursor[name];
			if (n == null)
				this.ThrowReadException(new System.Collections.Generic.KeyNotFoundException(
					                        "Element doesn't exist: " + name));

			// update the error state with the node we're about to read from
			this.ReadErrorNode = n;
			return n;
		}
		#endregion

		#region ReadAttribute
		protected override string ReadAttribute(string name)
		{
			this.ValidateReadPermission();

			XmlNode n = this.Cursor.Attributes[name];
			if (n == null)
				this.ThrowReadException(new System.Collections.Generic.KeyNotFoundException(
					                        "Attribute doesn't exist: " + name));

			Contract.Assume(n != null);
			// update the error state with the node we're about to read from
			this.ReadErrorNode = n;
			return n.Value;
		}
		#endregion

		#region ReadElementOpt
		/// <summary>Streams out the InnerText of element <paramref name="name"/></summary>
		/// <param name="name">Element name</param>
		/// <returns></returns>
		protected override string ReadElementOpt(string name)
		{
			this.ValidateReadPermission();

			XmlElement n = this.Cursor[name];
			if (n == null)
				return null;

			// element exists, update the error state with the node we're about to read from
			this.ReadErrorNode = n;

			// NOTE: GetInnerText will probably overwrite ReadErrorNode anyway
			string it = this.GetInnerText(n);

			return !string.IsNullOrEmpty(it)
				? it
				: null;
		}
		#endregion

		#region ReadAttributeOpt
		/// <summary>Streams out the attribute data of <paramref name="name"/></summary>
		/// <param name="name">Attribute name</param>
		/// <returns></returns>
		protected override string ReadAttributeOpt(string name)
		{
			this.ValidateReadPermission();

			XmlNode n = this.Cursor.Attributes[name];
			if (n == null)
				return null;

			// attribute exists, update the error state with the node we're about to read from
			this.ReadErrorNode = n;

			return n.Value;
		}
		#endregion
	};
}