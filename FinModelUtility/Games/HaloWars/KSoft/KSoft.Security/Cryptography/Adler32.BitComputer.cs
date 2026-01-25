using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Security.Cryptography
{
	partial class Adler32
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
		public struct BitComputer
		{
			uint s1_, s2_;

			public BitComputer(uint adler32)
			{
				this.s1_ = adler32 & 0xFFFF;
				this.s2_ = adler32 >> 16;
			}

			public static BitComputer New { get { return new BitComputer(1); } }

			public uint ComputeFinish()
			{
				return Adler32.ComputeFinish(this.s1_, this.s2_);
			}

			public void Compute(byte[] buffer, int offset, int length)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0 && length >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(offset + length <= buffer.Length);

				int buflen = length;
				for (int blocklen; buflen > 0; buflen -= blocklen)
				{
					blocklen = buflen < K_BLOCK_MAX_
						? buflen
						: K_BLOCK_MAX_;

					int x;
					for (x = 0; x + 7 < blocklen; x += 8, offset += 8)
					{
						this.s1_ += buffer[offset + 0];
						this.s2_ += this.s1_;
						this.s1_ += buffer[offset + 1];
						this.s2_ += this.s1_;
						this.s1_ += buffer[offset + 2];
						this.s2_ += this.s1_;
						this.s1_ += buffer[offset + 3];
						this.s2_ += this.s1_;
						this.s1_ += buffer[offset + 4];
						this.s2_ += this.s1_;
						this.s1_ += buffer[offset + 5];
						this.s2_ += this.s1_;
						this.s1_ += buffer[offset + 6];
						this.s2_ += this.s1_;
						this.s1_ += buffer[offset + 7];
						this.s2_ += this.s1_;
					}

					for (; x < blocklen; x++, offset++)
					{
						this.s1_ += buffer[offset];
						this.s2_ += this.s1_;
					}

					this.s1_ %= K_ADLER_MOD_;
					this.s2_ %= K_ADLER_MOD_;
				}
			}

			#region Compute 16-bits
			public void ComputeLe(ushort value)
			{
				ComputeUpdate((value & 0x00FFU) >> 0, ref this.s1_, ref this.s2_);
				ComputeUpdate((value & 0xFF00U) >> 8, ref this.s1_, ref this.s2_);
			}
			public void ComputeBe(ushort value)
			{
				ComputeUpdate((value & 0xFF00U) >> 8, ref this.s1_, ref this.s2_);
				ComputeUpdate((value & 0x00FFU) >> 0, ref this.s1_, ref this.s2_);
			}
			#endregion

			#region Compute 32-bits
			public void ComputeLe(uint value)
			{
				ComputeUpdate((value & 0x000000FFU) >> 0, ref this.s1_, ref this.s2_);
				ComputeUpdate((value & 0x0000FF00U) >> 8, ref this.s1_, ref this.s2_);
				ComputeUpdate((value & 0x00FF0000U) >> 16, ref this.s1_, ref this.s2_);
				ComputeUpdate((value & 0xFF000000U) >> 24, ref this.s1_, ref this.s2_);
			}
			public void ComputeBe(uint value)
			{
				ComputeUpdate((value & 0xFF000000U) >> 24, ref this.s1_, ref this.s2_);
				ComputeUpdate((value & 0x00FF0000U) >> 16, ref this.s1_, ref this.s2_);
				ComputeUpdate((value & 0x0000FF00U) >> 8, ref this.s1_, ref this.s2_);
				ComputeUpdate((value & 0x000000FFU) >> 0, ref this.s1_, ref this.s2_);
			}
			#endregion

			#region Compute 64-bits
			public void ComputeLe(ulong value)
			{
				uint lo = Bits.GetLowBits(value);
				uint hi = Bits.GetHighBits(value);
				uint value;

				value = lo;
				this.ComputeLe(value);

				value = hi;
				this.ComputeLe(value);
			}
			public void ComputeBe(ulong value)
			{
				uint lo = Bits.GetLowBits(value);
				uint hi = Bits.GetHighBits(value);
				uint value;

				value = hi;
				this.ComputeBe(value);

				value = lo;
				this.ComputeBe(value);
			}
			#endregion
		};
	};
}
