using System.IO;

namespace KSoft.Phoenix.Resource.ECF
{
	public sealed class EcfFileChunkDefinition
		: IO.ITagElementStringNameStreamable
	{
		public const string K_FILE_EXTENSION = ".ecf_chunk";

		internal int rawChunkIndex = TypeExtensions.K_NONE;

		public EcfFileDefinition Parent { get; private set; }

		public ulong Id { get; private set; }
		public byte AlignmentBit { get; private set; }
			= EcfChunk.K_DEFAULT_ALIGNMENT_BIT;
		/// <summary>Compression to use when baking into an ECF stream</summary>
		public EcfCompressionType CompressionType { get; private set; }

		/// <summary>Should be relative to the Parent's BasePath</summary>
		public string FilePath { get; private set; }
		public byte[] FileBytes { get; private set; }

		public bool HasPossibleFileData { get { return this.FilePath.IsNotNullOrEmpty() || this.FileBytes.IsNotNullOrEmpty(); } }

		#region ResourceFlags
		private uint mResourceFlags_;

		public bool IsContiguous
		{
			get { return Bitwise.Flags.Test(this.mResourceFlags_, 1U<<(ushort)EcfChunkResourceFlags.CONTIGUOUS); }
			set { Bitwise.Flags.Modify(value, ref this.mResourceFlags_, (ushort)1U<<(ushort)EcfChunkResourceFlags.CONTIGUOUS); }
		}

		public bool IsWriteCombined
		{
			get { return Bitwise.Flags.Test(this.mResourceFlags_, 1U<<(ushort)EcfChunkResourceFlags.WRITE_COMBINED); }
			set { Bitwise.Flags.Modify(value, ref this.mResourceFlags_, (ushort)1U<<(ushort)EcfChunkResourceFlags.WRITE_COMBINED); }
		}

		public bool IsDeflateStream
		{
			get { return Bitwise.Flags.Test(this.mResourceFlags_, 1U<<(ushort)EcfChunkResourceFlags.IS_DEFLATE_STREAM); }
			set { Bitwise.Flags.Modify(value, ref this.mResourceFlags_, (ushort)1U<<(ushort)EcfChunkResourceFlags.IS_DEFLATE_STREAM); }
		}

		public bool IsResourceTag
		{
			get { return Bitwise.Flags.Test(this.mResourceFlags_, 1U<<(ushort)EcfChunkResourceFlags.IS_RESOURCE_TAG); }
			set { Bitwise.Flags.Modify(value, ref this.mResourceFlags_, (ushort)1U<<(ushort)EcfChunkResourceFlags.IS_RESOURCE_TAG); }
		}
		#endregion

		public void Initialize(EcfFileDefinition parent, EcfChunk rawChunk, int rawChunkIndex)
		{
			this.rawChunkIndex = rawChunkIndex;

			this.Parent = parent;
			this.Id = rawChunk.entryId;
			this.AlignmentBit = rawChunk.dataAlignmentBit;
			this.CompressionType = rawChunk.CompressionType;
			this.IsContiguous = rawChunk.IsContiguous;
			this.IsWriteCombined = rawChunk.IsWriteCombined;
			this.IsDeflateStream = rawChunk.IsDeflateStream;
			this.IsResourceTag = rawChunk.IsResourceTag;
		}

		internal void SetupRawChunk(EcfChunk rawChunk, int rawChunkIndex)
		{
			this.rawChunkIndex = rawChunkIndex;

			rawChunk.entryId = this.Id;
			rawChunk.dataAlignmentBit = this.AlignmentBit;
			rawChunk.CompressionType = this.CompressionType;
			rawChunk.IsContiguous = this.IsContiguous;
			rawChunk.IsWriteCombined = this.IsWriteCombined;
			rawChunk.IsDeflateStream = this.IsDeflateStream;
			rawChunk.IsResourceTag = this.IsResourceTag;
		}

		public void SetFilePathFromParentNameAndId()
		{
			string fileName = string.Format("{0}_{1}{2}",
			                                 this.Parent.EcfName,
			                                 this.Id.ToString("X8"), K_FILE_EXTENSION);

			this.FilePath = fileName;
		}

		internal void SetFileBytes(byte[] bytes)
		{
			this.FileBytes = bytes;
		}

		#region ITagElementStringNameStreamable
		// #NOTE the attributes here should match the ones used in EcfChunk's code

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var ecfExpander = s.Owner as EcfFileExpander;

			if (s.IsReading)
				this.Parent = (EcfFileDefinition)s.UserData;

			s.StreamAttribute("id", this, obj => this.Id, NumeralBase.HEX);
			s.StreamAttributeOpt("align", this, obj => this.AlignmentBit, b => b != EcfChunk.K_DEFAULT_ALIGNMENT_BIT, NumeralBase.HEX);

			if (s.StreamAttributeEnumOpt("Compression", this, obj => this.CompressionType, e => e != EcfCompressionType.STORED))
			{
				// #NOTE DeflateRaw requires the decompressed size to be known somewhere, and generic ECF files do not store such info
				// Only available in ERAs
				if (this.CompressionType == EcfCompressionType.DEFLATE_RAW)
					s.ThrowReadException(new InvalidDataException(this.CompressionType + " is not supported in this context"));
			}

			if (s.IsReading)
				this.ReadResourceFlags(s);
			else if (s.IsWriting)
				this.WriteResourceFlags(s);

			s.StreamAttributeOpt("Path", this, obj => this.FilePath, Predicates.IsNotNullOrEmpty);

			// Don't try to write the file bytes
			bool tryToSerializeFileBytes = s.IsReading || this.FilePath.IsNullOrEmpty();
			if (ecfExpander != null)
			{
				if (!ecfExpander.expanderOptions.Test(EcfFileExpanderOptions.DONT_SAVE_CHUNKS_TO_FILES))
				{
					tryToSerializeFileBytes = true;
				}
			}

			if (tryToSerializeFileBytes)
			{
				if (!s.StreamCursorBytesOpt(this, obj => this.FileBytes))
				{
					if (this.FilePath.IsNullOrEmpty())
						s.ThrowReadException(new InvalidDataException("Expect Path attribute or file hex bytes"));
				}
			}
		}

		private void ReadResourceFlags<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			bool flag = false;
			if (s.ReadAttributeOpt("IsContiguous", ref flag))
				this.IsContiguous = flag;
			if (s.ReadAttributeOpt("IsWriteCombined", ref flag))
				this.IsWriteCombined = flag;
			if (s.ReadAttributeOpt("IsDeflateStream", ref flag))
				this.IsDeflateStream = flag;
			if (s.ReadAttributeOpt("IsResourceTag", ref flag))
				this.IsResourceTag = flag;
		}
		private void WriteResourceFlags<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (this.mResourceFlags_ == 0)
				return;

			if (this.IsContiguous)
				s.WriteAttribute("IsContiguous", true);
			if (this.IsWriteCombined)
				s.WriteAttribute("IsWriteCombined", true);
			if (this.IsDeflateStream)
				s.WriteAttribute("IsDeflateStream", true);
			if (this.IsResourceTag)
				s.WriteAttribute("IsResourceTag", true);
		}
		#endregion
	};
}