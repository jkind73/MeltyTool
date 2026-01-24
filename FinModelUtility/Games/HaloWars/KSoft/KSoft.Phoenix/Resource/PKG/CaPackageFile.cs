using System;
using System.IO;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Resource.PKG
{
	public enum CaPackageVersion
		: ulong
	{
		ZERO,
		NO_ALIGNMENT,
		USES_ALIGNMENT,

		K_NUMBER_OF
	};

	public struct CaPackageEntry
		: IO.IEndianStreamSerializable
	{
		public const int K_MAX_NAME_LENGTH = 511;

		// Technically a Pascal string with 64-bit length prefix, but I'm not adding 64-bit prefix support just for this one use case :|
		public string name;
		public long offset;
		public long size;

		public int CalculateSerializedSize()
		{
			int size = 0;
			size += sizeof(long);
			size += this.name.Length;
			size += sizeof(long); // Offset
			size += sizeof(long); // Size

			return size;
		}

		public void Serialize(IO.EndianStream s)
		{
			if (s.IsReading)
			{
				long position = s.Reader.BaseStream.Position;
				long nameLength = s.Reader.ReadInt64();
				if (nameLength < 0 || nameLength > K_MAX_NAME_LENGTH)
					throw new InvalidDataException("Invalid name length {0} at offset {1} in {2}".Format(nameLength, position, s.StreamName));
				this.name = s.Reader.ReadString(Memory.Strings.StringStorage.AsciiString, (int)nameLength);
				this.offset = s.Reader.ReadInt64();
				this.size = s.Reader.ReadInt64();
			}
			else if (s.IsWriting)
			{
				s.Writer.Write((long) this.name.Length);
				s.Writer.Write(this.name, Memory.Strings.StringStorage.AsciiString);
				s.Writer.Write(this.offset);
				s.Writer.Write(this.size);
			}
		}
	};

	public sealed class CaPackageFile
		: IO.IEndianStreamSerializable
	{
		public const string K_FILE_EXTENSION = ".pkg";

		[Memory.Strings.StringStorageMarkupAscii]
		public const string K_SIGNATURE = "capack";
		public const ulong K_CURRENT_VERSION = (ulong)CaPackageVersion.USES_ALIGNMENT;
		public const int K_DEFAULT_ALIGNMENT = sizeof(long);
		public const int K_MIN_FILE_ENTRY_COUNT = 1;
		public const int K_HEADER_LENGTH = 0
			+ 6 // kSignature.Length
			+ sizeof(CaPackageVersion)
			+ sizeof(long) // file entry count
			;

		public List<CaPackageEntry> FileEntries { get; private set; }
			= [];

		public long alignment = K_DEFAULT_ALIGNMENT;

		public bool HasEnoughFileEntries { get { return this.FileEntries.Count > K_MIN_FILE_ENTRY_COUNT; } }
		public bool UseAlignment { get { return K_CURRENT_VERSION >= (ulong)CaPackageVersion.USES_ALIGNMENT; } }

		public int CalculateHeaderAndFileChunksSize(CaPackageVersion version)
		{
			int size = 0;
			size += K_HEADER_LENGTH;

			foreach (var entry in this.FileEntries)
			{
				size += entry.CalculateSerializedSize();
			}

			if (version >= CaPackageVersion.USES_ALIGNMENT)
			{
				size += sizeof(long); // Alignment
			}

			return size;
		}

		public void Serialize(IO.EndianStream s)
		{
			s.StreamSignature(K_SIGNATURE, Memory.Strings.StringStorage.AsciiString);

			//s.StreamVersionEnum(ref Version, CaPackageVersion.kNumberOf);
			ulong version = K_CURRENT_VERSION;
			s.Stream(ref version);
			if (version <= 0 || version > K_CURRENT_VERSION)
				IO.VersionMismatchException.Assert(s.Reader, K_CURRENT_VERSION);

			this.SerializeAllocationTable(s);

			if (version >= (ulong)CaPackageVersion.USES_ALIGNMENT)
			{
				s.Stream(ref this.alignment);
			}
		}

		void SerializeAllocationTable(IO.EndianStream s)
		{
			long entriesCount = this.FileEntries.Count;
			s.Stream(ref entriesCount);
			if (entriesCount > 0)
			{
				if (s.IsReading)
				{
					this.FileEntries.Capacity = (int)entriesCount;
					for (int x = 0; x < entriesCount; x++)
					{
						var e = new CaPackageEntry();
						s.Stream(ref e);
						this.FileEntries.Add(e);
					}
				}
				else if (s.IsWriting)
				{
					foreach (var e in this.FileEntries)
					{
						var eCopy = e;
						s.Stream(ref eCopy);
					}
				}
			}

		}

		public byte[] ReadEntryBytes(IO.EndianStream s, CaPackageEntry entry)
		{
			Contract.Requires<ArgumentNullException>(s != null);

			if (entry.offset < 0 || entry.offset > s.BaseStream.Length)
			{
				throw new InvalidOperationException(string.Format(
					"File entry '{0}' offset @{1} is not within length #{2} of file {3}",
					entry.name, entry.offset, s.BaseStream.Length, s.StreamName));
			}

			long endOffset = entry.offset + entry.size;
			if (endOffset < 0 || endOffset > s.BaseStream.Length)
			{
				throw new InvalidOperationException(string.Format(
					"File entry '{0}' @{1} with size #{2} is not within length #{3} of file {4}",
					entry.name, entry.offset, entry.size, s.BaseStream.Length, s.StreamName));
			}

			s.Seek(entry.offset);
			byte[] bytes = new byte[entry.size];
			s.Stream(bytes);

			return bytes;
		}

		public void WriteEntryBytes(IO.EndianStream s, ref CaPackageEntry entry, Stream entryStream)
		{
			Contract.Requires<ArgumentNullException>(s != null);
			Contract.Requires<ArgumentNullException>(entryStream != null);
			Contract.Assume(entry.name.IsNotNullOrEmpty());
			Contract.Assume(entry.offset == 0);
			Contract.Assume(entry.size == 0);

			entry.offset = s.BaseStream.Position;
			entry.size = entryStream.Length;

			entryStream.CopyTo(s.BaseStream);
		}

		public void SetupHeaderAndEntries(CaPackageFileDefinition definition)
		{
			if (definition.alignment != 0)
				this.alignment = definition.alignment;

			foreach (var fileName in definition.FileNames)
			{
				// #TODO
			}
		}

		public static bool VerifyIsPkg(IO.EndianReader s)
		{
			Contract.Requires<InvalidOperationException>(s.BaseStream.CanRead);
			Contract.Requires<InvalidOperationException>(s.BaseStream.CanSeek);

			var baseStream = s.BaseStream;
			if ((baseStream.Length - baseStream.Position) < K_HEADER_LENGTH)
			{
				return false;
			}

			string sig = s.ReadString(Memory.Strings.StringStorage.AsciiString);
			ulong version = s.ReadUInt64();
			long fileEntryCount = s.ReadInt64();

			baseStream.Seek(-K_HEADER_LENGTH, SeekOrigin.Current);

			return sig == K_SIGNATURE
				&& version < (ulong)CaPackageVersion.K_NUMBER_OF
				&& fileEntryCount >= K_MIN_FILE_ENTRY_COUNT;
		}
	};
}

