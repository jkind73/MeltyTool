using Interop = System.Runtime.InteropServices;

using Vector2f = System.Numerics.Vector2;
using Vector3f = System.Numerics.Vector3;
using Vector4f = System.Numerics.Vector4;

namespace KSoft.Phoenix.Xmb
{
	partial class XmbVariantMemoryPool
	{
		[Interop.StructLayout(Interop.LayoutKind.Explicit)]
		sealed class PoolEntry
			: IO.IEndianStreamable
		{
			static readonly Text.StringStorageEncoding kAnsiEncoding =
				Text.StringStorageEncoding.TryAndGetStaticEncoding(Memory.Strings.StringStorage.CStringAscii);
			static readonly Text.StringStorageEncoding kUnicodeEncoding =
				Text.StringStorageEncoding.TryAndGetStaticEncoding(Memory.Strings.StringStorage.CStringUnicode);

			[Interop.FieldOffset(0)]
			public uint Int;
			[Interop.FieldOffset(0)]
			public float Single;
			[Interop.FieldOffset(0)]
			public double Double;
			[Interop.FieldOffset(0)]
			public Vector2f Vector2d;
			[Interop.FieldOffset(0)]
			public Vector3f Vector3d;
			[Interop.FieldOffset(0)]
			public Vector4f Vector4d = new Vector4f();

			// we don't know how big a .NET reference really is (we could be compiling for x64!) so always give it 8 bytes
			[Interop.FieldOffset(16)]
			public string String;

			[Interop.FieldOffset(24)]
			public XmbVariantType Type;

			// These are all type dependent so we reuse the memory space
			[Interop.FieldOffset(24 + 1)]
			public byte VectorLength;
//			[System.Runtime.InteropServices.FieldOffset(24 + 1)]
//			public bool IsUnsigned;
			[Interop.FieldOffset(24 + 1)]
			public bool IsUnicode;

			// Amount of padding to prefix this entry with when written
			[Interop.FieldOffset(24 + 2)]
			public byte PrePadSize;

			#region Equals
			public bool Equals(uint v)		{ return this.Type == XmbVariantType.INT && this.Int == v; }
			public bool Equals(int v)		{ return this.Type == XmbVariantType.INT && this.Int == (uint)v; }
			public bool Equals(float v)		{ return this.Type == XmbVariantType.SINGLE && this.Single == v; }
			public bool Equals(double v)	{ return this.Type == XmbVariantType.DOUBLE && this.Double == v; }
			public bool Equals(string v)	{ return this.Type == XmbVariantType.STRING && this.String == v; }
			public bool Equals(Vector2f v)	{ return this.Type == XmbVariantType.VECTOR && this.VectorLength == 2 && this.Vector2d == v; }
			public bool Equals(Vector3f v)	{ return this.Type == XmbVariantType.VECTOR && this.VectorLength == 3 && this.Vector3d == v; }
			public bool Equals(Vector4f v)	{ return this.Type == XmbVariantType.VECTOR && this.VectorLength == 4 && this.Vector4d == v; }
			#endregion
			#region New
			public static PoolEntry New(uint v)		{ return new PoolEntry() { Type = XmbVariantType.INT, Int = v }; }
			public static PoolEntry New(int v)		{ return new PoolEntry() { Type = XmbVariantType.INT, Int = (uint)v }; }
			public static PoolEntry New(float v)	{ return new PoolEntry() { Type = XmbVariantType.SINGLE, Single = v }; }
			public static PoolEntry New(double v)	{ return new PoolEntry() { Type = XmbVariantType.DOUBLE, Double = v }; }
			public static PoolEntry New(string v)	{ return new PoolEntry() { Type = XmbVariantType.STRING, String = v }; }
			public static PoolEntry New(Vector2f v)	{ return new PoolEntry() { Type = XmbVariantType.VECTOR, VectorLength = 2, Vector2d = v }; }
			public static PoolEntry New(Vector3f v)	{ return new PoolEntry() { Type = XmbVariantType.VECTOR, VectorLength = 3, Vector3d = v }; }
			public static PoolEntry New(Vector4f v)	{ return new PoolEntry() { Type = XmbVariantType.VECTOR, VectorLength = 4, Vector4d = v }; }
			public static PoolEntry New(XmbVariantType t)	{ return new PoolEntry() { Type = t }; }
			#endregion

			public uint CalculateSize()
			{
				switch (this.Type)
				{
				case XmbVariantType.INT:
				case XmbVariantType.SINGLE:
					return sizeof(uint);
				case XmbVariantType.DOUBLE:
					return sizeof(ulong);
				case XmbVariantType.STRING:
					var sse = this.IsUnicode ? kUnicodeEncoding : kAnsiEncoding;
					return (uint)sse.GetByteCount(this.String);
				case XmbVariantType.VECTOR:
					return (uint)(sizeof(uint) * this.VectorLength);

				default: throw new KSoft.Debug.UnreachableException(this.Type.ToString());
				}
			}

			public uint CalculatePadding(uint offset)
			{
				const int kAlignmentBit = IntegerMath.K_INT32_ALIGNMENT_BIT;
				this.PrePadSize = 0;

				if (this.Type != XmbVariantType.STRING)
					this.PrePadSize = (byte)IntegerMath.PaddingRequired(kAlignmentBit, offset);

				return this.PrePadSize;
			}

			#region IEndianStreamable Members
			public void Read(IO.EndianReader s)
			{
				switch (this.Type)
				{
				case XmbVariantType.INT:    s.Read(out this.Int); break;
				case XmbVariantType.SINGLE: s.Read(out this.Single); break;
				case XmbVariantType.DOUBLE: s.Read(out this.Double); break;
				case XmbVariantType.STRING:
					this.String = s.ReadString(this.IsUnicode ? kUnicodeEncoding : kAnsiEncoding);
					break;
				case XmbVariantType.VECTOR:
					if (this.VectorLength >= 1) s.Read(out this.Vector4d.X);
					if (this.VectorLength >= 2) s.Read(out this.Vector4d.Y);
					if (this.VectorLength >= 3) s.Read(out this.Vector4d.Z);
					if (this.VectorLength >= 4) s.Read(out this.Vector4d.W);
					break;

				default: throw new KSoft.Debug.UnreachableException(this.Type.ToString());
				}
			}

			public void Write(IO.EndianWriter s)
			{
				if (this.PrePadSize > 0)
					for (int x = 0; x < this.PrePadSize; x++)
						s.Write(byte.MinValue);

				switch (this.Type)
				{
				case XmbVariantType.INT:    s.Write(this.Int); break;
				case XmbVariantType.SINGLE: s.Write(this.Single); break;
				case XmbVariantType.DOUBLE: s.Write(this.Double); break;
				case XmbVariantType.STRING:
					s.Write(this.String, this.IsUnicode ? kUnicodeEncoding : kAnsiEncoding);
					break;
				case XmbVariantType.VECTOR:
					if (this.VectorLength >= 1) s.Write(this.Vector4d.X);
					if (this.VectorLength >= 2) s.Write(this.Vector4d.Y);
					if (this.VectorLength >= 3) s.Write(this.Vector4d.Z);
					if (this.VectorLength >= 4) s.Write(this.Vector4d.W);
					break;

				default: throw new KSoft.Debug.UnreachableException(this.Type.ToString());
				}
			}
			#endregion
		};
	};
}