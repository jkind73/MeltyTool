#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Runtime
{
	using BActionTypeStreamer = IO.EnumBinaryStreamer<Phx.BActionType, byte>;

	partial class CSaveMarker
	{
		public const ushort
			#region 0x1
			C_SAVE_MARKER_B_ENTITY_ACTION_IDLE = 0x2710,
			C_SAVE_MARKER_B_ENTITY_ACTION_LISTEN = 0x2711,
			C_SAVE_MARKER_B_UNIT_ACTION_MOVE = 0x2712,
			C_SAVE_MARKER_B_UNIT_ACTION_MOVE_AIR = 0x2713,

			#endregion

			C_SAVE_MARKER_B_UNIT_ACTION_RAGE = 0x276A
			;
	};

	public struct ActionListEntry
		: IO.IEndianStreamSerializable
	{
		internal static ActionListEntry invalid = new ActionListEntry();

		public bool action;
		public Phx.BActionType actionType;
		public int actionPtr;

	#region IEndianStreamSerializable Members
	public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.action);
			if (this.action)
			{
				s.Stream(ref this.actionType, BActionTypeStreamer.Instance);
				BSaveGame.StreamFreeListItemPtr(s, ref this.actionPtr);
			}
			else
			{
				this.actionType = Phx.BActionType.INVALID;
				this.actionPtr = TypeExtensions.K_NONE;
			}
		}
		#endregion
	};

	public sealed class BActionManager
	{
		const int C_ACTION_LIST_MAXIMUM_COUNT_ = 0xC8;

		public static IO.EndianStream StreamActionList(IO.EndianStream s, ref ActionListEntry[] actionList)
		{
			Contract.Requires(s.IsReading || actionList != null);

			bool reading = s.IsReading;

			var count = (byte)(reading ? 0 : actionList.Length);
			s.Stream(ref count);
			if (reading)
			{
				Contract.Assert(count <= C_ACTION_LIST_MAXIMUM_COUNT_);
				actionList = new ActionListEntry[count];
			}

			for (byte x = 0; x < count; x++)
			{
				var expectedIndex = x;
				s.Stream(ref expectedIndex);
				if (reading)
				{
					Contract.Assert(expectedIndex != CSaveMarker.ITERATOR_END_U_INT8);
					Contract.Assert(expectedIndex == x);
				}

				var t = reading ? ActionListEntry.invalid : actionList[x];
				t.Serialize(s);
				if (reading)
					actionList[x] = t;
			}

			s.StreamSignature(CSaveMarker.ITERATOR_END_U_INT8);

			return s;
		}
	};
}