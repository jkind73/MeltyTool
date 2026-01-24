using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Xmb
{
	// #TODO refactor these APIs

	public sealed class BinaryDataTree
		: IO.IEndianStreamSerializable
	{
		public const string K_BINARY_FILE_EXTENSION = ".binary_data_tree";
		public const string K_TEXT_FILE_EXTENSION = ".binary_data_tree_xml";

		BinaryDataTreeHeader mHeader_;

		internal BinaryDataTreeDecompiler Decompiler { get; private set; }
		public bool DecompileAttributesWithTypeData { get; set; }

		public bool ValidateData { get; set; } = true;

		public Shell.EndianFormat Endian
		{
			get { return this.mHeader_.SignatureAsEndianFormat; }
			set { this.mHeader_.SignatureAsEndianFormat = value; }
		}

		#region IEndianStreamable Members
		public void Serialize(IO.EndianStream s)
		{
			if (s.IsReading)
				this.Endian = BinaryDataTreeHeader.PeekSignatureAsEndianFormat(s.Reader);

			using (s.BeginEndianSwitch(this.Endian))
			{
				if (s.IsReading)
					this.ReadInternal(s);
				else if (s.IsWriting)
					this.WriteInternal(s);
			}
		}

	private void ReadInternal(IO.EndianStream s)
		{
			long streamLength = s.BaseStream.Length - s.BaseStream.Position;

			#region Header
			if (this.ValidateData)
			{
				if (streamLength < BinaryDataTreeHeader.K_SIZE_OF)
					throw new InvalidDataException("Expected more bytes for header data");
			}

			this.mHeader_.Serialize(s);

			if (this.ValidateData)
			{
				this.mHeader_.Validate();

				long minExpectedBytesRemaining =
					BinaryDataTreeHeader.K_SIZE_OF +
					(BinaryDataTreeSectionHeader.K_SIZE_OF * this.mHeader_.userSectionCount);
				if (streamLength < minExpectedBytesRemaining)
					throw new InvalidDataException("Expected more bytes for header and user sections data");
			}
			#endregion

			#region Data
			long dataPosition = s.BaseStream.Position;
			if (this.ValidateData)
			{
				long totalSize = BinaryDataTreeHeader.K_SIZE_OF + this.mHeader_.dataSize;
				if (streamLength < totalSize)
					throw new InvalidDataException("Expected more bytes for header and payload data");

				uint actualDataCrc = this.GetDataCrc32(s.BaseStream);
				if (this.mHeader_.dataCrc32 != actualDataCrc)
					throw new InvalidDataException(string.Format("Invalid Data CRC 0x{0}, expected 0x{1}",
						actualDataCrc.ToString("X8"),
						this.mHeader_.dataCrc32.ToString("X8")));
			}
			#endregion

			#region Sections
			var sectionHeaders = new BinaryDataTreeSectionHeader[this.mHeader_.userSectionCount];
			s.StreamArray(sectionHeaders);

			if (this.ValidateData)
			{
				foreach (var header in sectionHeaders)
				{
					if (streamLength < (header.offset + header.size))
						throw new InvalidDataException("Expected more bytes for section data");
				}
			}

			long offsetCursor = dataPosition;

			uint nodesSize = this.mHeader_[BinaryDataTreeSectionId.NODE_SECTION_INDEX];
			long nodesOffset = nodesSize > 0
				? offsetCursor
				: 0;
			offsetCursor += nodesSize;

			uint nameValuesSize = this.mHeader_[BinaryDataTreeSectionId.NAME_VALUE_SECTION_INDEX];
			long nameValuesOffset = nameValuesSize > 0
				? offsetCursor
				: 0;
			offsetCursor += nameValuesSize;

			uint nameDataSize = this.mHeader_[BinaryDataTreeSectionId.NAME_DATA_SECTION_INDEX];
			long nameDataOffset = nameDataSize > 0
				? offsetCursor
				: 0;
			offsetCursor += nameDataSize;

			if (this.mHeader_[BinaryDataTreeSectionId.VALUE_DATA_SECTION_INDEX] > 0)
				offsetCursor = IntegerMath.Align(IntegerMath.K16_BYTE_ALIGNMENT_BIT, offsetCursor);
			uint valueDataSize = this.mHeader_[BinaryDataTreeSectionId.VALUE_DATA_SECTION_INDEX];
			long valueDataOffset = valueDataSize > 0
				? offsetCursor
				: 0;
			offsetCursor += valueDataSize;

			if (this.ValidateData)
			{
				if (streamLength < offsetCursor)
					throw new InvalidDataException("Expected more bytes for section data");
			}
			#endregion

			#region Decompiler

			this.Decompiler = new BinaryDataTreeDecompiler();

			s.Seek(nameDataOffset);
			this.Decompiler.nameData = new byte[nameDataSize];
			s.Stream(this.Decompiler.nameData);

			s.Seek(valueDataOffset);
			this.Decompiler.valueData = new byte[valueDataSize];
			s.Stream(this.Decompiler.valueData);

			s.Seek(nodesOffset);
			this.Decompiler.packedNodes = new BinaryDataTreePackedNode[nodesSize / BinaryDataTreePackedNode.K_SIZE_OF];
			s.StreamArray(this.Decompiler.packedNodes);

			s.Seek(nameValuesOffset);
			this.Decompiler.nameValues = new BinaryDataTreeNameValue[nameValuesSize / BinaryDataTreeNameValue.K_SIZE_OF];
			s.StreamArray(this.Decompiler.nameValues);

			this.Decompiler.Decompile();
			#endregion
		}

		private void WriteInternal(IO.EndianStream s)
		{
			long headerPosition = s.BaseStream.Position;
			this.mHeader_.Serialize(s);

			// #TODO

			s.Seek(headerPosition);
			this.mHeader_.Serialize(s);

			throw new NotImplementedException();
		}

		private uint GetDataCrc32(Stream s)
		{
			var crcHasher = new Security.Cryptography.CrcHash32(PhxUtil.kCrc32Definition);
			var streamCrcHashComputer = new Security.Cryptography.StreamHashComputer<Security.Cryptography.CrcHash32>(crcHasher, s, restorePosition: true);
			streamCrcHashComputer.SetRangeAtCurrentOffset(this.mHeader_.dataSize);
			uint actualDataCrc = streamCrcHashComputer.Compute().Hash32;
			return actualDataCrc;
		}
		#endregion

		#region ToXml
		public XmlDocument ToXmlDocument()
		{
			if (this.Decompiler != null)
				return this.Decompiler.ToXmlDocument(this);

			throw new InvalidOperationException();
		}

		public void ToXml(string file)
		{
			Contract.Requires(!string.IsNullOrEmpty(file));

			using (var fs = File.Create(file))
			{
				this.ToXml(fs);
			}
		}
		public void ToXml(Stream stream)
		{
			Contract.Requires(stream != null);

			var doc = this.ToXmlDocument();

			var encoding = this.Decompiler.hasUnicodeStrings
				? System.Text.Encoding.UTF8
				: System.Text.Encoding.ASCII;
			var xmlWriterSettings = new XmlWriterSettings()
			{
				Indent = true,
				IndentChars = "\t",
				CloseOutput = false,
				Encoding = encoding,
			};
			using (var xml = XmlWriter.Create(stream, xmlWriterSettings))
			{
				doc.Save(xml);
			}
		}
		#endregion
	};

	sealed class BinaryDataTreeDecompiler
	{
		public List<BinaryDataTreeBuildNode> nodes;
		public BinaryDataTreeBuildNode rootNode;
		public bool hasUnicodeStrings;

		public BinaryDataTreePackedNode[] packedNodes;
		public BinaryDataTreeNameValue[] nameValues;
		public byte[] nameData;
		public byte[] valueData;

		public IO.EndianReader nameDataReader;
		public BinaryDataTreeMemoryPool valueDataPool;

		public void Decompile()
		{
			this.nameDataReader = new IO.EndianReader(
				new MemoryStream(this.nameData, writable: false),
				Shell.EndianFormat.LITTLE,
				name: "NameDataReader");

			this.valueDataPool = new BinaryDataTreeMemoryPool(this.valueData);

			this.nodes = new List<BinaryDataTreeBuildNode>(this.packedNodes.Length);
			for (int x = 0; x < this.packedNodes.Length; x++)
				this.nodes.Add(new BinaryDataTreeBuildNode());

			for (int x = 0; x < this.packedNodes.Length; x++)
			{
				var packedNode = this.packedNodes[x];
				var buildNode = this.nodes[x];

				buildNode.SetParent(this, packedNode);

				int numChildNodes;
				this.CalculateChildNodesCount(x, out numChildNodes);
				buildNode.SetChildren(this, packedNode, numChildNodes);

				int numNameValues;
				this.CalculateNameValuesCount(x, out numNameValues);
				if (numNameValues == 0)
					throw new InvalidDataException("No name-values: #" + x);
				buildNode.SetNameValues(this, packedNode, numNameValues);
			}

			Util.DisposeAndNull(ref this.nameDataReader);
		}

		public string ReadName(int nameOffset)
		{
			if (this.nameData == null || nameOffset >= this.nameData.Length)
				throw new InvalidOperationException(nameOffset.ToString("X8"));

			this.nameDataReader.Seek(nameOffset);
			return this.nameDataReader.ReadString(Memory.Strings.StringStorage.CStringAscii);
		}

		private void CalculateChildNodesCount(int nodeIndex, out int numChildNodes)
		{
			var packedNode = this.packedNodes[nodeIndex];

			numChildNodes = packedNode.childNodesCount;
			if (!packedNode.HasChildNodesCountOverflow)
				return;

			for (int childNodeIndex = packedNode.childNodeIndex; ; numChildNodes++)
			{
				if ((childNodeIndex + numChildNodes) > this.nodes.Count)
					throw new InvalidDataException();
				else if ((childNodeIndex + numChildNodes) == this.nodes.Count)
					break;

				var childNode = this.packedNodes[childNodeIndex + numChildNodes];
				if (childNode.parentIndex != nodeIndex)
					break;
			}
		}

		private void CalculateNameValuesCount(int nodeIndex, out int numNameValues)
		{
			var packedNode = this.packedNodes[nodeIndex];

			numNameValues = packedNode.nameValuesCount;
			if (!packedNode.HasNameValuesCountOverflow)
				return;

			for (int nameValueIndex = packedNode.nameValueOffset; ; numNameValues++)
			{
				if ((nameValueIndex + numNameValues) >= numNameValues)
					throw new InvalidDataException();

				var nameValue = this.nameValues[nameValueIndex + numNameValues];
				if (nameValue.IsLastNameValue)
					break;
			}
		}

		public XmlDocument ToXmlDocument(BinaryDataTree tree)
		{
			Contract.Requires(this.rootNode != null);
			Contract.Ensures(Contract.Result<XmlDocument>() != null);

			string rootName = this.rootNode.NodeName;
			var s = IO.XmlElementStream.CreateForWrite(rootName);
			this.rootNode.ToXml(tree, s);

			return s.Document;
		}
	};
}