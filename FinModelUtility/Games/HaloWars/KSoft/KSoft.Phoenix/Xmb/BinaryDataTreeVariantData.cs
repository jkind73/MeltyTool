using System;
using Interop = System.Runtime.InteropServices;

namespace KSoft.Phoenix.Xmb
{
	using BDTypeDesc = BinaryDataTreeVariantTypeDesc;

	[Interop.StructLayout(Interop.LayoutKind.Explicit)]
	public struct BinaryDataTreeVariantData
	{
		#region Direct Data
		[Interop.FieldOffset(0)]
		public bool Bool;
		[Interop.FieldOffset(0)]
		public uint Offset;
		[Interop.FieldOffset(0)]
		public uint Int;
		[Interop.FieldOffset(0)]
		public ulong Int64;
		[Interop.FieldOffset(0)]
		public float Single;
		[Interop.FieldOffset(0)]
		public double Double;
		#endregion

		[Interop.FieldOffset(8)]
		public BDTypeDesc TypeDesc;
		[Interop.FieldOffset(12)]
		public int ArrayLength;

		// String must come last, because we don't know how big a .NET reference really is (we could be compiling for x64!)
		[Interop.FieldOffset(16)]
		public string String;
		[Interop.FieldOffset(16)]
		public Array OpaqueArrayRef;

		public BinaryDataTreeVariantType Type { get { return this.TypeDesc.type; } }
		public bool IsUnicode { get { return this.TypeDesc.IsUnicode; } }

		public bool UseDirectEncoding { get { return this.TypeDesc.SizeOf <= sizeof(uint) && this.ArrayLength <= 1; } }

		public int StringOrArrayLength { get {
			if (this.Type == BinaryDataTreeVariantType.STRING)
				return this.String != null ? this.String.Length : 0;

			return this.OpaqueArrayRef != null ? this.OpaqueArrayRef.Length : 0;
		} }

		internal void Read(BinaryDataTreeMemoryPool pool, BinaryDataTreeNameValue nameValue)
		{
			this.TypeDesc = nameValue.GuessTypeDesc();

			bool directEncoding = nameValue.DirectEncoding;

			uint totalDataSize = nameValue.Size;
			if (nameValue.SizeIsIndirect)
			{
				if (directEncoding)
					throw new InvalidOperationException();

				totalDataSize = pool.GetSizeValue(nameValue.Offset);

				if (totalDataSize < BinaryDataTreeNameValue.K_INDIRECT_SIZE_THRESHOLD)
					throw new InvalidOperationException();
			}

			if (this.TypeDesc.SizeOf > 0)
			{
				if ((totalDataSize % this.TypeDesc.SizeOf) != 0)
					throw new InvalidOperationException(nameValue.ToString());

				this.ArrayLength = (int)(totalDataSize / this.TypeDesc.SizeOf);
			}

			if (this.ArrayLength > 1)
			{
				if (this.Type != BinaryDataTreeVariantType.STRING)
					this.OpaqueArrayRef = this.TypeDesc.MakeArray(this.ArrayLength);

				pool.InternalBuffer.Seek(nameValue.Offset);
			}

			switch (this.Type)
			{
				case BinaryDataTreeVariantType.NULL:
					break;

				case BinaryDataTreeVariantType.BOOL:
					if (this.ArrayLength > 1)
						this.TypeDesc.ReadArray(pool.InternalBuffer, this.OpaqueArrayRef);
					else
						this.Bool = nameValue.Bool;
					break;

				case BinaryDataTreeVariantType.INT:
					if (this.ArrayLength > 1)
						this.TypeDesc.ReadArray(pool.InternalBuffer, this.OpaqueArrayRef);
					else
					{
						if (this.TypeDesc.SizeOf < sizeof(long))
							this.Int = nameValue.Int;
						else
							this.Int64 = pool.InternalBuffer.ReadUInt64();
					}
					break;

				case BinaryDataTreeVariantType.FLOAT:
					if (this.ArrayLength > 1)
						this.TypeDesc.ReadArray(pool.InternalBuffer, this.OpaqueArrayRef);
					else
					{
						if (this.TypeDesc.SizeOf < sizeof(double))
							this.Single = nameValue.Single;
						else
							this.Double = pool.InternalBuffer.ReadDouble();
					}
					break;

				case BinaryDataTreeVariantType.STRING:
					this.ArrayLength = 1;
					if (!this.IsUnicode && totalDataSize <= sizeof(uint))
					{
						var sb = new System.Text.StringBuilder();
						for (uint x = 0, v = nameValue.Int; x < sizeof(uint); x++, v >>= Bits.K_BYTE_BIT_COUNT)
						{
							sb.Append((char)(v & 0xFF));
						}

						this.String = sb.ToString();
					}
					else
					{
						this.String = pool.InternalBuffer.ReadString(this.IsUnicode
							? Memory.Strings.StringStorage.CStringUnicode
							: Memory.Strings.StringStorage.CStringAscii);
					}
					break;

				default: throw new KSoft.Debug.UnreachableException(this.Type.ToString());
			}
		}

		public void ToStream<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (this.Type == BinaryDataTreeVariantType.NULL)
				return;

			this.TypeDesc.ToStream(s, this.ArrayLength);

			if (this.ArrayLength > 1 && this.Type != BinaryDataTreeVariantType.STRING)
			{
				var arrayStr = this.TypeDesc.ArrayToString(this.OpaqueArrayRef);
				s.WriteCursor(arrayStr);
				return;
			}

			switch (this.Type)
			{
				case BinaryDataTreeVariantType.BOOL:
					s.WriteCursor(this.Bool);
					break;

				case BinaryDataTreeVariantType.INT:
					s.WriteCursor(this.Int64);
					break;

				case BinaryDataTreeVariantType.FLOAT:
					if (this.TypeDesc.SizeOf < sizeof(double))
						s.WriteCursor(this.Single);
					else
						s.WriteCursor(this.Double);
					break;

				case BinaryDataTreeVariantType.STRING:
					if (this.String.IsNotNullOrEmpty())
						s.WriteCursor(this.String);
					break;

				default: throw new KSoft.Debug.UnreachableException(this.Type.ToString());
			}
		}

		public void ToStreamAsAttribute<TDoc, TCursor>(string attributeName, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (this.Type == BinaryDataTreeVariantType.NULL)
				return;

			if (this.ArrayLength > 1 && this.Type != BinaryDataTreeVariantType.STRING)
			{
				var arrayStr = this.TypeDesc.ArrayToString(this.OpaqueArrayRef);
				s.WriteAttribute(attributeName, arrayStr);
				return;
			}

			switch (this.Type)
			{
				case BinaryDataTreeVariantType.BOOL:
					s.WriteAttribute(attributeName, this.Bool);
					break;

				case BinaryDataTreeVariantType.INT:
					s.WriteAttribute(attributeName, this.Int64);
					break;

				case BinaryDataTreeVariantType.FLOAT:
					if (this.TypeDesc.SizeOf < sizeof(double))
						s.WriteAttribute(attributeName, this.Single);
					else
						s.WriteAttribute(attributeName, this.Double);
					break;

				case BinaryDataTreeVariantType.STRING:
					if (this.String.IsNotNullOrEmpty())
						s.WriteAttribute(attributeName, this.String);
					break;

				default: throw new KSoft.Debug.UnreachableException(this.Type.ToString());
			}
		}

		public void FromStream<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			this.TypeDesc = BDTypeDesc.GuessFromStream(s, out this.ArrayLength);

			// #TODO
			throw new NotImplementedException();
		}
	};
}