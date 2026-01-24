using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Xmb
{
	public enum BinaryDataTreeHeaderSignature : byte
	{
		LITTLE_ENDIAN = 0x3E,
		BIG_ENDIAN = 0xE3,
	};

	public enum BinaryDataTreeSectionId
	{
		NODE_SECTION_INDEX,
		NAME_VALUE_SECTION_INDEX, // 4b align
		NAME_DATA_SECTION_INDEX, // 4b align
		VALUE_DATA_SECTION_INDEX, // 16b align

		K_NUMBER_OF,
	};

	public struct BinaryDataTreeHeader
		: IO.IEndianStreamSerializable
	{
		private const byte K_EXPECTED_SIZE_ = 1+1+1+1+4+4+ (4 * (int)BinaryDataTreeSectionId.K_NUMBER_OF);
		private const byte K_EXPECTED_HEADER_DWORD_COUNT_ = K_EXPECTED_SIZE_ / sizeof(uint);

		public const uint K_SIZE_OF = K_EXPECTED_SIZE_;

		public BinaryDataTreeHeaderSignature signature;
		//private byte mHeaderDwordCount;
		public byte headerCrc8;
		public byte userSectionCount;

		public uint dataCrc32;
		public uint dataSize;

		public uint baseSectionSize0;
		public uint baseSectionSize1;
		public uint baseSectionSize2;
		public uint baseSectionSize3;

		public uint this[BinaryDataTreeSectionId sectionId]
		{
			get
			{
				switch ((int)sectionId)
				{
					case 0: return this.baseSectionSize0;
					case 1: return this.baseSectionSize1;
					case 2: return this.baseSectionSize2;
					case 3: return this.baseSectionSize3;

					default:
						throw new KSoft.Debug.UnreachableException(sectionId.ToString());
				}
			}
			set
			{
				switch ((int)sectionId)
				{
					case 0:
						this.baseSectionSize0 = value;
						break;
					case 1:
						this.baseSectionSize1 = value;
						break;
					case 2:
						this.baseSectionSize2 = value;
						break;
					case 3:
						this.baseSectionSize3 = value;
						break;

					default:
						throw new KSoft.Debug.UnreachableException(sectionId.ToString());
				}
			}
		}

		public Shell.EndianFormat SignatureAsEndianFormat
		{
			get
			{
				return this.signature == BinaryDataTreeHeaderSignature.LITTLE_ENDIAN
					? Shell.EndianFormat.LITTLE
					: Shell.EndianFormat.BIG;
			}
			set
			{
				this.signature = value == Shell.EndianFormat.LITTLE
					? BinaryDataTreeHeaderSignature.LITTLE_ENDIAN
					: BinaryDataTreeHeaderSignature.BIG_ENDIAN;
			}
		}

		public ushort GetCrc16()
		{
			var computer = new Security.Cryptography.Crc16.BitComputer(PhxUtil.kCrc16Definition);
			computer.ComputeBegin();

			computer.Compute((byte) this.signature);
			computer.Compute(K_EXPECTED_HEADER_DWORD_COUNT_);
			computer.Compute(byte.MinValue);
			computer.Compute(this.userSectionCount);

			var byteOrder = this.SignatureAsEndianFormat;

			computer.Compute(byteOrder, this.dataCrc32);
			computer.Compute(byteOrder, this.dataSize);

			computer.Compute(byteOrder, this.baseSectionSize0);
			computer.Compute(byteOrder, this.baseSectionSize1);
			computer.Compute(byteOrder, this.baseSectionSize2);
			computer.Compute(byteOrder, this.baseSectionSize3);

			return computer.ComputeFinish();
		}

		public void Serialize(IO.EndianStream s)
		{
			bool reading = s.IsReading;

			byte signature = (byte) this.signature;

			if (!reading)
				this.headerCrc8 = (byte) this.GetCrc16();

			s.Stream(ref signature);
			s.StreamSignature(K_EXPECTED_HEADER_DWORD_COUNT_);
			s.Stream(ref this.headerCrc8);
			s.Stream(ref this.userSectionCount);

			s.Stream(ref this.dataCrc32);
			s.Stream(ref this.dataSize);

			s.Stream(ref this.baseSectionSize0);
			s.Stream(ref this.baseSectionSize1);
			s.Stream(ref this.baseSectionSize2);
			s.Stream(ref this.baseSectionSize3);

			if (reading)
			{
				this.signature = (BinaryDataTreeHeaderSignature)signature;
			}
		}

		public void Validate()
		{
			var actualCrc = (byte) this.GetCrc16();
			if (actualCrc != this.headerCrc8)
				throw new InvalidDataException(string.Format("Invalid CRC 0x{0}, expected 0x{1}",
					actualCrc.ToString("X2"),
					this.headerCrc8.ToString("X2")));
		}

		public static BinaryDataTreeHeaderSignature PeekSignature(BinaryReader reader)
		{
			Contract.Requires(reader != null);

			var peek = reader.PeekByte();

			switch (peek)
			{
				case (int)BinaryDataTreeHeaderSignature.LITTLE_ENDIAN:
				case (int)BinaryDataTreeHeaderSignature.BIG_ENDIAN:
					return (BinaryDataTreeHeaderSignature)peek;

				default:
					throw new InvalidDataException(peek.ToString("X8"));
			}
		}

		public static Shell.EndianFormat PeekSignatureAsEndianFormat(BinaryReader reader)
		{
			Contract.Requires(reader != null);

			var signature = PeekSignature(reader);

			return signature == BinaryDataTreeHeaderSignature.LITTLE_ENDIAN
				? Shell.EndianFormat.LITTLE
				: Shell.EndianFormat.BIG;
		}
	};

	public struct BinaryDataTreeSectionHeader
		: IO.IEndianStreamSerializable
	{
		public const uint K_SIZE_OF = sizeof(uint) * 3;

		public uint id;
		public uint size;
		public uint offset;

		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.id);
			s.Stream(ref this.size);
			s.Stream(ref this.offset);
		}
	};
}