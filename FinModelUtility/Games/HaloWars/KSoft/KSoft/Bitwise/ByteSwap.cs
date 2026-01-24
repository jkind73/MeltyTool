using System;
using System.Diagnostics.CodeAnalysis;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Bitwise
{
	/// <summary>Pre-defined byte swapping codes</summary>
	[EnumBitEncoderDisable]
	[SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
	public enum BsCode : short
	{
		/// <summary>Byte-swap code for 8 bits of data</summary>
		BYTE = 1,
		/// <summary>Byte-swap code for 16 bits of data</summary>
		[SuppressMessage("Microsoft.Design", "CA1720:IdentifiersShouldNotContainTypeNames")]
		INT16 = -2,
		/// <summary>Byte-swap code for 32 bits of data</summary>
		[SuppressMessage("Microsoft.Design", "CA1720:IdentifiersShouldNotContainTypeNames")]
		INT32 = -4,
		/// <summary>Byte-swap code for 64 bits of data</summary>
		[SuppressMessage("Microsoft.Design", "CA1720:IdentifiersShouldNotContainTypeNames")]
		INT64 = -8,

		/// <summary>
		/// Byte-swap code for the start of a repeated table of byte swap codes.
		/// Next int in the byte swap code list is the amount of times to repeat the codes
		/// </summary>
		ARRAY_START = -100,
		/// <summary>Byte-swap code for the end of a repeated table of byte swap codes</summary>
		ARRAY_END = -101,
	};

	public static partial class ByteSwap
	{
		// ArrayStart, {Count}, {Elements}, ArrayEnd
		internal const int K_MINUMUM_NUMBER_OF_DEFINITION_BS_CODES = 4;

		[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
		public struct BsDefinition
			: IByteSwappable
		{
			readonly string kName_;
			public override string ToString()	{ return this.kName_; }
			readonly short[] kBsCodes_;
			[SuppressMessage("Microsoft.Design", "CA1819:PropertiesShouldNotReturnArrays")]
			public short[] ByteSwapCodes		{ get { return this.kBsCodes_; } }
			readonly int kSizeOf_;
			public int SizeOf					{ get { return this.kSizeOf_; } }

			public BsDefinition(string name, int sizeOf, params short[] bsCodes)
			{
				Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name));
				Contract.Requires<ArgumentOutOfRangeException>(sizeOf > 0);
				Contract.Requires<ArgumentNullException>(bsCodes != null);
				Contract.Requires<ArgumentException>(bsCodes.Length >= K_MINUMUM_NUMBER_OF_DEFINITION_BS_CODES);

				this.kName_ = name;
				this.kSizeOf_ = sizeOf;
				this.kBsCodes_ = bsCodes;
			}
		};

		/// <summary>Byte swap a given structure a number of times over a range of bytes</summary>
		/// <param name="definition">Structure definition in terms of byte swap codes</param>
		/// <param name="buffer">Buffer containing the bytes of an instance of the definition</param>
		/// <param name="startIndex">Where to start processing the definition in the buffer</param>
		/// <param name="count">Number of times to process the definition on the buffer</param>
		/// <returns>Offset in <paramref name="buffer"/> where processing ended</returns>
		public static int SwapData(IByteSwappable definition, byte[] buffer,
			int startIndex = 0, int count = 1)
		{
			Contract.Requires<ArgumentNullException>(definition != null);
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(startIndex <= buffer.Length);
			Contract.Requires<ArgumentOutOfRangeException>(count > 0);
			Contract.Requires<ArgumentOutOfRangeException>((count*definition.SizeOf) <= (buffer.Length-startIndex),
				"buffer doesn't have enough data for the given byte swap parameters");
			Contract.Ensures(Contract.Result<int>() >= 0);

			if (count == 0)
				return startIndex;

			var swap = new Swapper(definition);
			int bufferIndex = startIndex;
			for (int elementsRemaining = count; elementsRemaining > 0; elementsRemaining--)
				bufferIndex = swap.SwapData(buffer, bufferIndex);

			Contract.Assert(bufferIndex == (startIndex + (definition.SizeOf * count)));
			return bufferIndex;
		}

		#region Int byte swap definitions
		/// <summary>Byte-swap definition for a 16-bit integer</summary>
		public static readonly IByteSwappable KInt16Definition = new BsDefinition("Int16", sizeof(short),
			(short)BsCode.ARRAY_START, 1,
			(short)BsCode.INT16,
			(short)BsCode.ARRAY_END);
		/// <summary>Byte-swap definition for a 32-bit integer</summary>
		public static readonly IByteSwappable KInt32Definition = new BsDefinition("Int32", sizeof(int),
			(short)BsCode.ARRAY_START, 1,
			(short)BsCode.INT32,
			(short)BsCode.ARRAY_END);
		/// <summary>Byte-swap definition for a 64-bit integer</summary>
		public static readonly IByteSwappable KInt64Definition = new BsDefinition("Int64", sizeof(long),
			(short)BsCode.ARRAY_START, 1,
			(short)BsCode.INT64,
			(short)BsCode.ARRAY_END);
		#endregion

		#region Single
		/// <summary>Swaps a <see cref="Single" /> and returns the result</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static float SwapSingle(
			float value)
		{
			return new SingleUnion(
				SwapUInt32(new SingleUnion(value).Integer)
				).Real;
		}
		/// <summary>Swaps a <see cref="Single" /> by reference</summary>
		/// <param name="value"></param>
		[Contracts.Pure]
		public static void SwapSingle(
			ref float value)
		{
			var union = new SingleUnion(value);
			Swap(ref union.Integer);
			value = union.Real;
		}
		/// <summary>Replaces 4 bytes in an array with a floating-point value</summary>
		/// <param name="buffer">byte buffer</param>
		/// <param name="offset">offset in <paramref name="buffer"/> to put the new value</param>
		/// <param name="value">value to replace the buffer's current bytes with</param>
		/// <remarks><paramref name="buffer"/>'s endian order is assumed to be the same as the current operating environment</remarks>
		[Contracts.Pure]
		public static void ReplaceBytes(byte[] buffer, int offset,
			float value)
		{
			ReplaceBytes(buffer, offset, new SingleUnion(value).Integer);
		}

		public static float SingleFromUInt32(uint bits)
		{
			var union = new SingleUnion(bits);

			return union.Real;
		}
		public static uint SingleToUInt32(float value)
		{
			var union = new SingleUnion(value);

			return union.Integer;
		}
		#endregion
		#region Double
		/// <summary>Swaps a <see cref="Double" /> and returns the result</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static double SwapDouble(
			double value)
		{
			return new DoubleUnion(
				SwapUInt64(new DoubleUnion(value).Integer)
				).Real;
		}
		/// <summary>Swaps a <see cref="Double" /> by reference</summary>
		/// <param name="value"></param>
		[Contracts.Pure]
		public static void SwapDouble(
			ref double value)
		{
			var union = new DoubleUnion(value);
			Swap(ref union.Integer);
			value = union.Real;
		}
		/// <summary>Replaces 8 bytes in an array with a floating-point value</summary>
		/// <param name="buffer">byte buffer</param>
		/// <param name="offset">offset in <paramref name="buffer"/> to put the new value</param>
		/// <param name="value">value to replace the buffer's current bytes with</param>
		/// <remarks><paramref name="buffer"/>'s endian order is assumed to be the same as the current operating environment</remarks>
		[Contracts.Pure]
		public static void ReplaceBytes(byte[] buffer, int offset,
			double value)
		{
			ReplaceBytes(buffer, offset, new DoubleUnion(value).Integer);
		}

		public static double DoubleFromUInt64(ulong bits)
		{
			var union = new DoubleUnion(bits);

			return union.Real;
		}
		public static ulong DoubleToUInt64(double value)
		{
			var union = new DoubleUnion(value);

			return union.Integer;
		}
		#endregion
	};
}
