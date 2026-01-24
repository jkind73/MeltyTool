using System.Collections.Generic;
using System.IO;

namespace KSoft.Phoenix.Xmb
{
	public struct BinaryDataTreePackedNode
		: IO.IEndianStreamSerializable
	{
		public const int K_SIZE_OF = 2+2+2+1+1;

		public ushort parentIndex;// = ushort.MaxValue;
		public ushort childNodeIndex;// = ushort.MaxValue;
		public ushort nameValueOffset;
		public byte nameValuesCount;
		public byte childNodesCount;

		public bool IsRootNode { get { return this.parentIndex == ushort.MaxValue; } }
		public bool HasNameValuesCountOverflow { get { return this.nameValuesCount == byte.MaxValue; } }
		public bool HasChildNodesCountOverflow { get { return this.childNodesCount == byte.MaxValue; } }

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.parentIndex);
			s.Stream(ref this.childNodeIndex);
			s.Stream(ref this.nameValueOffset);
			s.Stream(ref this.nameValuesCount);
			s.Stream(ref this.childNodesCount);
		}
		#endregion
	};

	public sealed class BinaryDataTreeBuildNode
	{
		public BinaryDataTreeBuildNode parent;
		public List<BinaryDataTreeBuildNode> children;
		// First entry should be the element's name and text
		// Remaining entries are the attribute names and values
		public List<BinaryDataTreeBuildNameValue> nameValues;

		public string NodeName { get {
			var nameValue = this.nameValues[0];
			return nameValue.name;
		} }
		public BinaryDataTreeVariantData NodeVariant { get {
			var nameValue = this.nameValues[0];
			return nameValue.variant;
		} }

		internal void SetParent(BinaryDataTreeDecompiler decompiler, BinaryDataTreePackedNode packedNode)
		{
			if (packedNode.IsRootNode)
			{
				decompiler.rootNode = this;
				this.parent = null;
			}
			else
			{
				this.parent = decompiler.nodes[packedNode.parentIndex];
			}
		}

		internal void SetChildren(BinaryDataTreeDecompiler decompiler, BinaryDataTreePackedNode packedNode, int numChildNodes)
		{
			this.children = new List<BinaryDataTreeBuildNode>(numChildNodes);
			for (int x = 0; x < numChildNodes; x++)
				this.children.Add(decompiler.nodes[packedNode.childNodeIndex + x]);
		}

		internal void SetNameValues(BinaryDataTreeDecompiler decompiler, BinaryDataTreePackedNode packedNode, int numNameValues)
		{
			this.nameValues = new List<BinaryDataTreeBuildNameValue>(numNameValues);
			for (int y = 0; y < numNameValues; y++)
				this.nameValues.Add(new BinaryDataTreeBuildNameValue());

			for (int x = 0; x < this.nameValues.Count; x++)
			{
				int packedNameValueIndex = packedNode.nameValueOffset + x;
				var packedNameValue = decompiler.nameValues[packedNameValueIndex];
				var buildNameValue = this.nameValues[x];

				if (x == (this.nameValues.Count-1))
				{
					if (!packedNameValue.IsLastNameValue)
						throw new InvalidDataException("Expected IsLastNameValue");
				}

				buildNameValue.name = decompiler.ReadName(packedNameValue.NameOffset);
				buildNameValue.variant.Read(decompiler.valueDataPool, packedNameValue);

				if (packedNameValue.HasUnicodeData)
					decompiler.hasUnicodeStrings = true;
			}
		}

		internal void ToXml(BinaryDataTree tree, IO.XmlElementStream s)
		{
			this.AttributesToXml(tree, s);
			this.ChildrenToXml(tree, s);
			this.InnerTextToXml(s);
		}
		void AttributesToXml(BinaryDataTree tree, IO.XmlElementStream s)
		{
			if (this.nameValues == null || this.nameValues.Count <= 1)
				return;

			if (tree.DecompileAttributesWithTypeData)
			{
				using (s.EnterCursorBookmark("Attributes"))
				{
					for (int x = 1; x < this.nameValues.Count; x++)
					{
						var nameValue = this.nameValues[x];

						using (s.EnterCursorBookmark(nameValue.name))
							nameValue.variant.ToStream(s);
					}
				}
			}
			else
			{
				for (int x = 1; x < this.nameValues.Count; x++)
				{
					var nameValue = this.nameValues[x];
					nameValue.variant.ToStreamAsAttribute(nameValue.name, s);
				}
			}
		}
		void ChildrenToXml(BinaryDataTree tree, IO.XmlElementStream s)
		{
			if (this.children == null || this.children.Count == 0)
				return;

			foreach (var child in this.children)
			{
				using (s.EnterCursorBookmark(child.NodeName))
					child.ToXml(tree, s);
			}
		}
		void InnerTextToXml(IO.XmlElementStream s)
		{
			var innerTextVariant = this.NodeVariant;
			if (innerTextVariant.Type == BinaryDataTreeVariantType.NULL)
				return;

			innerTextVariant.ToStream(s);
		}
	};
}