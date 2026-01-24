using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using Interop = System.Runtime.InteropServices;

namespace KSoft.Phoenix.Xmb
{
	using BitFieldTraits = Bitwise.BitFieldTraits;
	using BitEncoders = TypeExtensionsPhx.BitEncoders;

	[Interop.StructLayout(Interop.LayoutKind.Explicit, Size=K_SIZE_OF)]
	/*public*/ struct BinaryDataTreeNameValue
		: IO.IEndianStreamSerializable
	{
		#region Constants
		public const int K_SIZE_OF = 8;

		const int K_MAX_DIRECT_ENCODED_STRING_LENGTH_ = 4;

		const int K_SIZE_BIT_COUNT_ = 7;

		// nesting these into a static class makes them run before the struct's static ctor...
		// which, being a value type cctor, may not run when we want it
		static class Constants
		{
			// 0
			public static readonly BitFieldTraits KTypeIsUnsignedBitField =
				new BitFieldTraits(Bits.K_BOOLEAN_BIT_COUNT);
			// 1
			public static readonly BitFieldTraits KDirectEncodingBitField =
				new BitFieldTraits(Bits.K_BOOLEAN_BIT_COUNT, KTypeIsUnsignedBitField);
			// 2
			public static readonly BitFieldTraits KTypeBitField =
				new BitFieldTraits(BitEncoders.BinaryDataTreeVariantType.BitCountTrait, KDirectEncodingBitField);
			// 5
			public static readonly BitFieldTraits KTypeSizeInBytesLog2BitField =
				new BitFieldTraits(BitEncoders.BinaryDataTreeVariantTypeSizeInBytes.BitCountTrait, KTypeBitField);
			// 8
			public static readonly BitFieldTraits KIsLastNameValueBitField =
				new BitFieldTraits(Bits.K_BOOLEAN_BIT_COUNT, KTypeSizeInBytesLog2BitField);
			// 9
			public static readonly BitFieldTraits KSizeBitField =
				new BitFieldTraits(K_SIZE_BIT_COUNT_, KIsLastNameValueBitField);

			public static readonly BitFieldTraits KLastBitField =
				KSizeBitField;
		};

		/// <summary>Number of bits required to represent a bit-encoded representation of this value type</summary>
		public static int BitCount { get { return Constants.KLastBitField.FieldsBitCount; } }
		public static uint Bitmask { get { return Constants.KLastBitField.FieldsBitmask.u32; } }

		public const uint K_INDIRECT_SIZE_THRESHOLD = 1U << K_SIZE_BIT_COUNT_;
		#endregion

		public static BinaryDataTreeNameValue Empty { get { return new BinaryDataTreeNameValue() { Type = BinaryDataTreeVariantType.NULL }; } }

		public override string ToString()
		{
			return string.Format("0x{0} 0x{1} 0x{2}",
			                     this.NameOffset.ToString("X4"),
			                     this.Flags.ToString("X4"),
			                     this.Int.ToString("X8"));
		}

		#region Properties
		[Interop.FieldOffset(4)]
		public ushort NameOffset;
		[Interop.FieldOffset(6)]
		public ushort Flags;

		public bool IsUnsigned
		{
			get { return Bits.BitDecode(this.Flags, Constants.KTypeIsUnsignedBitField).ToBoolean(); }
			set { this.Flags = Bits.BitEncode(value.ToUInt16(), this.Flags, Constants.KTypeIsUnsignedBitField); }
		}

		public bool DirectEncoding
		{
			get { return Bits.BitDecode(this.Flags, Constants.KDirectEncodingBitField).ToBoolean(); }
			set { this.Flags = Bits.BitEncode(value.ToUInt16(), this.Flags, Constants.KDirectEncodingBitField); }
		}

		public BinaryDataTreeVariantType Type
		{
			get { return BitEncoders.BinaryDataTreeVariantType.BitDecode(this.Flags, Constants.KTypeBitField); }
			set { this.Flags = BitEncoders.BinaryDataTreeVariantType.BitEncode(value, this.Flags, Constants.KTypeBitField); }
		}

		public BinaryDataTreeVariantTypeSizeInBytes TypeSizeInBytes
		{
			get { return BitEncoders.BinaryDataTreeVariantTypeSizeInBytes.BitDecode(this.Flags, Constants.KTypeSizeInBytesLog2BitField); }
			set { this.Flags = BitEncoders.BinaryDataTreeVariantTypeSizeInBytes.BitEncode(value, this.Flags, Constants.KTypeSizeInBytesLog2BitField); }
		}

		public bool IsLastNameValue
		{
			get { return Bits.BitDecode(this.Flags, Constants.KIsLastNameValueBitField).ToBoolean(); }
			set { this.Flags = Bits.BitEncode(value.ToUInt16(), this.Flags, Constants.KIsLastNameValueBitField); }
		}

		public byte Size
		{
			get { return (byte)Bits.BitDecode(this.Flags, Constants.KSizeBitField); }
			set { this.Flags = Bits.BitEncode(value, this.Flags, Constants.KSizeBitField); }
		}

		public bool IsEmpty { get { return this.Type == BinaryDataTreeVariantType.NULL; } }

		public bool IsIndirect { get { return !this.DirectEncoding; } }
		public bool SizeIsIndirect { get { return this.Size == Constants.KSizeBitField.Bitmask32; } }
		public bool IsArray { get { return this.Size > this.TypeSize; } }

		public bool IsUnicode { get {
			return this.Type == BinaryDataTreeVariantType.STRING && this.TypeSizeInBytes == BinaryDataTreeVariantTypeSizeInBytes._2byte;
		} }

		public bool HasUnicodeData { get {
			return this.IsUnicode;
		} }

		public uint TypeSize { get {
			if (this.IsEmpty)
				return 0;
			return 1U << (int) this.TypeSizeInBytes;
		} }
		#endregion

		#region Data
		[Interop.FieldOffset(0)]
		public bool Bool;
		[Interop.FieldOffset(0)]
		public uint Offset;
		[Interop.FieldOffset(0)]
		public uint Int;
		[Interop.FieldOffset(0)]
		public float Single;

		[Interop.FieldOffset(0)]
		public byte Char0;
		[Interop.FieldOffset(1)]
		public byte Char1;
		[Interop.FieldOffset(2)]
		public byte Char2;
		[Interop.FieldOffset(3)]
		public byte Char3;
		#endregion

		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.Int);
			s.Stream(ref this.NameOffset);
			s.Stream(ref this.Flags);
		}

		#region ToString
		string StringToString(XmbVariantMemoryPool pool)
		{
			string result = null;

			if (this.IsIndirect)
				result = pool.GetString(this.Offset, this.IsUnicode);
			else
			{
				// Unicode is always indirect
				//if (IsUnicode) result = new string((char)Char0, 1);
				//else
				{
					var sb = new System.Text.StringBuilder(3);
					if (this.Char0 != '\0')
						sb.Append((char) this.Char0);
					if (this.Char1 != '\0')
						sb.Append((char) this.Char1);
					if (this.Char2 != '\0')
						sb.Append((char) this.Char2);

					result = sb.ToString();
				}
			}
			return result;
		}

		uint GetDataSize(XmbVariantMemoryPool pool)
		{
			uint size = this.Size;

			if (this.SizeIsIndirect)
			{
				size = pool.GetUInt32(this.Offset-sizeof(uint));
				Contract.Assert(size > Constants.KSizeBitField.Bitmask32);
			}

			return size;
		}

		[Obsolete]
		public uint GetLength(XmbVariantMemoryPool pool)
		{
			uint length = 0;
			switch (this.Type)
			{
				case BinaryDataTreeVariantType.NULL:
					break;

				case BinaryDataTreeVariantType.STRING:
					length = (uint) this.StringToString(pool).Length;
					break;

				default:
					length = this.GetDataSize(pool);
					length >>= (int) this.TypeSizeInBytes;
					break;
			}

			return length;
		}

		internal string ToString(XmbVariantMemoryPool pool)
		{
			string result = "";

			switch (this.Type)
			{
				case BinaryDataTreeVariantType.BOOL: {
					// Phoenix uses lower case and Boolean.ToString uppercases the first letter
					result = this.Bool ? "true" : "false";
				} break;

				case BinaryDataTreeVariantType.INT: {
					uint i = this.Int;
					if (this.IsIndirect)
						i = pool.GetUInt32(this.Offset);
					result = this.IsUnsigned
						? i.ToString()
						: ((int)i).ToString();
				} break;

				case BinaryDataTreeVariantType.FLOAT: {
					if (this.TypeSizeInBytes == BinaryDataTreeVariantTypeSizeInBytes._8byte)
					{
						double d = pool.GetDouble(this.Offset);
						result = d.ToStringInvariant(Numbers.K_DOUBLE_ROUND_TRIP_FORMAT_SPECIFIER);
					}
					else
					{
						float f = this.Single;
						if (this.IsIndirect)
							f = pool.GetSingle(this.Offset);
						result = f.ToStringInvariant(Numbers.K_FLOAT_ROUND_TRIP_FORMAT_SPECIFIER);
					}
				} break;

				case BinaryDataTreeVariantType.STRING: {
					result = this.StringToString(pool);
				} break;
			}

			return result;
		}

		#endregion

		#region RequiresIndirectStorage
		public static bool SizeValueRequiresIndirectStorage(int size)
		{
			Contract.Requires(size >= 0);

			return (uint)size >= Constants.KSizeBitField.Bitmask32;
		}
		public static bool StringRequiresIndirectStorage(string s, bool isUnicode)
		{
			Contract.Requires(s != null);
#if false
			return s.Length > kMaxDirectEncodedStringLength || isUnicode;
#else
			return true;
#endif
		}
		#endregion

		public BinaryDataTreeVariantTypeDesc GuessTypeDesc()
		{
			switch (this.Type)
			{
				case BinaryDataTreeVariantType.NULL:
					return BinaryDataTreeVariantTypeDesc.Null;

				case BinaryDataTreeVariantType.BOOL:
					return BinaryDataTreeVariantTypeDesc.Bool;

				case BinaryDataTreeVariantType.INT:
					switch (this.TypeSize)
					{
						case sizeof(byte):
							return this.IsUnsigned
								? BinaryDataTreeVariantTypeDesc.UInt8
								: BinaryDataTreeVariantTypeDesc. Int8;
						case sizeof(ushort):
							return this.IsUnsigned
								? BinaryDataTreeVariantTypeDesc.UInt16
								: BinaryDataTreeVariantTypeDesc. Int16;
						case sizeof(uint):
							return this.IsUnsigned
								? BinaryDataTreeVariantTypeDesc.UInt32
								: BinaryDataTreeVariantTypeDesc. Int32;
						case sizeof(ulong):
							return this.IsUnsigned
								? BinaryDataTreeVariantTypeDesc.UInt64
								: BinaryDataTreeVariantTypeDesc. Int64;
					}
					throw new KSoft.Debug.UnreachableException(this.Type + this.TypeSize.ToString());

				case BinaryDataTreeVariantType.FLOAT:
					if (this.TypeSize == sizeof(double))
					{
						return BinaryDataTreeVariantTypeDesc.Double;
					}
					else
					{
						// #NOTE it doesn't matter if SizeIsIndirect, because SingleVector Size can fit within the 7-bits of Size
						uint elements = this.Size / this.TypeSize;
						switch (elements)
						{
							case 2:
							case 3:
							case 4:
								return BinaryDataTreeVariantTypeDesc.SingleVector;
						}

						return BinaryDataTreeVariantTypeDesc.Single;
					}

				case BinaryDataTreeVariantType.STRING:
					return this.IsUnicode
						? BinaryDataTreeVariantTypeDesc.UnicodeString
						: BinaryDataTreeVariantTypeDesc.String;

				default:
					throw new KSoft.Debug.UnreachableException(this.Type.ToString());
			}
		}
	};

	public sealed class BinaryDataTreeBuildNameValue
	{
		public string name;
		public BinaryDataTreeVariantData variant;
	};
}