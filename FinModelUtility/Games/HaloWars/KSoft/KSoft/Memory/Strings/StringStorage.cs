using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using KSoft.Shell;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Memory.Strings
{
	/// <summary>String storage definition</summary>
	[SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
	public struct StringStorage : //IO.IEndianStreamable,
		IEquatable<StringStorage>, IEqualityComparer<StringStorage>,
		IComparer<StringStorage>, IComparable<StringStorage>,
		System.Collections.IComparer, IComparable
	{
		#region WidthType
		StringStorageWidthType mWidthType_;
		/// <summary>Character serialization width/encoding type</summary>
		public StringStorageWidthType WidthType { get { return this.mWidthType_; } }
		#endregion

		#region Type
		StringStorageType mType_;
		/// <summary>Character serialization format method</summary>
		public StringStorageType Type { get { return this.mType_; } }
		#endregion

		#region ByteOrder
		Shell.EndianFormat mByteOrder_;
		/// <summary>Endian byte order of the character storage</summary>
		/// <remarks>Affects both the wide-characters and any length prefixes written</remarks>
		public Shell.EndianFormat ByteOrder { get { return this.mByteOrder_; } }
		#endregion

		#region LengthPrefix
		StringStorageLengthPrefix mLengthPrefix_;
		/// <summary>Length prefix size</summary>
		public StringStorageLengthPrefix LengthPrefix { get { return this.mLengthPrefix_; } }

		public bool HasLengthPrefix { get { return this.mType_.UsesLengthPrefix(); } }
		#endregion

		#region FixedLength
		short mFixedLength_;
		/// <summary>Fixed string serialization length</summary>
		/// <remarks>Set to '0' when no specified fixed length</remarks>
		public short FixedLength { get { return this.mFixedLength_; } }

		/// <summary>Does the storage use a fixed length character array</summary>
		/// <remarks>
		/// Ignored in <see cref="StringStorageType.Clr"/> cases
		///
		/// For <see cref="StringStorageType.CHAR_ARRAY"/> cases, the full fixed length
		/// buffer can be used, but for <see cref="StringStorageType.C_STRING"/> cases
		/// the <see cref="FixedLength"/> will be 1 less due to null termination
		/// </remarks>
		public bool IsFixedLength { get { return this.mFixedLength_ != 0 && !this.HasLengthPrefix; } }

	#endregion

	#region Ctor
	/// <summary>Construct a new string storage definition</summary>
	/// <param name="widthType">Width size of a single character of this string definition</param>
	/// <param name="type">Storage method for this string definition</param>
	/// <param name="byteOrder"></param>
	/// <param name="fixedLength">The storage fixed length (in characters) of this string definition</param>
	public StringStorage(StringStorageWidthType widthType, StringStorageType type,
			Shell.EndianFormat byteOrder = Shell.EndianFormat.LITTLE, short fixedLength = 0)
		{
			Contract.Requires(!type.UsesLengthPrefix(), "Use ctor with StringStorageLengthPrefix instead");
			Contract.Requires(fixedLength >= 0);
			Contract.Requires(fixedLength == 0 || !widthType.IsVariableWidth(),
				"Can't use a variable width encoding with fixed buffers!");

			this.mWidthType_ = widthType;
			this.mType_ = type;
			this.mByteOrder_ = byteOrder;
			this.mLengthPrefix_ = StringStorageLengthPrefix.NONE;
			this.mFixedLength_ = fixedLength;

			this.kHashCode_ = CalculateHashCode(this.mWidthType_, this.mType_, this.mByteOrder_, this.mLengthPrefix_, this.mFixedLength_);
		}
		/// <summary>Construct a new string storage definition (in <see cref="EndianFormat.LITTLE"/> byte order)</summary>
		/// <param name="widthType">Width size of a single character of this string definition</param>
		/// <param name="type">Storage method for this string definition</param>
		/// <param name="fixedLength">The storage fixed length (in characters) of this string definition</param>
		public StringStorage(StringStorageWidthType widthType, StringStorageType type, short fixedLength) :
			this(widthType, type, Shell.EndianFormat.LITTLE, fixedLength)
		{
			Contract.Requires(!type.UsesLengthPrefix(), "Use ctor with StringStorageLengthPrefix instead");
			Contract.Requires(fixedLength >= 0);
			Contract.Requires(fixedLength == 0 || !widthType.IsVariableWidth(),
				"Can't use a variable width encoding with fixed buffers!");
		}
		/// <summary>Construct a new Pascal string storage definition</summary>
		/// <param name="widthType">Width size of a single character of this string definition</param>
		/// <param name="prefix">Length prefix size</param>
		/// <param name="byteOrder"></param>
		public StringStorage(StringStorageWidthType widthType, StringStorageLengthPrefix prefix,
			Shell.EndianFormat byteOrder = Shell.EndianFormat.LITTLE)
		{
			this.mWidthType_ = widthType;
			this.mType_ = StringStorageType.PASCAL;
			this.mByteOrder_ = byteOrder;
			this.mLengthPrefix_ = prefix;
			this.mFixedLength_ = 0;

			this.kHashCode_ = CalculateHashCode(this.mWidthType_, this.mType_, this.mByteOrder_, this.mLengthPrefix_, this.mFixedLength_);
		}
		#endregion

		#region IEndianStreamable Members
#if false // #TODO
		public void Read(IO.EndianReader s)
		{
			mWidthType = (StringStorageWidthType)s.ReadByte();
			mType = (StringStorageType)s.ReadByte();
			mByteOrder = (Shell.EndianFormat)s.ReadByte();
			s.Seek(sizeof(byte));
			mFixedLength = s.ReadInt16();
			s.Seek(sizeof(ushort));
		}

		public void Write(IO.EndianWriter s)
		{
			s.Write((byte)mWidthType);
			s.Write((byte)mType);
			s.Write((byte)mByteOrder);
			s.Write(byte.MinValue);
			s.Write(mFixedLength);
			s.Write(ushort.MinValue);
		}
#endif
		#endregion

		#region GetHashCode
		static int CalculateHashCode(StringStorageWidthType widthType, StringStorageType type, Shell.EndianFormat byteOrder,
			StringStorageLengthPrefix prefix,
			short fixedLength)
		{
			var encoder = new Bitwise.HandleBitEncoder();
			encoder.Encode32(widthType, TypeExtensions.BitEncoders.StringStorageWidthType);
			encoder.Encode32(type, TypeExtensions.BitEncoders.StringStorageType);
			encoder.Encode32(byteOrder, TypeExtensions.BitEncoders.EndianFormat);

			if(type.UsesLengthPrefix())
				encoder.Encode32(prefix, TypeExtensions.BitEncoders.StringStorageLengthPrefix);
			else if (fixedLength != 0)
				encoder.Encode32((uint)fixedLength, 0x7FFF);

			return (int)encoder.GetHandle32();
		}
		readonly int kHashCode_;
		/// <summary>Returns the hash code for this instance</summary>
		/// <returns>All of this definition's fields bit-encoded into an integer</returns>
		public override int GetHashCode()	{ return this.kHashCode_; }
		#endregion

		#region IEquatable<StringStorage> Members
		/// <summary>Compares this to another <see cref="StringStorage"/> object testing their underlying fields for equality</summary>
		/// <param name="obj">other <see cref="StringStorage"/> object</param>
		/// <returns>true if both this object and <paramref name="obj"/> are equal</returns>
		public bool Equals(StringStorage other)					{ return this.kHashCode_ == other.kHashCode_; }

		public bool Equals(StringStorage x, StringStorage y)	{ return x.kHashCode_ == y.kHashCode_; }

		public int GetHashCode(StringStorage obj)				{ return obj.GetHashCode(); }

		/// <summary>Compares this to another object testing for equality</summary>
		/// <param name="obj"></param>
		/// <returns>
		/// True if both this object and <paramref name="obj"/> are equal.
		/// False if <paramref name="obj"/> is not a <see cref="StringStorage"/></returns>
		public override bool Equals(object obj)
		{
			if (obj is StringStorage s)
				return this.Equals(s);

			return false;
		}
		#endregion

		#region IComparer<StringStorage> Members
		/// <summary>Compare two <see cref="StringStorage"/> objects for similar serializer values</summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		/// <see cref="int CompareTo(StringStorage)"/>
		public int Compare(StringStorage x, StringStorage y) => x.CompareTo(y);
		/// <summary>Compare this with another <see cref="StringStorage"/> object for similar serializer values</summary>
		/// <param name="other"></param>
		/// <returns></returns>
		/// <remarks>
		/// Compares the object fields in the following order:
		/// <see cref="StringStorage.Type"/>,
		/// <see cref="StringStorage.WidthType"/>,
		/// <see cref="StringStorage.ByteOrder"/>,
		/// <see cref="StringStorage.LengthPrefix"/>,
		/// <see cref="StringStorage.FixedLength"/>
		/// </remarks>
		public int CompareTo(StringStorage other)
		{
			if (this.mType_ == other.mType_)
			{
				if (this.mWidthType_ == other.mWidthType_)
					if (this.mByteOrder_ == other.mByteOrder_)
						if (this.mLengthPrefix_ == other.mLengthPrefix_)
							return this.mFixedLength_ - other.mFixedLength_;
						else
							return ((int) this.mLengthPrefix_) - ((int)other.mLengthPrefix_);
					else
						return ((int) this.mByteOrder_) - ((int)other.mByteOrder_);
				else
					return ((int) this.mWidthType_) - ((int)other.mWidthType_);
			}
			else
				return ((int) this.mType_) - ((int)other.mType_);
		}

		/// <summary></summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		/// <see cref="int Compare(StringStorage, StringStorage)"/>
		public int Compare(object x, object y) => this.Compare((StringStorage)x, (StringStorage)y);
		/// <summary></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		/// <see cref="int CompareTo(StringStorage)"/>
		public int CompareTo(object obj) => this.CompareTo((StringStorage)obj);
		#endregion

		#region Operators
		public static bool operator ==(StringStorage a, StringStorage b) => a.Equals(b);
		public static bool operator !=(StringStorage a, StringStorage b) => !(a == b);
		#endregion


		#region CString
		static readonly StringStorage KCStringAscii = new StringStorage(StringStorageWidthType.ASCII, StringStorageType.C_STRING);
		/// <summary>Get a storage definition for a regular CString format ASCII string</summary>
		public static StringStorage CStringAscii { get { return KCStringAscii; } }

		static readonly StringStorage KCStringUtf8 = new StringStorage(StringStorageWidthType.UTF8, StringStorageType.C_STRING);
		/// <summary>Get a storage definition for a regular CString format ASCII string</summary>
		public static StringStorage CStringUtf8 { get { return KCStringUtf8; } }

		static readonly StringStorage KCStringUnicode = new StringStorage(StringStorageWidthType.UNICODE, StringStorageType.C_STRING);
		/// <summary>Get a storage definition for a CString format Unicode string</summary>
		public static StringStorage CStringUnicode { get { return KCStringUnicode; } }

		static readonly StringStorage KCStringUnicodeBe = new StringStorage(StringStorageWidthType.UNICODE, StringStorageType.C_STRING, Shell.EndianFormat.BIG);
		/// <summary>Get a storage definition for a CString format Unicode string (big endian)</summary>
		public static StringStorage CStringUnicodeBigEndian { get { return KCStringUnicodeBe; } }
		#endregion

		#region String
		static readonly StringStorage KStringAscii = new StringStorage(StringStorageWidthType.ASCII, StringStorageType.CHAR_ARRAY);
		/// <summary>Get a storage definition for a string of ASCII characters</summary>
		/// <remarks>This is a <see cref="StringStorageType.CHAR_ARRAY"/> which doesn't specify a fixed length.</remarks>
		public static StringStorage AsciiString { get { return KStringAscii; } }

		static readonly StringStorage KStringUtf8 = new StringStorage(StringStorageWidthType.UTF8, StringStorageType.CHAR_ARRAY);
		/// <summary>Get a storage definition for a string of ASCII characters</summary>
		/// <remarks>This is a <see cref="StringStorageType.CHAR_ARRAY"/> which doesn't specify a fixed length.</remarks>
		public static StringStorage Utf8String { get { return KStringUtf8; } }

		static readonly StringStorage KStringUnicode = new StringStorage(StringStorageWidthType.UNICODE, StringStorageType.CHAR_ARRAY);
		/// <summary>Get a storage definition for a string of Unicode characters</summary>
		/// <remarks>This is a <see cref="StringStorageType.CHAR_ARRAY"/> which doesn't specify a fixed length.</remarks>
		public static StringStorage UnicodeString { get { return KStringUnicode; } }

		static readonly StringStorage KStringUnicodeBe = new StringStorage(StringStorageWidthType.UNICODE, StringStorageType.CHAR_ARRAY, Shell.EndianFormat.BIG);
		/// <summary>Get a storage definition for a string of Unicode characters (big endian)</summary>
		/// <remarks>This is a <see cref="StringStorageType.CHAR_ARRAY"/> which doesn't specify a fixed length.</remarks>
		public static StringStorage UnicodeStringBigEndian { get { return KStringUnicodeBe; } }
		#endregion

		internal static readonly StringStorage[] KStorageTypesList = [
			// Ascii
			KCStringAscii,
			/* Clr */ new StringStorage(StringStorageWidthType.ASCII, StringStorageLengthPrefix.INT7),
			KStringAscii,

			// Unicode
			KCStringUnicode,
			/* Clr */ new StringStorage(StringStorageWidthType.UNICODE, StringStorageLengthPrefix.INT7),
			KStringUnicode,

			// UTF8
			KCStringUtf8,
			/* Clr */ new StringStorage(StringStorageWidthType.UTF8, StringStorageLengthPrefix.INT7),
			KStringUtf8,

			// Unicode-BE
			KCStringUnicodeBe,
			/* Clr */ new StringStorage(StringStorageWidthType.UNICODE, StringStorageLengthPrefix.INT7, Shell.EndianFormat.BIG),
			KStringUnicodeBe
		];
	};
}
