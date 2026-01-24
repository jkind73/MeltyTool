using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Xmb
{
	static class XmbVariantSerialization
	{
		const int K_INFO_BIT_INDEX_ = 24;
		public const uint K_INFO_BIT_MASK = 0xFF000000;
		public const uint K_VALUE_BIT_MASK = 0x00FFFFFF;

		#region Type coding
		public enum RawVariantType : byte
		{
			NULL			= 0, // i.e., empty string
			SINGLE24		= 1, // S1E6M17
			SINGLE			= 2, // Indirect
			INT24			= 3,
			INT				= 4, // Indirect
			FIXED_POINT		= 5,
			DOUBLE			= 6,
			BOOL			= 7,
			STRING_ANSI		= 8, // if string is 3 characters or less, it gets put in the data field
			STRING_UNICODE	= 9,
			VECTOR			= 10,
		};

		const int K_INFO_TYPE_BIT_INDEX_ = K_INFO_BIT_INDEX_ + 0;
		const int K_INFO_TYPE_BIT_COUNT_ = 4;
		const uint K_INFO_TYPE_BIT_MASK_ = 0x0F000000;
		static RawVariantType GetType(uint data)
		{
			data &= K_INFO_TYPE_BIT_MASK_;
			data >>= K_INFO_TYPE_BIT_INDEX_;

			return (RawVariantType)data;
		}
		static void SetType(RawVariantType type, ref uint data)
		{
			uint t = (uint)type;
			t <<= K_INFO_TYPE_BIT_INDEX_;
			t &= K_INFO_TYPE_BIT_MASK_;

			data |= t;
		}

		#region Int24
		public static void Int24FromVariant(XmbVariant v, out uint data)
		{
			data = Bitwise.Int24.GetNumber(v.Int);

			if (!v.IsUnsigned)
				data = Bitwise.Int24.SetSigned(data, true);
		}
		public static void Int24ToVariant(ref XmbVariant v, RawVariantFlags f, uint data)
		{
			v.Type = XmbVariantType.INT;
			v.IsUnsigned = (f & RawVariantFlags.UNSIGNED) != 0;

			if (!v.IsUnsigned && Bitwise.Int24.IsSigned(data))
				data |= 0xFF000000;

			v.Int = data;
		}
		#endregion

		static void StringFromVariant(XmbVariant v, ref uint data)
		{
			if (v.IsIndirect)
			{
				data |= v.Offset & K_VALUE_BIT_MASK;
			}
			else if (v.IsUnicode)
			{
				throw new System.IO.InvalidDataException("Unicode should always be indirect");//data |= (ushort)v.Char0;
			}
			else
			{
				data |= (uint)(v.Char0 << 0);
				data |= (uint)(v.Char1 << 8);
				data |= (uint)(v.Char2 << 16);
			}
		}
		static void StringToVariant(ref XmbVariant v, uint data)
		{
			if (v.IsIndirect)
			{
				v.Offset = data;
			}
			else if (v.IsUnicode)
			{
				throw new System.IO.InvalidDataException("Unicode should always be indirect");//v.Char0 = (char)data;
			}
			else
			{
				v.Char0 = (byte)((data >> 0)  & 0xFF);
				v.Char1 = (byte)((data >> 8)  & 0xFF);
				v.Char2 = (byte)((data >> 16) & 0xFF);
			}
		}
		#endregion

		#region Length coding
		enum RawVariantLength : byte
		{
			_1D = 0,
			_2D = 1,
			_3D = 2,
			_4D = 3,
		};

		const int K_INFO_LENGTH_BIT_INDEX_ = K_INFO_TYPE_BIT_INDEX_ + K_INFO_TYPE_BIT_COUNT_;
		const int K_INFO_LENGTH_BIT_COUNT_ = 2;
		const uint K_INFO_LENGTH_BIT_MASK_ = 0x30000000;
		static RawVariantLength GetLength(uint data)
		{
			data &= K_INFO_LENGTH_BIT_MASK_;
			data >>= K_INFO_LENGTH_BIT_INDEX_;

			return (RawVariantLength)data;
		}
		static void SetLength(RawVariantLength length, ref uint data)
		{
			uint l = (uint)length;
			l <<= K_INFO_LENGTH_BIT_INDEX_;
			l &= K_INFO_LENGTH_BIT_MASK_;

			data |= l;
		}

		static RawVariantLength RawLengthFromByte(byte length)
		{
			Contract.Requires(length > 0 && length <= 4);

			RawVariantLength l = (RawVariantLength)(--length);

			return l;
		}
		static byte RawLengthToByte(RawVariantLength length)
		{
			const byte kRebase = 1;

			byte l = (byte)length;
			l += kRebase;

			return l;
		}
		#endregion

		#region Flags coding
		[Flags]
		public enum RawVariantFlags : byte
		{
			UNSIGNED = 1 << 0,
			OFFSET = 1 << 1,
		};

		const int K_INFO_FLAGS_BIT_INDEX_ = K_INFO_LENGTH_BIT_INDEX_ + K_INFO_LENGTH_BIT_COUNT_;
	const uint K_INFO_FLAGS_BIT_MASK_ = 0xC0000000;
		static RawVariantFlags GetFlags(uint data)
		{
			data &= K_INFO_FLAGS_BIT_MASK_;
			data >>= K_INFO_FLAGS_BIT_INDEX_;

			return (RawVariantFlags)data;
		}
		static void SetFlags(RawVariantFlags flags, ref uint data)
		{
			uint f = (uint)flags;
			f <<= K_INFO_TYPE_BIT_INDEX_;
			f &= K_INFO_TYPE_BIT_MASK_;

			data |= f;
		}
		#endregion

		#region RequiresIndirectStorage
		public static bool SingleRequiresIndirectStorage(float single)
		{
			return !SingleFixedPoint.InRange(single) && !Bitwise.Single24.InRange(single);
		}
		public static bool IntRequiresIndirectStorage(uint value)
		{
			return !Bitwise.Int24.InRange(value);
		}
		public static bool StringRequiresIndirectStorage(string s, bool isUnicode)
		{
			return s.Length > 3 || isUnicode;
		}
		#endregion

		#region Read
		static void Compose(out XmbVariant v, uint data)
		{
			v = XmbVariant.Empty;

			RawVariantType type = GetType(data);
			RawVariantLength length = GetLength(data);
			RawVariantFlags flags = GetFlags(data);
			// Get the actual data value
			data &= K_VALUE_BIT_MASK;

			switch (type)
			{
				#region Single
				case RawVariantType.SINGLE24:
					v.Type = XmbVariantType.SINGLE;
					v.Single = Bitwise.Single24.ToSingle(data);
					break;
				case RawVariantType.SINGLE:
					v.Type = XmbVariantType.SINGLE;
					v.IsIndirect = (flags & RawVariantFlags.OFFSET) != 0; // should always be true
					v.Offset = data;
					break;

				case RawVariantType.FIXED_POINT:
					v.Type = XmbVariantType.SINGLE;
					v.Single = SingleFixedPoint.ToSingle(data);
					break;
				#endregion

				#region Int
				case RawVariantType.INT24:
					Int24ToVariant(ref v, flags, data);
					break;
				case RawVariantType.INT:
					v.Type = XmbVariantType.INT;
					v.IsUnsigned = (flags & RawVariantFlags.UNSIGNED) != 0;
					v.IsIndirect = (flags & RawVariantFlags.OFFSET) != 0; // should always be true
					v.Offset = data;
					break;
				#endregion

				#region Double
				case RawVariantType.DOUBLE:
					v.Type = XmbVariantType.DOUBLE;
					v.IsIndirect = (flags & RawVariantFlags.OFFSET) != 0; // should always be true
					v.Offset = data;
					break;
				#endregion

				#region Bool
				case RawVariantType.BOOL:
					v.Type = XmbVariantType.BOOL;
					v.Bool = data != 0;
					break;
				#endregion

				#region String
				case RawVariantType.STRING_ANSI:
					v.Type = XmbVariantType.STRING;
					v.IsIndirect = (flags & RawVariantFlags.OFFSET) != 0;
					v.IsUnicode = false;
					break;
				case RawVariantType.STRING_UNICODE:
					v.Type = XmbVariantType.STRING;
					v.IsIndirect = (flags & RawVariantFlags.OFFSET) != 0;
					v.IsUnicode = true;
					break;
				#endregion

				#region Vector
				case RawVariantType.VECTOR:
					v.Type = XmbVariantType.VECTOR;
					v.VectorLength = RawLengthToByte(length);
					v.IsIndirect = (flags & RawVariantFlags.OFFSET) != 0; // should always be true
					v.Offset = data;
					break;
				#endregion
			}

			if (v.Type == XmbVariantType.STRING)
				StringToVariant(ref v, data);
		}
		public static void Read(IO.EndianReader s, out XmbVariant v)
		{
			uint data = s.ReadUInt32();

			Compose(out v, data);
		}
		#endregion

		#region Write
		static void DecomposeSingle(XmbVariant v, out RawVariantType t, out uint data)
		{
			t = RawVariantType.SINGLE;
			float single = v.Single;

			if (SingleFixedPoint.InRange(single))
			{
				t = RawVariantType.FIXED_POINT;
				data = SingleFixedPoint.FromSingle(single);
			}
			else if (Bitwise.Single24.InRange(single))
			{
				t = RawVariantType.SINGLE24;
				data = Bitwise.Single24.FromSingle(single);
			}
			else
				data = v.Offset;
		}
		static void DecomposeInt(XmbVariant v, out RawVariantType t, ref RawVariantFlags f, out uint data)
		{
			t = RawVariantType.INT;
			if (v.IsUnsigned) f |= RawVariantFlags.UNSIGNED;
			data = v.Int;

			if (Bitwise.Int24.InRange(v.Int))
			{
				t = RawVariantType.INT24;
				Int24FromVariant(v, out data);
			}
		}
		static void Decompose(XmbVariant v,
			out RawVariantType t, out RawVariantLength l, out RawVariantFlags f, out uint data)
		{
			t = RawVariantType.NULL;
			l = (RawVariantLength)byte.MinValue;
			f = (RawVariantFlags)byte.MinValue;
			data = 0;

			bool isIndirect = v.IsIndirect;
			bool isUnsigned = v.Type == XmbVariantType.INT && v.IsUnsigned;

			switch (v.Type)
			{
				case XmbVariantType.SINGLE:
					DecomposeSingle(v, out t, out data);
					break;

				case XmbVariantType.INT:
					DecomposeInt(v, out t, ref f, out data);
					break;

				case XmbVariantType.DOUBLE: // double is always indirect
					t = RawVariantType.DOUBLE;
					break;

				case XmbVariantType.BOOL:
					t = RawVariantType.BOOL;
					data = v.Bool ? 1U : 0U;
					break;

				case XmbVariantType.STRING:
					t = v.IsUnicode ? RawVariantType.STRING_UNICODE : RawVariantType.STRING_ANSI;
					StringFromVariant(v, ref data);
					break;

				case XmbVariantType.VECTOR: // Vector is always indirect
					t = RawVariantType.VECTOR;
					l = RawLengthFromByte(v.VectorLength);
					break;
			}

			if (isIndirect)
				data = v.Offset & K_VALUE_BIT_MASK;
		}
		public static void Write(IO.EndianWriter s, XmbVariant v)
		{
			uint data = 0;

			RawVariantType t;
			RawVariantLength l;
			RawVariantFlags f;
			Decompose(v, out t, out l, out f, out data);

			SetType(t, ref data);
			SetLength(l, ref data);
			SetFlags(f, ref data);

			s.Write(data);
		}
		#endregion
	};
}