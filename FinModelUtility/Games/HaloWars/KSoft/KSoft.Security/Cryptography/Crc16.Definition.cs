#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Security.Cryptography
{
	public static partial class Crc16
	{
		public sealed class Definition
		{
			readonly ushort mPolynomial_;
			readonly ushort[] mCrcTable_;
			readonly ushort mInitialValue_;
			readonly ushort mXorIn_;
			readonly ushort mXorOut_;

			public ushort Polynomial { get { return this.mPolynomial_; } }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1819:PropertiesShouldNotReturnArrays")]
			public ushort[] CrcTable { get { return this.mCrcTable_; } }
			public ushort InitialValue { get { return this.mInitialValue_; } }
			public ushort XorIn { get { return this.mXorIn_; } }
			public ushort XorOut { get { return this.mXorOut_; } }

			static ushort[] BuildCrcTable(uint polynomial)
			{
				var crcTable = new ushort[K_CRC_TABLE_SIZE];

				for (uint index = 0; index < crcTable.Length; index++)
				{
					uint crc = index << 8;
					for (uint j = 0; j < 8; j++)
					{
						if ((crc & 0x8000) != 0)
							crc = (crc << 1) ^ polynomial;
						else
							crc <<= 1;
					}
					crcTable[index] = (ushort)crc;
				}

				Contract.Assert(crcTable[1] != 0);
				Contract.Assert(crcTable[crcTable.Length-1] != 0);

				return crcTable;
			}

			public Definition(ushort polynomial = K_DEFAULT_POLYNOMIAL, ushort initialValue = ushort.MaxValue, ushort xorIn = 0, ushort xorOut = 0, params ushort[] crcTable)
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

			public ushort ComputeUpdate(ushort crc, uint value)
			{
				value &= 0xFF;
				ushort a = (ushort) (crc << 8);
				ushort b = (ushort)((crc >> 8) & 0x00FFFFFF); // don't include the top most byte in case there was somehow any carry
				ushort c = this.CrcTable[(b ^ value) & 0xFF];
				return (ushort)(a ^ c);
			}

			public void ComputeUpdate(uint value, ref ushort crc)
			{
				crc = this.ComputeUpdate(crc, value);
			}

			internal ushort HashCore(ushort crc, byte[] array, int startIndex, int count)
			{
				for (int index = startIndex; count != 0; --count, ++index)
				{
					crc = this.ComputeUpdate(crc, array[index]);
				}

				return crc;
			}
			public ushort Crc(ref ushort crc, byte[] buffer, int size)
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
