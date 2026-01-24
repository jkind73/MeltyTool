#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Security.Cryptography
{
	public static partial class Crc32
	{
		public sealed class Definition
		{
			readonly uint mPolynomial_;
			readonly uint[] mCrcTable_;
			readonly uint mInitialValue_;
			readonly uint mXorIn_;
			readonly uint mXorOut_;

			public uint Polynomial { get { return this.mPolynomial_; } }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1819:PropertiesShouldNotReturnArrays")]
			public uint[] CrcTable { get { return this.mCrcTable_; } }
			public uint InitialValue { get { return this.mInitialValue_; } }
			public uint XorIn { get { return this.mXorIn_; } }
			public uint XorOut { get { return this.mXorOut_; } }

			static uint[] BuildCrcTable(uint polynomial)
			{
				var crcTable = new uint[K_CRC_TABLE_SIZE];

				for (uint index = 0; index < crcTable.Length; index++)
				{
					uint crc = index;
					for (uint j = 0; j < 8; j++)
					{
						if ((crc & 1) == 1)
							crc = (crc >> 1) ^ polynomial;
						else
							crc >>= 1;
					}
					crcTable[index] = crc;
				}

				Contract.Assert(crcTable[1] != 0);
				Contract.Assert(crcTable[crcTable.Length-1] != 0);

				return crcTable;
			}

			public Definition(uint polynomial = K_DEFAULT_POLYNOMIAL, uint initialValue = uint.MaxValue, uint xorIn = 0, uint xorOut = 0, params uint[] crcTable)
			{
				Contract.Requires(crcTable.IsNullOrEmpty() || crcTable.Length == K_CRC_TABLE_SIZE);

				this.mPolynomial_ = polynomial;
				this.mInitialValue_ = initialValue;
				this.mXorIn_ = xorIn;
				this.mXorOut_ = xorOut;

				this.mCrcTable_ = crcTable.IsNullOrEmpty()
					? BuildCrcTable(this.Polynomial)
					: crcTable;
			}

			public uint ComputeUpdate(uint crc, uint value)
			{
				value &= 0xFF;
				uint a = (crc >> 8) & 0x00FFFFFF; // don't include the top most byte in case there was somehow any carry
				uint b = this.CrcTable[((int)crc ^ value) & 0xFF];
				return a ^ b;
			}

			public void ComputeUpdate(uint value, ref uint crc)
			{
				crc = this.ComputeUpdate(crc, value);
			}

			internal uint HashCore(uint crc, byte[] array, int startIndex, int count)
			{
				for (int index = startIndex; count != 0; --count, ++index)
				{
					crc = this.ComputeUpdate(crc, array[index]);
				}

				return crc;
			}
			public uint Crc(ref uint crc, byte[] buffer, int size)
			{
				if (crc == 0)
					crc = this.InitialValue;

				crc ^= this.XorIn;

				crc = this.HashCore(crc, buffer, 0, size);

				crc ^= this.XorOut;

				return crc;
			}
		};
	};
}
