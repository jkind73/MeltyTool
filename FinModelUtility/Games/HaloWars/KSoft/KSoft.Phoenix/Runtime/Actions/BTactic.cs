#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Runtime
{
	partial class CSaveMarker
	{
		public const ushort
			TACTIC = 0x2710,
			PROTO_ACTION = 0x2711,
			WEAPON = 0x2712
			;
	};

	sealed class BTactic
		: IO.IEndianStreamSerializable
	{
		public BWeapon[] weapons;
		public BProtoAction[] protoActions;
		public bool animInfoLoaded;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray(s, ref this.weapons);
			Contract.Assert(this.weapons.Length <= BWeapon.K_MAX_COUNT);
			BSaveGame.StreamArray(s, ref this.protoActions);
			Contract.Assert(this.protoActions.Length <= BProtoAction.K_MAX_COUNT);
			s.Stream(ref this.animInfoLoaded);
			s.StreamSignature(CSaveMarker.TACTIC);
		}
		#endregion
	};
}