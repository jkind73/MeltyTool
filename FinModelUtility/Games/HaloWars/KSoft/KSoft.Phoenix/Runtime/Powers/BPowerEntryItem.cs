
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerEntryItem
		: IO.IEndianStreamSerializable
		//, IO.IIndentedTextWritable
	{
		public BEntityID squadId;
		public int usesRemaining, timesUsed, chargeCap;
		public uint nextGrantTime;
		public bool infiniteUses, recharging;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.squadId);
			s.Stream(ref this.usesRemaining); s.Stream(ref this.timesUsed); s.Stream(ref this.chargeCap);
			s.Stream(ref this.nextGrantTime);
			s.Stream(ref this.infiniteUses); s.Stream(ref this.recharging);
		}
		#endregion

		#region IIndentedStreamWritable Members
#if false
		public void ToStream(IO.IndentedTextWriter s)
		{
			s.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
				UsesRemaining.ToString(), TimesUsed.ToString(), ChargeCap.ToString(),
				NextGrantTime.ToString(), InfiniteUses.ToString(), Recharging.ToString(),
				SquadID.ToString("X8"));
		}
#endif
		#endregion
	};
}