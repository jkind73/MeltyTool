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
	[Contracts.ContractClass(typeof(ByteSwappableContract))]
	public interface IByteSwappable
	{
		int SizeOf { get; }

		[SuppressMessage("Microsoft.Design", "CA1819:PropertiesShouldNotReturnArrays")]
		short[] ByteSwapCodes { get; }
	};
	[Contracts.ContractClassFor(typeof(IByteSwappable))]
	abstract class ByteSwappableContract : IByteSwappable
	{
		#region IByteSwappable Members
		public int SizeOf { get {
			Contract.Ensures(Contract.Result<int>() > 0,
				"You can't define a zero-byte struct, so what's the point of this definition?");

			throw new NotImplementedException();
		} }

		public short[] ByteSwapCodes { get {
			Contract.Ensures(Contract.Result<short[]>() != null);
			Contract.Ensures(Contract.Result<short[]>().Length >= ByteSwap.K_MINUMUM_NUMBER_OF_DEFINITION_BS_CODES,
				"Codes should include: ArrayStart, {Count}, {Elements}, and ArrayEnd");

			throw new NotImplementedException();
		} }

		#endregion
	};
}
