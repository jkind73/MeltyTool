using System;
using System.Diagnostics.CodeAnalysis;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Bitwise
{
	partial class ByteSwap
	{
		[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
		public struct Swapper
		{
			readonly short[] kCodes_;

			public Swapper(int sizeOf, params short[] codes)
			{
				Contract.Requires<ArgumentOutOfRangeException>(sizeOf > 0);
				Contract.Requires<ArgumentNullException>(codes != null);

				this.kCodes_ = codes;
			}
			public Swapper(IByteSwappable definition)
			{
				Contract.Requires<ArgumentNullException>(definition != null);

				this.kCodes_ = definition.ByteSwapCodes;
			}

			public int SwapData(byte[] buffer, int startIndex = 0)
			{
				Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(startIndex <= buffer.Length);

				return this.SwapData(buffer, startIndex, out int sizeInBytes, out int sizeInCodes);
			}

			public int SwapData(byte[] buffer, int startIndex,
				out int sizeInBytes, out int sizeInCodes)
			{
				Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(buffer == null || startIndex <= buffer.Length);

				return this.SwapDataImpl(buffer, startIndex, out sizeInBytes, out sizeInCodes, 0);
			}

			private int SwapDataImpl(byte[] buffer, int startIndex
				, out int outSizeInBytes, out int outSizeInCodes
				, int codesStartIndex)
			{
				outSizeInBytes = outSizeInCodes = 0;
				int sizeInBytes = 0;
				int sizeInCodes = 0;

				int codesIndex = codesStartIndex;
				Contract.Assert(this.kCodes_[codesIndex] == (int)BsCode.ARRAY_START);

				int arrayCount = this.kCodes_[codesIndex + 1]; // array count comes after ArrayStart

				bool bufferIsValid = buffer != null;
				int bufferIndex = startIndex;
				for (int elementsRemaining = arrayCount; elementsRemaining > 0; )
				{
					sizeInCodes = 2;
					bool foundArrayEnd = false;
					for (codesIndex = codesStartIndex + sizeInCodes; !foundArrayEnd; )
					{
						var currentCode = this.kCodes_[codesIndex];
						switch (currentCode)
						{
							#region Word
							case (int)BsCode.INT16:
								if (bufferIsValid)
								{
									SwapInt16(buffer, bufferIndex);
									bufferIndex += sizeof(short);
								}

								sizeInBytes += sizeof(short);
								sizeInCodes++;
								codesIndex++;
								break;
							#endregion

							#region DWord
							case (int)BsCode.INT32:
								if (bufferIsValid)
								{
									SwapInt32(buffer, bufferIndex);
									bufferIndex += sizeof(int);
								}

								sizeInBytes += sizeof(int);
								sizeInCodes++;
								codesIndex++;
								break;
							#endregion

							#region QWord
							case (int)BsCode.INT64:
								if (bufferIsValid)
								{
									SwapInt64(buffer, bufferIndex);
									bufferIndex += sizeof(long);
								}

								sizeInBytes += sizeof(long);
								sizeInCodes++;
								codesIndex++;
								break;
							#endregion

							case (int)BsCode.ARRAY_START:
								int recursiveSizeInBytes, recursiveSizeInCodes;

								this.SwapDataImpl(buffer, bufferIndex,
								                  out recursiveSizeInBytes, out recursiveSizeInCodes,
								                  codesIndex);

								if (bufferIsValid)
									bufferIndex += recursiveSizeInBytes;

								codesIndex += recursiveSizeInCodes;
								sizeInCodes += recursiveSizeInCodes;
								sizeInBytes += recursiveSizeInBytes;
								break;

							case (int)BsCode.ARRAY_END:
								codesIndex++;
								sizeInCodes++;
								elementsRemaining--;
								foundArrayEnd = true;
								break;

							#region Skip (default)
							default:
								if (currentCode < 0)
									throw new Debug.UnreachableException();

								if (bufferIsValid)
									bufferIndex += currentCode;

								codesIndex++;
								sizeInCodes++;
								sizeInBytes += currentCode;
								break;
							#endregion
						}
					}
				}

				outSizeInBytes = sizeInBytes;
				outSizeInCodes = sizeInCodes;
				return bufferIsValid
					? bufferIndex
					: TypeExtensions.K_NONE;
			}
		};
	};
}
