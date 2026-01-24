using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using HashAlgorithm = System.Security.Cryptography.HashAlgorithm;

namespace KSoft.Security.Cryptography
{
	// See also Karl Malbrain's compact CRC-32, with pre and post conditioning.
	// See "A compact CCITT crc16 and crc32 C implementation that balances processor cache usage against speed":
	// http://www.geocities.ws/malbrain/crc_c.html
	// or https://github.com/richgel999/lzham_codec_devel/blob/master/lzhamdecomp/lzham_checksum.cpp#L47

	public static partial class Crc32
	{
		public const int K_CRC_TABLE_SIZE = 256;
		public const uint K_DEFAULT_POLYNOMIAL = 0xEDB88320; // 0x04C11DB7 nets the same results
		internal static readonly uint[] KDefaultTable = new Definition().CrcTable;
	};

	public sealed class CrcHash32
		: HashAlgorithm
	{
		#region Registeration
		public const string K_ALGORITHM_NAME = "KSoft.Security.Cryptography.CrcHash32";

		public new static CrcHash32 Create(string algName)
		{
			return (CrcHash32)System.Security.Cryptography.CryptoConfig.CreateFromName(algName);
		}
		public new static CrcHash32 Create()
		{
			return Create(K_ALGORITHM_NAME);
		}

		static CrcHash32()
		{
			System.Security.Cryptography.CryptoConfig.AddAlgorithm(typeof(CrcHash32), K_ALGORITHM_NAME);
		}
		#endregion

		readonly Crc32.Definition mDefinition_;
		byte[] mHashBytes_;
		public uint Hash32 { get; private set; }

		public CrcHash32()
			: this(new Crc32.Definition(crcTable: Crc32.KDefaultTable))
		{
		}

		public CrcHash32(Crc32.Definition definition)
		{
			Contract.Requires(definition != null);

			this.HashSizeValue = Bits.K_INT32_BIT_COUNT;

			this.mDefinition_ = definition;
			this.mHashBytes_ = new byte[sizeof(uint)];
		}

		public override void Initialize()
		{
			Array.Clear(this.mHashBytes_, 0, this.mHashBytes_.Length);
			this.Hash32 = this.mDefinition_.InitialValue;

			this.Hash32 ^= this.mDefinition_.XorIn;
		}

		/// <summary>Performs the hash algorithm on the data provided.</summary>
		/// <param name="array">The array containing the data.</param>
		/// <param name="startIndex">The position in the array to begin reading from.</param>
		/// <param name="count">How many bytes in the array to read.</param>
		protected override void HashCore(byte[] array, int startIndex, int count)
		{
			this.Hash32 = this.mDefinition_.HashCore(this.Hash32, array, startIndex, count);
		}

		/// <summary>Performs any final activities required by the hash algorithm.</summary>
		/// <returns>The final hash value.</returns>
		protected override byte[] HashFinal()
		{
			this.Hash32 ^= this.mDefinition_.XorOut;
			Bitwise.ByteSwap.ReplaceBytes(this.mHashBytes_, 0, this.Hash32);
			return this.mHashBytes_;
		}
	};
}
