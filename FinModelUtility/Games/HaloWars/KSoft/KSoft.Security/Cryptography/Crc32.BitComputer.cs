using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Security.Cryptography
{
	partial class Crc32
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
		public struct BitComputer
		{
			Definition mDefinition_;
			uint mCrc_;

			public BitComputer(Definition definition)
			{
				Contract.Requires(definition != null);

				this.mDefinition_ = definition;
				this.mCrc_ = this.mDefinition_.InitialValue;
			}
			public BitComputer(Definition definition, uint initialValue)
			{
				Contract.Requires(definition != null);

				this.mDefinition_ = definition;
				this.mCrc_ = initialValue;
			}

			public void ComputeBegin()
			{
				this.mCrc_ ^= this.mDefinition_.XorIn;
			}

			public uint ComputeFinish()
			{
				this.mCrc_ ^= this.mDefinition_.XorOut;
				return this.mCrc_;
			}

			public void Compute(byte[] buffer, int offset, int length)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0 && length >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(offset+length <= buffer.Length);

				for (int x = 0; x < length; x++)
					this.mDefinition_.ComputeUpdate(buffer[offset+x], ref this.mCrc_);
			}

			public void Compute(byte value)
			{
				this.mDefinition_.ComputeUpdate(value, ref this.mCrc_);
			}

			#region Compute 16-bits
			public void ComputeLe(ushort value)
			{
				this.mDefinition_.ComputeUpdate((value & 0x00FFU) >> 0, ref this.mCrc_);
				this.mDefinition_.ComputeUpdate((value & 0xFF00U) >> 8, ref this.mCrc_);
			}
			public void ComputeBe(ushort value)
			{
				this.mDefinition_.ComputeUpdate((value & 0xFF00U) >> 8, ref this.mCrc_);
				this.mDefinition_.ComputeUpdate((value & 0x00FFU) >> 0, ref this.mCrc_);
			}
			#endregion

			#region Compute 32-bits
			public void ComputeLe(uint value)
			{
				this.mDefinition_.ComputeUpdate((value & 0x000000FFU) >> 0, ref this.mCrc_);
				this.mDefinition_.ComputeUpdate((value & 0x0000FF00U) >> 8, ref this.mCrc_);
				this.mDefinition_.ComputeUpdate((value & 0x00FF0000U) >> 16, ref this.mCrc_);
				this.mDefinition_.ComputeUpdate((value & 0xFF000000U) >> 24, ref this.mCrc_);
			}
			public void ComputeBe(uint value)
			{
				this.mDefinition_.ComputeUpdate((value & 0xFF000000U) >> 24, ref this.mCrc_);
				this.mDefinition_.ComputeUpdate((value & 0x00FF0000U) >> 16, ref this.mCrc_);
				this.mDefinition_.ComputeUpdate((value & 0x0000FF00U) >> 8, ref this.mCrc_);
				this.mDefinition_.ComputeUpdate((value & 0x000000FFU) >> 0, ref this.mCrc_);
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
