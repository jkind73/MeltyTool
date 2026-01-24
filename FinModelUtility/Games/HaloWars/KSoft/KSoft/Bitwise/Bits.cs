using System;
using System.Diagnostics.CodeAnalysis;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft
{
	/// <summary>Utility class for bit level manipulation</summary>
	// Reference: http://graphics.stanford.edu/~seander/bithacks.html
	// Also, http://stackoverflow.com/questions/746171/best-algorithm-for-bit-reversal-from-msb-lsb-to-lsb-msb-in-c
	// http://corner.squareup.com/2013/07/reversing-bits-on-arm.html
	public static partial class Bits
	{
		/// <summary>Number of logical bits in a <see cref="System.Boolean"/></summary>
		public const int K_BOOLEAN_BIT_COUNT = 1;

		[Contracts.Pure]
		static int BitmaskLookUpTableGetLength(int wordBitSize)
		{
			// first element in the LUT is zero, followed by a mask for each range of bits up until wordBitSize
			return 1 + wordBitSize;
		}

		// https://en.wikipedia.org/wiki/De_Bruijn_sequence

		#region MultiplyDeBruijnBitPosition
		static readonly byte[] KMultiplyDeBruijnBitPositionHighestBitSet32 = GenerateMultiplyDeBruijnBitPositionHighestBitSet32();
		static readonly byte[] KMultiplyDeBruijnBitPositionLeadingZeros32 = GenerateMultiplyDeBruijnBitPositionLeadingZeros32();
		static readonly byte[] KMultiplyDeBruijnBitPositionTrailingZeros32 = [
			0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
			31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
		];

		static byte[] GenerateMultiplyDeBruijnBitPositionHighestBitSet32()
		{
			return [
				0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30,
				8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31
			];
		}

		static byte[] GenerateMultiplyDeBruijnBitPositionLeadingZeros32()
		{
			var src = GenerateMultiplyDeBruijnBitPositionHighestBitSet32();
			var dst = new byte[K_INT32_BIT_COUNT];
			for (int x = 0; x < dst.Length; x++)
				dst[x] = (byte)(src[x]+1);

			return dst;
		}
		#endregion

		#region Contract messages
		const string K_BIT_SWAP_START_BIT_INDEX_NOT_GREATER_THAN_ZERO_ =
			"Doesn't make sense to bit swap 1 bit. Or to start at a negative index";

		const string K_GET_MAX_ENUM_BITS_MAX_VALUE_OUT_OF_RANGE_MESSAGE_ = "There is no point in this if '0' is the only option";

		const string K_GET_BITMASK_ENUM_MAX_VALUE_OUT_OF_RANGE_MESSAGE_ = K_GET_MAX_ENUM_BITS_MAX_VALUE_OUT_OF_RANGE_MESSAGE_;
		const string K_GET_BITMASK_FLAG_MAX_VALUE_OUT_OF_RANGE_MESSAGE_ = K_GET_MAX_ENUM_BITS_MAX_VALUE_OUT_OF_RANGE_MESSAGE_;
		#endregion

		#region Memory/ArrayCopy
		// #REVIEW: Does #DOTNET5 enable us to change this to a class and use stackalloc?
		[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
		public struct MemoryCopier<TDst, TSrc>
			where TDst : struct
			where TSrc : struct
		{
			// As long as no one uses the default constructor, the cctor should be ran before instance code
			// http://stackoverflow.com/a/3246817/444977
			static MemoryCopier()
			{
				var dstType = typeof(TDst);
				var srcType = typeof(TSrc);

				if (!dstType.IsPrimitive)
				{
					Debug.Trace.LowLevel.TraceDataSansId(System.Diagnostics.TraceEventType.Critical,
						nameof(MemoryCopier<TDst, TSrc>) + ": Destination type is not a primitive type",
						dstType.FullName);
				}

				if (!srcType.IsPrimitive)
				{
					Debug.Trace.LowLevel.TraceDataSansId(System.Diagnostics.TraceEventType.Critical,
						nameof(MemoryCopier<TDst, TSrc>) + ": Source type is not a primitive type",
						srcType.FullName);
				}
			}

			readonly int mDstTypeSize_;
			readonly int mSrcTypeSize_;

			public int DestinationTypeSize { get { return this.mDstTypeSize_; } }
			public int SourceTypeSize { get { return this.mSrcTypeSize_; } }

			public MemoryCopier(
				[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
				bool dummy)
			{
				this.mDstTypeSize_ = LowLevel.Util.Unmanaged.SizeOf<TDst>();
				this.mSrcTypeSize_ = LowLevel.Util.Unmanaged.SizeOf<TSrc>();
			}

			internal void CopyInternal(TDst[] dst, int dstOffset,
				TSrc[] src, int srcOffset,
				int srcCopyCount)
			{
				Contract.Assert(this.DestinationTypeSize != 0 && this.SourceTypeSize != 0,
					"somebody used MemoryCopier's default constructor!");

				if (srcCopyCount == 0)
					return;

				// Get the available size of the buffers
				int dstBufferLocalSizeInBytes = (dst.Length - dstOffset) * this.mDstTypeSize_;
				int srcBufferLocalSizeInBytes = (src.Length - srcOffset) * this.mSrcTypeSize_;

				// Size, in bytes, of the src elements to copy. Could be smaller than src_buffer_size
				int srcCopyCountInBytes = this.mSrcTypeSize_ * srcCopyCount;

				if (srcCopyCountInBytes > dstBufferLocalSizeInBytes)
					throw new ArgumentOutOfRangeException(nameof(srcCopyCount), srcCopyCount,
						"total source memory to copy exceeds the memory available in destination");

				Buffer.BlockCopy(src, srcOffset,
					dst, dstOffset,
					srcCopyCountInBytes);
			}

			public void Copy(TDst[] dst, int dstOffset,
				TSrc[] src, int srcOffset,
				int srcCopyCount)
			{
				Contract.Requires<ArgumentNullException>(dst != null);
				Contract.Requires<ArgumentNullException>(src != null);
				Contract.Requires<ArgumentOutOfRangeException>(srcCopyCount >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(
					dstOffset >= 0 && dstOffset < dst.Length);
				Contract.Requires<ArgumentOutOfRangeException>(
					srcOffset >= 0 && srcOffset < src.Length);

				Contract.Requires<ArgumentOutOfRangeException>(
					(srcOffset+srcCopyCount) <= src.Length);

				this.CopyInternal(dst, dstOffset, src, srcOffset, srcCopyCount);
			}

			public void Copy(TDst[] dst, TSrc[] src,
				int srcCopyCount)
			{
				Contract.Requires<ArgumentNullException>(dst != null);
				Contract.Requires<ArgumentNullException>(src != null);
				Contract.Requires<ArgumentOutOfRangeException>(srcCopyCount >= 0);

				this.CopyInternal(dst, 0, src, 0, srcCopyCount);
			}
		};

		/// <remarks>Declared as public as it's used in code contracts. Caller responsible for null and index-positive checks</remarks>
		[Contracts.Pure]
		public static bool ArrayCopyFromBytesBoundsValidate(byte[] src, int srcOffset, Array dst, int dstOffset, int count, int elementSize)
		{
			if (count < 0)
				return false;

			int srcIndexEnd = srcOffset + count;
			int dstIndexEnd = dstOffset + (count / elementSize);
			//int copy_leftovers = count % elementSize;

			if (srcIndexEnd > src.Length ||
				dstIndexEnd > dst.Length)
				return false;

			//if (copy_leftovers != 0)
			//	return false;

			return true;
		}
		/// <remarks>Declared as public as it's used in code contracts. Caller responsible for null and index-positive checks</remarks>
		[Contracts.Pure]
		public static bool ArrayCopyToBytesBoundsValidate(Array src, int srcOffset, byte[] dst, int dstOffset, int count, int elementSize)
		{
			if (count < 0)
				return false;

			int srcIndexEnd = srcOffset + count;
			int dstIndexEnd = dstOffset + (count * elementSize);

			if (srcIndexEnd > src.Length ||
				dstIndexEnd > dst.Length)
				return false;

			return true;
		}
#if false // #TODO
		/// <summary>
		/// Copies a range of elements from a source array into the element memory of a destination array
		/// </summary>
		/// <typeparam name="TSrc">Source element type. Must be a primitive type</typeparam>
		/// <typeparam name="TDst">Destination element type. Must be a primitive type</typeparam>
		/// <param name="sourceArray">Memory to copy from</param>
		/// <param name="sourceIndex">Element index to start the copy from</param>
		/// <param name="length">Number of source elements to copy</param>
		/// <param name="destinationArray">Memory to copy to</param>
		/// <param name="destinationIndex">Element index to start the copy at</param>
		/// <returns>True if the memcpy operation was successful</returns>
		/// <remarks>Unlike <see cref="System.Buffer.BlockCopy"/> (which is more like memmove), this doesn't guard against overlap</remarks>
		public static bool MemoryCopy<TSrc, TDst>(TSrc[] sourceArray, int sourceIndex, int length, TDst[] destinationArray, int destinationIndex)
			where TSrc : struct
			where TDst : struct
		{
			const string k_type_not_primitive_msg_postfix = " must be a primitive type";

			Contract.Requires<ArgumentNullException>(sourceArray != null);
			Contract.Requires<ArgumentOutOfRangeException>(sourceIndex >= 0);
			Contract.Requires<ArgumentException>(typeof(TSrc).IsPrimitive, "TSrc" + k_type_not_primitive_msg_postfix);
			Contract.Requires<ArgumentNullException>(destinationArray != null);
			Contract.Requires<ArgumentOutOfRangeException>(destinationIndex >= 0);
			Contract.Requires<ArgumentException>(typeof(TDst).IsPrimitive, "TDst" + k_type_not_primitive_msg_postfix);

			// LowLevel's Memcpy takes destinationArray first, then sourceArray, like C's memcpy
			return LowLevel.Util.ValueTypeBitConverter.Memcpy(	destinationArray, destinationIndex,
																sourceArray, sourceIndex, length,
																true); // check that array types are primitives
		}
#endif
		#endregion

		#region Get high/low bits
		/// <summary>Convenience function for getting the high order bits (LSB) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Signed representation of the high-bits in <paramref name="value"/></returns>
		[Contracts.Pure]
		public static int GetHighBitsSigned(uint value)	{ return (int)((value >> 16) & 0xFFFFFFFF); }
		/// <summary>Convenience function for getting the low order bits (MSB) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Signed representation of the low-bits in <paramref name="value"/></returns>
		[Contracts.Pure]
		public static int GetLowBitsSigned(uint value)	{ return (int)(value & 0xFFFFFFFF); }

		/// <summary>Convenience function for getting the high order bits (LSB) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Unsigned representation of the high-bits in <paramref name="value"/></returns>
		[Contracts.Pure]
		public static uint GetHighBits(ulong value)	{ return (uint)((value >> 32) & 0xFFFFFFFF); }
		/// <summary>Convenience function for getting the low order bits (MSB) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Unsigned representation of the low-bits in <paramref name="value"/></returns>
		[Contracts.Pure]
		public static uint GetLowBits(ulong value)	{ return (uint)(value & 0xFFFFFFFF); }

		/// <summary>Convenience function for getting the high order bits (LSB) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Unsigned representation of the high-bits in <paramref name="value"/></returns>
		[Contracts.Pure]
		public static int GetHighBitsSigned(ulong value)	{ return (int)((value >> 32) & 0xFFFFFFFF); }
		/// <summary>Convenience function for getting the low order bits (MSB) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Unsigned representation of the low-bits in <paramref name="value"/></returns>
		[Contracts.Pure]
		public static int GetLowBitsSigned(ulong value)	{ return (int)(value & 0xFFFFFFFF); }
		#endregion

		#region HighestBitSetIndex
		[Contracts.Pure]
		public static byte IndexOfHighestBitSet(uint value)
		{
			Contract.Ensures(Contract.Result<byte>() < K_INT32_BIT_COUNT);

			value |= value >> 1; // first round down to one less than a power of 2
			value |= value >> 2;
			value |= value >> 4;
			value |= value >> 8;
			value |= value >> 16;

			uint index = (value * 0x07C4ACDDU) >> 27;
			return KMultiplyDeBruijnBitPositionHighestBitSet32[index];
		}
		[Contracts.Pure]
		public static byte IndexOfHighestBitSet(ulong value)
		{
			Contract.Ensures(Contract.Result<byte>() < K_INT64_BIT_COUNT);

			int index = 0;
			uint high = GetHighBits(value);
			if(high != 0)
				index = IndexOfHighestBitSet(high) + K_INT32_BIT_COUNT;
			else
				index = IndexOfHighestBitSet(GetLowBits(value));

			Contract.Assume(index >= 0);
			return (byte)index;
		}
		#endregion

		#region LeadingZerosCount
		/// <summary>Count the "leftmost" consecutive zero bits (leading) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static byte LeadingZerosCount(byte value)
		{
			Contract.Ensures(Contract.Result<byte>() <= K_BYTE_BIT_COUNT);
			return (byte)( LeadingZerosCount((uint)value) - (K_BYTE_BIT_COUNT * 3) );
		}
		/// <summary>Count the "leftmost" consecutive zero bits (leading) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static byte LeadingZerosCount(ushort value)
		{
			Contract.Ensures(Contract.Result<byte>() <= K_INT16_BIT_COUNT);
			return (byte)( LeadingZerosCount((uint)value) - (K_BYTE_BIT_COUNT * 2) );
		}
		/// <summary>Count the "leftmost" consecutive zero bits (leading) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static byte LeadingZerosCount(uint value)
		{
			Contract.Ensures(Contract.Result<byte>() <= K_INT32_BIT_COUNT);
			if (value == 0)
				return K_INT32_BIT_COUNT;

			value |= value >> 1; // first round down to one less than a power of 2
			value |= value >> 2;
			value |= value >> 4;
			value |= value >> 8;
			value |= value >> 16;

			// subtract the log base 2 from the number of bits in the integer
			uint index = (value * 0x07C4ACDDU) >> 27;
			return (byte)(K_INT32_BIT_COUNT - KMultiplyDeBruijnBitPositionLeadingZeros32[index]);
		}
		/// <summary>Count the "leftmost" consecutive zero bits (leading) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static byte LeadingZerosCount(ulong value)
		{
			Contract.Ensures(Contract.Result<byte>() <= K_INT64_BIT_COUNT);

			byte count = LeadingZerosCount(GetHighBits(value));
			// The high bits were all zero, continue checking low bits
			if (count == K_INT32_BIT_COUNT)
				count += LeadingZerosCount(GetLowBits(value));

			return count;
		}
		#endregion

		#region TrailingZerosCount
		/// <summary>Count the "rightmost" consecutive zero bits (trailing) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static byte TrailingZerosCount(uint value)
		{
			Contract.Ensures(Contract.Result<byte>() <= K_INT32_BIT_COUNT);
			if (value == 0)
				return K_INT32_BIT_COUNT;

			// instead of (value & -value), where the op result is a long, we do this to keep it all 32-bit
			uint ls1B = (~value) + 1; // two's complement
			ls1B = value & ls1B; // least significant 1 bit
			uint index = (ls1B * 0x077CB531U) >> 27;
			return KMultiplyDeBruijnBitPositionTrailingZeros32[index];
		}
		/// <summary>Count the "rightmost" consecutive zero bits (trailing) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static byte TrailingZerosCount(ulong value)
		{
			Contract.Ensures(Contract.Result<byte>() <= K_INT64_BIT_COUNT);

			byte count = TrailingZerosCount(GetLowBits(value));
			// The low bits were all zero, continue checking high bits
			if (count == K_INT32_BIT_COUNT)
				count += TrailingZerosCount(GetHighBits(value));

			return count;
		}
		#endregion

		#region BitDecode 16
		/// <summary>Bit decode an enumeration or flags from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from</param>
		/// <param name="traits"></param>
		/// <returns>The enumeration\flags value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		[Contracts.Pure]
		public static ushort BitDecode(ushort bits, Bitwise.BitFieldTraits traits)
		{
			Contract.Requires/*<ArgumentException>*/(!traits.IsEmpty);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(traits.BitIndex < K_INT16_BIT_COUNT);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(traits.BitIndex+traits.BitCount <= K_INT16_BIT_COUNT);

			return (ushort)((bits >> traits.BitIndex) & traits.Bitmask16);
		}
		#endregion
		#region BitEncode 16
		/// <summary>Bit encode a value into an unsigned integer, removing the original data in the value's range</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="traits"></param>
		/// <returns><paramref name="bits"/> with <paramref name="value"/> encoded into it</returns>
		/// <remarks>
		/// Clears the bit-space between <paramref name="bitIndex"/> + <paramref name="bitMask"/>
		/// so any existing values will be lost after <paramref name="value"/> is added
		/// </remarks>
		[Contracts.Pure]
		public static ushort BitEncode(ushort value, ushort bits, Bitwise.BitFieldTraits traits)
		{
			Contract.Requires/*<ArgumentException>*/(!traits.IsEmpty);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(traits.BitIndex < K_INT16_BIT_COUNT);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(traits.BitIndex+traits.BitCount <= K_INT16_BIT_COUNT);

			var bitmask = (uint)traits.Bitmask16;
			// Use the bit mask's invert so we can get all of the non-value bits
			return (ushort)BitEncodeFlags(value, bits & (~bitmask), traits.BitIndex, bitmask);
		}
		#endregion
	};
}
