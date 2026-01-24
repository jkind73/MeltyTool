using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	[Contracts.ContractClass(typeof(EndianStreamSerializableContract))]
	public interface IEndianStreamSerializable
	{
		void Serialize(EndianStream s);
	};

	[Contracts.ContractClassFor(typeof(IEndianStreamSerializable))]
	abstract class EndianStreamSerializableContract : IEndianStreamSerializable
	{
		public void Serialize(EndianStream s)	{ Contract.Requires(s != null); }
	};
}