using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Xmb
{
	using EType = BinaryDataTreeVariantType;
	using ESizeInBytes = BinaryDataTreeVariantTypeSizeInBytes;

	public enum BinaryDataTreeVariantType : byte
	{
		NULL,
		BOOL,
		INT,
		FLOAT,
		STRING,

		/// <remarks>3 bits</remarks>
		[Obsolete(EnumBitEncoderBase.K_OBSOLETE_MSG, true)] K_NUMBER_OF,
	};

	public enum BinaryDataTreeVariantTypeSizeInBytes : byte
	{
		_1byte,
		_2byte,
		_4byte,
		_8byte,
		_16byte,

		/// <remarks>3 bits</remarks>
		[Obsolete(EnumBitEncoderBase.K_OBSOLETE_MSG, true)] K_NUMBER_OF,
	};

	public struct BinaryDataTreeVariantTypeDesc
	{
		public const int K_SIZE_OF = 4;

		const byte K_IS_UNSIGNED_ = 1;

		public EType type;
		private ESizeInBytes size_;
		private ESizeInBytes alignment_;
		private byte flags_;

		public bool IsUnsigned { get { return this.flags_ != 0; } }
		public bool IsUnicode { get { return this.type == EType.STRING && this.size_ == ESizeInBytes._2byte; } }
		public int SizeBit { get { return (int) this.size_; } }
		public int SizeOf { get { return 1<< this.SizeBit; } }
		public int AlignmentBit { get { return (int) this.alignment_; } }
		public int AlignmentOf { get { return 1<< this.AlignmentBit; } }

		private BinaryDataTreeVariantTypeDesc(EType type, ESizeInBytes size, ESizeInBytes alignment, byte flags = 0)
		{
			this.type = type;
			this.size_ = size;
			this.alignment_ = alignment;
			this.flags_ = flags;
		}

		public override string ToString()
		{
			return string.Format("{0} {1} {2} {3}",
			                     this.size_,
			                     this.alignment_,
			                     this.flags_,
			                     this.type);
		}

		#region Equality utils
		public static bool operator ==(BinaryDataTreeVariantTypeDesc lhs, BinaryDataTreeVariantTypeDesc rhs)
		{
			return lhs.type == rhs.type
				&& lhs.size_ == rhs.size_
				&& lhs.alignment_ == rhs.alignment_
				&& lhs.flags_ == rhs.flags_;
		}
		public static bool operator !=(BinaryDataTreeVariantTypeDesc lhs, BinaryDataTreeVariantTypeDesc rhs)
		{
			return lhs.type != rhs.type
				|| lhs.size_ != rhs.size_
				|| lhs.alignment_ != rhs.alignment_
				|| lhs.flags_ != rhs.flags_;
		}

		public override bool Equals(object obj)
		{
			if (obj is BinaryDataTreeVariantTypeDesc)
				return this == (BinaryDataTreeVariantTypeDesc)obj;
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public uint GetSuperFastHashCode()
		{
			var buffer = PhxUtil.GetBufferForSuperFastHash(sizeof(uint));

			uint hash;

			Bitwise.ByteSwap.ReplaceBytes(buffer, 0, (uint) this.type);
			hash = PhxUtil.SuperFastHash(buffer, 0, sizeof(uint));

			Bitwise.ByteSwap.ReplaceBytes(buffer, 0, (ushort) this.size_);
			hash = PhxUtil.SuperFastHash(buffer, 0, sizeof(ushort), hash);

			Bitwise.ByteSwap.ReplaceBytes(buffer, 0, (ushort) this.alignment_);
			hash = PhxUtil.SuperFastHash(buffer, 0, sizeof(ushort), hash);

			buffer[0] = this.flags_;
			hash = PhxUtil.SuperFastHash(buffer, 0, sizeof(byte), hash);

			return hash;
		}
		#endregion

		#region Array utils
		public Array MakeArray(int length)
		{
			if (length <= 1)
				return null;

			switch (this.type)
			{
				case EType.NULL:
					return null;

				case EType.BOOL:
					return new bool[length];

				case EType.INT:
				{
					if (this.IsUnsigned)
					{
						switch (this.size_)
						{
							case ESizeInBytes._1byte: return new byte[length];
							case ESizeInBytes._2byte: return new ushort[length];
							case ESizeInBytes._4byte: return new uint[length];
							case ESizeInBytes._8byte: return new ulong[length];
						}
					}
					else
					{
						switch (this.size_)
						{
							case ESizeInBytes._1byte: return new sbyte[length];
							case ESizeInBytes._2byte: return new short[length];
							case ESizeInBytes._4byte: return new int[length];
							case ESizeInBytes._8byte: return new long[length];
						}
					}
				} throw new KSoft.Debug.UnreachableException(this.ToString());

				case EType.FLOAT:
				{
					switch (this.size_)
					{
						case ESizeInBytes._4byte: return new float[length];
						case ESizeInBytes._8byte: return new double[length];
					}
				} throw new KSoft.Debug.UnreachableException(this.ToString());

				case EType.STRING:
					throw new KSoft.Debug.UnreachableException(this.ToString());

				default:
					throw new KSoft.Debug.UnreachableException(this.ToString());
			}
		}

		public Array ReadArray(IO.EndianReader reader, Array array)
		{
			switch (this.type)
			{
				case EType.NULL:
					return null;

				case EType.BOOL:
					return reader.ReadFixedArray((bool[])array);

				case EType.INT:
				{
					if (this.IsUnsigned)
					{
						switch (this.size_)
						{
							case ESizeInBytes._1byte: return reader.ReadFixedArray((byte[])array);
							case ESizeInBytes._2byte: return reader.ReadFixedArray((ushort[])array);
							case ESizeInBytes._4byte: return reader.ReadFixedArray((uint[])array);
							case ESizeInBytes._8byte: return reader.ReadFixedArray((ulong[])array);
						}
					}
					else
					{
						switch (this.size_)
						{
							case ESizeInBytes._1byte: return reader.ReadFixedArray((sbyte[])array);
							case ESizeInBytes._2byte: return reader.ReadFixedArray((short[])array);
							case ESizeInBytes._4byte: return reader.ReadFixedArray((int[])array);
							case ESizeInBytes._8byte: return reader.ReadFixedArray((long[])array);
						}
					}
				} throw new KSoft.Debug.UnreachableException(this.ToString());

				case EType.FLOAT:
				{
					switch (this.size_)
					{
						case ESizeInBytes._4byte: return reader.ReadFixedArray((float[])array);
						case ESizeInBytes._8byte: return reader.ReadFixedArray((double[])array);
					}
				} throw new KSoft.Debug.UnreachableException(this.ToString());

				case EType.STRING:
					throw new KSoft.Debug.UnreachableException(this.ToString());

				default:
					throw new KSoft.Debug.UnreachableException(this.ToString());
			}
		}

		public Array WriteArray(IO.EndianWriter writer, Array array)
		{
			switch (this.type)
			{
				case EType.NULL:
					return null;

				case EType.BOOL:
					return writer.WriteFixedArray((bool[])array);

				case EType.INT:
				{
					if (this.IsUnsigned)
					{
						switch (this.size_)
						{
							case ESizeInBytes._1byte: return writer.WriteFixedArray((byte[])array);
							case ESizeInBytes._2byte: return writer.WriteFixedArray((ushort[])array);
							case ESizeInBytes._4byte: return writer.WriteFixedArray((uint[])array);
							case ESizeInBytes._8byte: return writer.WriteFixedArray((ulong[])array);
						}
					}
					else
					{
						switch (this.size_)
						{
							case ESizeInBytes._1byte: return writer.WriteFixedArray((sbyte[])array);
							case ESizeInBytes._2byte: return writer.WriteFixedArray((short[])array);
							case ESizeInBytes._4byte: return writer.WriteFixedArray((int[])array);
							case ESizeInBytes._8byte: return writer.WriteFixedArray((long[])array);
						}
					}
				} throw new KSoft.Debug.UnreachableException(this.ToString());

				case EType.FLOAT:
				{
					switch (this.size_)
					{
						case ESizeInBytes._4byte: return writer.WriteFixedArray((float[])array);
						case ESizeInBytes._8byte: return writer.WriteFixedArray((double[])array);
					}
				} throw new KSoft.Debug.UnreachableException(this.ToString());

				case EType.STRING:
					throw new KSoft.Debug.UnreachableException(this.ToString());

				default:
					throw new KSoft.Debug.UnreachableException(this.ToString());
			}
		}

		public string ArrayToString(Array array)
		{
			switch (this.type)
			{
				case EType.NULL:
					return null;

				case EType.BOOL:
					return ((bool[])array).ToConcatBinaryString();

				case EType.INT:
					return array.ArrayToConcatString();

				case EType.FLOAT:
				{
					switch (this.size_)
					{
						case ESizeInBytes._4byte: return ((float[])array).ToConcatStringInvariant();
						case ESizeInBytes._8byte: return ((double[])array).ToConcatStringInvariant();
					}
				} throw new KSoft.Debug.UnreachableException(this.ToString());

				case EType.STRING:
					throw new KSoft.Debug.UnreachableException(this.ToString());

				default:
					throw new KSoft.Debug.UnreachableException(this.ToString());
			}
		}

		public Array ArrayFromString(string str)
		{
			if (str.IsNullOrEmpty() || this.type == EType.NULL)
				return null;

			var strList = new List<string>();
			if (!Util.ParseStringList(str, strList))
				throw new System.IO.InvalidDataException(str);

			if (strList.Count == 0)
				return null;

			switch (this.type)
			{
				case EType.BOOL:
					return strList.ConvertAllArray(Text.Util.ParseBooleanLazy);

				case EType.INT:
				{
					if (this.IsUnsigned)
					{
						switch (this.size_)
						{
							case ESizeInBytes._1byte: return strList.ConvertAllArray(Convert.ToByte);
							case ESizeInBytes._2byte: return strList.ConvertAllArray(Convert.ToUInt16);
							case ESizeInBytes._4byte: return strList.ConvertAllArray(Convert.ToUInt32);
							case ESizeInBytes._8byte: return strList.ConvertAllArray(Convert.ToUInt64);
						}
					}
					else
					{
						switch (this.size_)
						{
							case ESizeInBytes._1byte: return strList.ConvertAllArray(Convert.ToSByte);
							case ESizeInBytes._2byte: return strList.ConvertAllArray(Convert.ToInt16);
							case ESizeInBytes._4byte: return strList.ConvertAllArray(Convert.ToInt32);
							case ESizeInBytes._8byte: return strList.ConvertAllArray(Convert.ToInt64);
						}
					}
				} throw new KSoft.Debug.UnreachableException(this.ToString());

				case EType.FLOAT:
				{
					switch (this.size_)
					{
						case ESizeInBytes._4byte: return strList.ConvertAllArray(Numbers.FloatParseInvariant);
						case ESizeInBytes._8byte: return strList.ConvertAllArray(Numbers.DoubleParseInvariant);
					}
				} throw new KSoft.Debug.UnreachableException(this.ToString());

				case EType.STRING:
					throw new NotSupportedException(this.ToString());

				default:
					throw new KSoft.Debug.UnreachableException(this.ToString());
			}
		}
		#endregion

		#region ITagElementTextStreamable Members
		string GetSerializedTypeName()
		{
			switch (this.type)
			{
				case EType.NULL:
					return "null";
				case EType.BOOL:
					return "bool";
				case EType.INT:
					return string.Format("{0}int{1}",
					                     this.IsUnsigned ? "u" : "",
					                     this.SizeOf * Bits.K_BYTE_BIT_COUNT);
				case EType.FLOAT:
					return this.SizeOf == sizeof(float)
						? "float"
						: "double";
				case EType.STRING:
					return this.IsUnicode
						? "ustring"
						: "string";

				default:
					throw new KSoft.Debug.UnreachableException(this.ToString());
			}
		}
		static BinaryDataTreeVariantTypeDesc GuessTypeFromSerializedString(string typeName)
		{
			switch (typeName)
			{
				case null:
				case "":
				case "null": return Null;

				case "bool": return Bool;
				case "uint8": return UInt8;
				case "uint16": return UInt16;
				case "uint32": return UInt32;
				case "uint64": return UInt64;
				case "int8": return Int8;
				case "int16": return Int16;
				case "int32": return Int32;
				case "int64": return Int64;
				case "float": return Single;
				case "double": return Double;
				case "ustring": return UnicodeString;
				case "string": return String;

				default:
					throw new System.IO.InvalidDataException(typeName);
			}
		}

		public void ToStream<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, int arrayLength)
			where TDoc : class
			where TCursor : class
		{
			if (this.type != EType.NULL)
			{
				string typeName = this.GetSerializedTypeName();
				s.WriteAttribute("dataType", typeName);
			}

			if (this.type != EType.NULL && this.AlignmentOf > this.SizeOf)
				s.WriteAttribute("alignment", this.AlignmentOf);

			if (arrayLength > 1)
				s.WriteAttribute("arraySize", arrayLength);
		}

		public static BinaryDataTreeVariantTypeDesc GuessFromStream<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, out int arrayLength)
			where TDoc : class
			where TCursor : class
		{
			arrayLength = 0;

			string typeName = null;
			if (!s.ReadAttributeOpt("dataType", ref typeName))
				return Null;

			var guessedType = GuessTypeFromSerializedString(typeName);

			int alignment = -1;
			s.ReadAttributeOpt("alignment", ref alignment);

			if (s.ReadAttributeOpt("arraySize", ref arrayLength) && arrayLength > 1)
			{
				if (guessedType.type == EType.FLOAT)
					guessedType = SingleVector;
			}

			return guessedType;
		}
		#endregion

		#region Well known types
		public static BinaryDataTreeVariantTypeDesc Null { get {
			return new BinaryDataTreeVariantTypeDesc(EType.NULL, ESizeInBytes._1byte, ESizeInBytes._1byte);
		} }
		public static BinaryDataTreeVariantTypeDesc Bool { get {
			return new BinaryDataTreeVariantTypeDesc(EType.BOOL, ESizeInBytes._1byte, ESizeInBytes._1byte, K_IS_UNSIGNED_);
		} }
		public static BinaryDataTreeVariantTypeDesc UInt8 { get {
			return new BinaryDataTreeVariantTypeDesc(EType.INT, ESizeInBytes._1byte, ESizeInBytes._1byte, K_IS_UNSIGNED_);
		} }
		public static BinaryDataTreeVariantTypeDesc Int8 { get {
			return new BinaryDataTreeVariantTypeDesc(EType.INT, ESizeInBytes._1byte, ESizeInBytes._1byte);
		} }
		public static BinaryDataTreeVariantTypeDesc UInt16 { get {
			return new BinaryDataTreeVariantTypeDesc(EType.INT, ESizeInBytes._2byte, ESizeInBytes._2byte, K_IS_UNSIGNED_);
		} }
		public static BinaryDataTreeVariantTypeDesc Int16 { get {
			return new BinaryDataTreeVariantTypeDesc(EType.INT, ESizeInBytes._2byte, ESizeInBytes._2byte);
		} }
		public static BinaryDataTreeVariantTypeDesc UInt32 { get {
			return new BinaryDataTreeVariantTypeDesc(EType.INT, ESizeInBytes._4byte, ESizeInBytes._4byte, K_IS_UNSIGNED_);
		} }
		public static BinaryDataTreeVariantTypeDesc Int32 { get {
			return new BinaryDataTreeVariantTypeDesc(EType.INT, ESizeInBytes._4byte, ESizeInBytes._4byte);
		} }
		public static BinaryDataTreeVariantTypeDesc UInt64 { get {
			return new BinaryDataTreeVariantTypeDesc(EType.INT, ESizeInBytes._8byte, ESizeInBytes._8byte, K_IS_UNSIGNED_);
		} }
		public static BinaryDataTreeVariantTypeDesc Int64 { get {
			return new BinaryDataTreeVariantTypeDesc(EType.INT, ESizeInBytes._8byte, ESizeInBytes._8byte);
		} }
		public static BinaryDataTreeVariantTypeDesc Single { get {
			return new BinaryDataTreeVariantTypeDesc(EType.FLOAT, ESizeInBytes._4byte, ESizeInBytes._4byte);
		} }
		public static BinaryDataTreeVariantTypeDesc Double { get {
			return new BinaryDataTreeVariantTypeDesc(EType.FLOAT, ESizeInBytes._8byte, ESizeInBytes._8byte);
		} }
		public static BinaryDataTreeVariantTypeDesc String { get {
			return new BinaryDataTreeVariantTypeDesc(EType.STRING, ESizeInBytes._1byte, ESizeInBytes._1byte, K_IS_UNSIGNED_);
		} }
		public static BinaryDataTreeVariantTypeDesc UnicodeString { get {
			return new BinaryDataTreeVariantTypeDesc(EType.STRING, ESizeInBytes._2byte, ESizeInBytes._2byte, K_IS_UNSIGNED_);
		} }
		public static BinaryDataTreeVariantTypeDesc SingleVector { get {
			return new BinaryDataTreeVariantTypeDesc(EType.FLOAT, ESizeInBytes._4byte, ESizeInBytes._16byte);
		} }
		#endregion
	};
}