using System;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft
{
	partial class Bits
	{
		#region Bit Vector length calculations
		/// <summary>Calculates how many <see cref="System.Byte"/>s are needed to hold a bit vector of a certain length</summary>
		/// <param name="bitsCount">Number of bits to be hosted in the vector</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int VectorLengthInBytes(int bitsCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitsCount >= 0);

			return (bitsCount + (K_BYTE_BIT_COUNT-1)) >> K_BYTE_BIT_SHIFT;
		}

		/// <summary>Calculates how many <see cref="System.Int16"/>s are needed to hold a bit vector of a certain length</summary>
		/// <param name="bitsCount">Number of bits to be hosted in the vector</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int VectorLengthInInt16(int bitsCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitsCount >= 0);

			return (bitsCount + (K_INT16_BIT_COUNT-1)) >> K_INT16_BIT_SHIFT;
		}

		/// <summary>Calculates how many <see cref="System.Int32"/>s are needed to hold a bit vector of a certain length</summary>
		/// <param name="bitsCount">Number of bits to be hosted in the vector</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int VectorLengthInInt32(int bitsCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitsCount >= 0);

			return (bitsCount + (K_INT32_BIT_COUNT-1)) >> K_INT32_BIT_SHIFT;
		}

		/// <summary>Calculates how many <see cref="System.Int64"/>s are needed to hold a bit vector of a certain length</summary>
		/// <param name="bitsCount">Number of bits to be hosted in the vector</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int VectorLengthInInt64(int bitsCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitsCount >= 0);

			return (bitsCount + (K_INT64_BIT_COUNT-1)) >> K_INT64_BIT_SHIFT;
		}

		#endregion

		#region Bit Vector element bitmask (kVectorWordFormat dependent)
		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Byte"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static byte VectorElementBitMaskInBytesLe(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);
			const byte kOne = 1;

			return (byte)(kOne << (bitIndex % K_BYTE_BIT_COUNT));
		}
		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Byte"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static byte VectorElementBitMaskInBytesBe(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);
			const byte kOne = 1;
			const byte kMostSignificantBit = kOne << (K_BYTE_BIT_COUNT - 1);

			return (byte)(kMostSignificantBit >> (bitIndex % K_BYTE_BIT_COUNT));
		}
		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Byte"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static byte VectorElementBitMaskInBytes(int bitIndex,
			Shell.EndianFormat byteOrder = K_VECTOR_WORD_FORMAT)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			return byteOrder == Shell.EndianFormat.BIG
				? VectorElementBitMaskInBytesBe(bitIndex)
				: VectorElementBitMaskInBytesLe(bitIndex);
		}
		/// <summary>Get the procedure for building a mask of a specific bit in a vector, relative to the vector's element size (<see cref="System.Byte"/>)</summary>
		/// <param name="proc"></param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		public static void GetVectorElementBitMaskInT(out VectorElementBitMask<byte> proc,
			Shell.EndianFormat byteOrder = K_VECTOR_WORD_FORMAT)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			proc = byteOrder == Shell.EndianFormat.BIG
				? (VectorElementBitMask<byte>)VectorElementBitMaskInBytesBe
				: (VectorElementBitMask<byte>)VectorElementBitMaskInBytesLe;
		}

		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Int16"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static ushort VectorElementBitMaskInInt16Le(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);
			const ushort kOne = 1;

			return (ushort)(kOne << (bitIndex % K_INT16_BIT_COUNT));
		}
		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Int16"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static ushort VectorElementBitMaskInInt16Be(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);
			const ushort kOne = 1;
			const ushort kMostSignificantBit = kOne << (K_INT16_BIT_COUNT - 1);

			return (ushort)(kMostSignificantBit >> (bitIndex % K_INT16_BIT_COUNT));
		}
		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Int16"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static ushort VectorElementBitMaskInInt16(int bitIndex,
			Shell.EndianFormat byteOrder = K_VECTOR_WORD_FORMAT)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			return byteOrder == Shell.EndianFormat.BIG
				? VectorElementBitMaskInInt16Be(bitIndex)
				: VectorElementBitMaskInInt16Le(bitIndex);
		}
		/// <summary>Get the procedure for building a mask of a specific bit in a vector, relative to the vector's element size (<see cref="System.Int16"/>)</summary>
		/// <param name="proc"></param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		public static void GetVectorElementBitMaskInT(out VectorElementBitMask<ushort> proc,
			Shell.EndianFormat byteOrder = K_VECTOR_WORD_FORMAT)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			proc = byteOrder == Shell.EndianFormat.BIG
				? (VectorElementBitMask<ushort>)VectorElementBitMaskInInt16Be
				: (VectorElementBitMask<ushort>)VectorElementBitMaskInInt16Le;
		}

		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Int32"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static uint VectorElementBitMaskInInt32Le(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);
			const uint kOne = 1;

			return (uint)(kOne << (bitIndex % K_INT32_BIT_COUNT));
		}
		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Int32"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static uint VectorElementBitMaskInInt32Be(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);
			const uint kOne = 1;
			const uint kMostSignificantBit = kOne << (K_INT32_BIT_COUNT - 1);

			return (uint)(kMostSignificantBit >> (bitIndex % K_INT32_BIT_COUNT));
		}
		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Int32"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static uint VectorElementBitMaskInInt32(int bitIndex,
			Shell.EndianFormat byteOrder = K_VECTOR_WORD_FORMAT)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			return byteOrder == Shell.EndianFormat.BIG
				? VectorElementBitMaskInInt32Be(bitIndex)
				: VectorElementBitMaskInInt32Le(bitIndex);
		}
		/// <summary>Get the procedure for building a mask of a specific bit in a vector, relative to the vector's element size (<see cref="System.Int32"/>)</summary>
		/// <param name="proc"></param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		public static void GetVectorElementBitMaskInT(out VectorElementBitMask<uint> proc,
			Shell.EndianFormat byteOrder = K_VECTOR_WORD_FORMAT)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			proc = byteOrder == Shell.EndianFormat.BIG
				? (VectorElementBitMask<uint>)VectorElementBitMaskInInt32Be
				: (VectorElementBitMask<uint>)VectorElementBitMaskInInt32Le;
		}

		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Int64"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static ulong VectorElementBitMaskInInt64Le(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);
			const ulong kOne = 1;

			return (ulong)(kOne << (bitIndex % K_INT64_BIT_COUNT));
		}
		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Int64"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static ulong VectorElementBitMaskInInt64Be(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);
			const ulong kOne = 1;
			const ulong kMostSignificantBit = kOne << (K_INT64_BIT_COUNT - 1);

			return (ulong)(kMostSignificantBit >> (bitIndex % K_INT64_BIT_COUNT));
		}
		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Int64"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static ulong VectorElementBitMaskInInt64(int bitIndex,
			Shell.EndianFormat byteOrder = K_VECTOR_WORD_FORMAT)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			return byteOrder == Shell.EndianFormat.BIG
				? VectorElementBitMaskInInt64Be(bitIndex)
				: VectorElementBitMaskInInt64Le(bitIndex);
		}
		/// <summary>Get the procedure for building a mask of a specific bit in a vector, relative to the vector's element size (<see cref="System.Int64"/>)</summary>
		/// <param name="proc"></param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		public static void GetVectorElementBitMaskInT(out VectorElementBitMask<ulong> proc,
			Shell.EndianFormat byteOrder = K_VECTOR_WORD_FORMAT)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			proc = byteOrder == Shell.EndianFormat.BIG
				? (VectorElementBitMask<ulong>)VectorElementBitMaskInInt64Be
				: (VectorElementBitMask<ulong>)VectorElementBitMaskInInt64Le;
		}

		#endregion

		#region Bit Vector element section bitmask (kVectorWordFormat dependent)
		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Byte"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static byte VectorElementSectionBitMaskInBytesLe(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return (byte)(byte.MaxValue << startBitIndex);
		}
		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Byte"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static byte VectorElementSectionBitMaskInBytesBe(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return (byte)(byte.MaxValue >> startBitIndex);
		}
		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Byte"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static byte VectorElementSectionBitMaskInBytes(int startBitIndex,
			Shell.EndianFormat byteOrder = K_VECTOR_WORD_FORMAT)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return byteOrder == Shell.EndianFormat.BIG
				? VectorElementSectionBitMaskInBytesBe(startBitIndex)
				: VectorElementSectionBitMaskInBytesLe(startBitIndex);
		}
		/// <summary>Get the procedure for building a mask of a section of bits in a vector, relative to the vector's element size (<see cref="System.Byte"/>)</summary>
		/// <param name="proc"></param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		public static void GetVectorElementSectionBitMaskInT(out VectorElementBitMask<byte> proc,
			Shell.EndianFormat byteOrder = K_VECTOR_WORD_FORMAT)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			proc = byteOrder == Shell.EndianFormat.BIG
				? (VectorElementBitMask<byte>)VectorElementSectionBitMaskInBytesBe
				: (VectorElementBitMask<byte>)VectorElementSectionBitMaskInBytesLe;
		}

		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Int16"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static ushort VectorElementSectionBitMaskInInt16Le(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return (ushort)(ushort.MaxValue << startBitIndex);
		}
		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Int16"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static ushort VectorElementSectionBitMaskInInt16Be(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return (ushort)(ushort.MaxValue >> startBitIndex);
		}
		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Int16"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static ushort VectorElementSectionBitMaskInInt16(int startBitIndex,
			Shell.EndianFormat byteOrder = K_VECTOR_WORD_FORMAT)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return byteOrder == Shell.EndianFormat.BIG
				? VectorElementSectionBitMaskInInt16Be(startBitIndex)
				: VectorElementSectionBitMaskInInt16Le(startBitIndex);
		}
		/// <summary>Get the procedure for building a mask of a section of bits in a vector, relative to the vector's element size (<see cref="System.Int16"/>)</summary>
		/// <param name="proc"></param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		public static void GetVectorElementSectionBitMaskInT(out VectorElementBitMask<ushort> proc,
			Shell.EndianFormat byteOrder = K_VECTOR_WORD_FORMAT)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			proc = byteOrder == Shell.EndianFormat.BIG
				? (VectorElementBitMask<ushort>)VectorElementSectionBitMaskInInt16Be
				: (VectorElementBitMask<ushort>)VectorElementSectionBitMaskInInt16Le;
		}

		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Int32"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static uint VectorElementSectionBitMaskInInt32Le(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return (uint)(uint.MaxValue << startBitIndex);
		}
		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Int32"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static uint VectorElementSectionBitMaskInInt32Be(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return (uint)(uint.MaxValue >> startBitIndex);
		}
		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Int32"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static uint VectorElementSectionBitMaskInInt32(int startBitIndex,
			Shell.EndianFormat byteOrder = K_VECTOR_WORD_FORMAT)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return byteOrder == Shell.EndianFormat.BIG
				? VectorElementSectionBitMaskInInt32Be(startBitIndex)
				: VectorElementSectionBitMaskInInt32Le(startBitIndex);
		}
		/// <summary>Get the procedure for building a mask of a section of bits in a vector, relative to the vector's element size (<see cref="System.Int32"/>)</summary>
		/// <param name="proc"></param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		public static void GetVectorElementSectionBitMaskInT(out VectorElementBitMask<uint> proc,
			Shell.EndianFormat byteOrder = K_VECTOR_WORD_FORMAT)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			proc = byteOrder == Shell.EndianFormat.BIG
				? (VectorElementBitMask<uint>)VectorElementSectionBitMaskInInt32Be
				: (VectorElementBitMask<uint>)VectorElementSectionBitMaskInInt32Le;
		}

		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Int64"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static ulong VectorElementSectionBitMaskInInt64Le(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return (ulong)(ulong.MaxValue << startBitIndex);
		}
		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Int64"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static ulong VectorElementSectionBitMaskInInt64Be(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return (ulong)(ulong.MaxValue >> startBitIndex);
		}
		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Int64"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static ulong VectorElementSectionBitMaskInInt64(int startBitIndex,
			Shell.EndianFormat byteOrder = K_VECTOR_WORD_FORMAT)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return byteOrder == Shell.EndianFormat.BIG
				? VectorElementSectionBitMaskInInt64Be(startBitIndex)
				: VectorElementSectionBitMaskInInt64Le(startBitIndex);
		}
		/// <summary>Get the procedure for building a mask of a section of bits in a vector, relative to the vector's element size (<see cref="System.Int64"/>)</summary>
		/// <param name="proc"></param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		public static void GetVectorElementSectionBitMaskInT(out VectorElementBitMask<ulong> proc,
			Shell.EndianFormat byteOrder = K_VECTOR_WORD_FORMAT)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			proc = byteOrder == Shell.EndianFormat.BIG
				? (VectorElementBitMask<ulong>)VectorElementSectionBitMaskInInt64Be
				: (VectorElementBitMask<ulong>)VectorElementSectionBitMaskInInt64Le;
		}

		#endregion

		#region Bit Vector element from byte[]
		public static void VectorElementFromBufferInT(byte[] buffer, int index, ref byte element)
		{
			Contract.Requires/*<ArgumentNullException>*/(buffer != null);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(index >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(index+sizeof(byte) <= buffer.Length);

			element = buffer[index];
		}
		public static void GetVectorElementFromBufferInT(out VectorElementFromBuffer<byte> proc)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			proc = VectorElementFromBufferInT;
		}

		public static void VectorElementFromBufferInT(byte[] buffer, int index, ref ushort element)
		{
			Contract.Requires/*<ArgumentNullException>*/(buffer != null);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(index >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(index+sizeof(ushort) <= buffer.Length);

			element = BitConverter.ToUInt16(buffer, index);
		}
		public static void GetVectorElementFromBufferInT(out VectorElementFromBuffer<ushort> proc)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			proc = VectorElementFromBufferInT;
		}

		public static void VectorElementFromBufferInT(byte[] buffer, int index, ref uint element)
		{
			Contract.Requires/*<ArgumentNullException>*/(buffer != null);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(index >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(index+sizeof(uint) <= buffer.Length);

			element = BitConverter.ToUInt32(buffer, index);
		}
		public static void GetVectorElementFromBufferInT(out VectorElementFromBuffer<uint> proc)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			proc = VectorElementFromBufferInT;
		}

		public static void VectorElementFromBufferInT(byte[] buffer, int index, ref ulong element)
		{
			Contract.Requires/*<ArgumentNullException>*/(buffer != null);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(index >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(index+sizeof(ulong) <= buffer.Length);

			element = BitConverter.ToUInt64(buffer, index);
		}
		public static void GetVectorElementFromBufferInT(out VectorElementFromBuffer<ulong> proc)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			proc = VectorElementFromBufferInT;
		}

		#endregion

		#region Bit Vector bitIndex to vector_index
		/// <summary>Get the vector index of a bit index, for a vector represented in <see cref="System.Byte"/>s</summary>
		/// <param name="bitIndex">Index of the bit which we want the vector index of</param>
		/// <returns>The index of a <see cref="System.Byte"/> which holds the bit in question</returns>
		[Contracts.Pure]
		public static int VectorIndexInBytes(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			return bitIndex >> K_BYTE_BIT_SHIFT;
		}

		/// <summary>Get the vector index of a bit index, for a vector represented in <see cref="System.Int16"/>s</summary>
		/// <param name="bitIndex">Index of the bit which we want the vector index of</param>
		/// <returns>The index of a <see cref="System.Int16"/> which holds the bit in question</returns>
		[Contracts.Pure]
		public static int VectorIndexInInt16(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			return bitIndex >> K_INT16_BIT_SHIFT;
		}

		/// <summary>Get the vector index of a bit index, for a vector represented in <see cref="System.Int32"/>s</summary>
		/// <param name="bitIndex">Index of the bit which we want the vector index of</param>
		/// <returns>The index of a <see cref="System.Int32"/> which holds the bit in question</returns>
		[Contracts.Pure]
		public static int VectorIndexInInt32(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			return bitIndex >> K_INT32_BIT_SHIFT;
		}

		/// <summary>Get the vector index of a bit index, for a vector represented in <see cref="System.Int64"/>s</summary>
		/// <param name="bitIndex">Index of the bit which we want the vector index of</param>
		/// <returns>The index of a <see cref="System.Int64"/> which holds the bit in question</returns>
		[Contracts.Pure]
		public static int VectorIndexInInt64(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			return bitIndex >> K_INT64_BIT_SHIFT;
		}

		#endregion

		#region Bit Vector cursor to bitIndex
		/// <summary>Calculates the bit position of a vector cursor based on <see cref="System.Byte"/> elements</summary>
		/// <param name="index">Element index of the cursor</param>
		/// <param name="bitOffset">Element bit offset of the current</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int VectorBitIndexInBytes(int index, int bitOffset)
		{
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(bitOffset >= 0);

			return (index << K_BYTE_BIT_SHIFT) + bitOffset;
		}

		/// <summary>Calculates the bit position of a vector cursor based on <see cref="System.Int16"/> elements</summary>
		/// <param name="index">Element index of the cursor</param>
		/// <param name="bitOffset">Element bit offset of the current</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int VectorBitIndexInInt16(int index, int bitOffset)
		{
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(bitOffset >= 0);

			return (index << K_INT16_BIT_SHIFT) + bitOffset;
		}

		/// <summary>Calculates the bit position of a vector cursor based on <see cref="System.Int32"/> elements</summary>
		/// <param name="index">Element index of the cursor</param>
		/// <param name="bitOffset">Element bit offset of the current</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int VectorBitIndexInInt32(int index, int bitOffset)
		{
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(bitOffset >= 0);

			return (index << K_INT32_BIT_SHIFT) + bitOffset;
		}

		/// <summary>Calculates the bit position of a vector cursor based on <see cref="System.Int64"/> elements</summary>
		/// <param name="index">Element index of the cursor</param>
		/// <param name="bitOffset">Element bit offset of the current</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int VectorBitIndexInInt64(int index, int bitOffset)
		{
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(bitOffset >= 0);

			return (index << K_INT64_BIT_SHIFT) + bitOffset;
		}

		#endregion

		#region Bit Vector cursor from bitIndex
		/// <summary>Calculates the vector cursor based on a bit index in a <see cref="System.Byte"/> vector</summary>
		/// <param name="bitIndex">Index to translate into a cursor</param>
		/// <param name="index">Element index of the cursor</param>
		/// <param name="bitOffset">Element bit offset of the current</param>
		public static void VectorBitCursorInBytes(int bitIndex, out int index, out int bitOffset)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			index = VectorIndexInBytes(bitIndex);
			bitOffset = bitIndex & K_BYTE_BIT_MOD;
		}

		/// <summary>Calculates the vector cursor based on a bit index in a <see cref="System.Int16"/> vector</summary>
		/// <param name="bitIndex">Index to translate into a cursor</param>
		/// <param name="index">Element index of the cursor</param>
		/// <param name="bitOffset">Element bit offset of the current</param>
		public static void VectorBitCursorInInt16(int bitIndex, out int index, out int bitOffset)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			index = VectorIndexInInt16(bitIndex);
			bitOffset = bitIndex & K_INT16_BIT_MOD;
		}

		/// <summary>Calculates the vector cursor based on a bit index in a <see cref="System.Int32"/> vector</summary>
		/// <param name="bitIndex">Index to translate into a cursor</param>
		/// <param name="index">Element index of the cursor</param>
		/// <param name="bitOffset">Element bit offset of the current</param>
		public static void VectorBitCursorInInt32(int bitIndex, out int index, out int bitOffset)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			index = VectorIndexInInt32(bitIndex);
			bitOffset = bitIndex & K_INT32_BIT_MOD;
		}

		/// <summary>Calculates the vector cursor based on a bit index in a <see cref="System.Int64"/> vector</summary>
		/// <param name="bitIndex">Index to translate into a cursor</param>
		/// <param name="index">Element index of the cursor</param>
		/// <param name="bitOffset">Element bit offset of the current</param>
		public static void VectorBitCursorInInt64(int bitIndex, out int index, out int bitOffset)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			index = VectorIndexInInt64(bitIndex);
			bitOffset = bitIndex & K_INT64_BIT_MOD;
		}

		#endregion
	};
}