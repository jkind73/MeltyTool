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

	public static partial class Crc16
	{
		public const int K_CRC_TABLE_SIZE = 256;
		public const ushort K_DEFAULT_POLYNOMIAL = 0x1021;
		internal static readonly ushort[] KDefaultTable = new Definition().CrcTable;
	};

	public sealed class CrcHash16
		: HashAlgorithm
	{
		#region Registeration
		public const string K_ALGORITHM_NAME = "KSoft.Security.Cryptography.CrcHash16";

		public new static CrcHash16 Create(string algName)
		{
			return (CrcHash16)System.Security.Cryptography.CryptoConfig.CreateFromName(algName);
		}
		public new static CrcHash16 Create()
		{
			return Create(K_ALGORITHM_NAME);
		}

		static CrcHash16()
		{
			System.Security.Cryptography.CryptoConfig.AddAlgorithm(typeof(CrcHash16), K_ALGORITHM_NAME);
		}
		#endregion

		readonly Crc16.Definition mDefinition_;
		byte[] mHashBytes_;
		public ushort Hash16 { get; private set; }

		public CrcHash16()
			: this(new Crc16.Definition(crcTable: Crc16.KDefaultTable))
		{
		}

		public CrcHash16(Crc16.Definition definition)
		{
			Contract.Requires(definition != null);

			this.HashSizeValue = Bits.K_INT16_BIT_COUNT;

			this.mDefinition_ = definition;
			this.mHashBytes_ = new byte[sizeof(ushort)];
		}

		public override void Initialize()
		{
			Array.Clear(this.mHashBytes_, 0, this.mHashBytes_.Length);
			this.Hash16 = this.mDefinition_.InitialValue;

			this.Hash16 ^= this.mDefinition_.XorIn;
		}

		/// <summary>Performs the hash algorithm on the data provided.</summary>
		/// <param name="array">The array containing the data.</param>
		/// <param name="startIndex">The position in the array to begin reading from.</param>
		/// <param name="count">How many bytes in the array to read.</param>
		protected override void HashCore(byte[] array, int startIndex, int count)
		{
			this.Hash16 = this.mDefinition_.HashCore(this.Hash16, array, startIndex, count);
		}

		/// <summary>Performs any final activities required by the hash algorithm.</summary>
		/// <returns>The final hash value.</returns>
		protected override byte[] HashFinal()
		{
			this.Hash16 ^= this.mDefinition_.XorOut;
			Bitwise.ByteSwap.ReplaceBytes(this.mHashBytes_, 0, this.Hash16);
			return this.mHashBytes_;
		}
	};
}
