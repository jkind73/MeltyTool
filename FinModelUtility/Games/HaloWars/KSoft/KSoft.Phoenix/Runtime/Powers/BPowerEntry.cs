
using BProtoPowerID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerEntry
		: IO.IEndianStreamSerializable
		//, IO.IIndentedTextWritable
	{
		public BPowerEntryItem[] items;
		public BProtoPowerID protoPowerId;
		public int timesUsed, iconLocation;
		public bool ignoreCost, ignoreTechPrereqs, ignorePop;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray16(s, ref this.items);
			s.Stream(ref this.protoPowerId);
			s.Stream(ref this.timesUsed); s.Stream(ref this.iconLocation);
			s.Stream(ref this.ignoreCost); s.Stream(ref this.ignoreTechPrereqs); s.Stream(ref this.ignorePop);
		}
		#endregion

		#region IIndentedStreamWritable Members
#if false
		public void ToStream(IO.IndentedTextWriter s)
		{
			var sg = s.Owner as BSaveGame;

			s.WriteLine("{1}\t{2}\t{3}\t{4}\t{5}\t{0}", sg.Database.ProtoPowers[ProtoPowerID],
				TimesUsed.ToString(), IconLocation.ToString(),
				IgnoreCost.ToString(), IgnoreTechPrereqs.ToString(), IgnorePop.ToString());
			using (s.EnterIndentBookmark())
				for (int x = 0; x < Items.Length; x++)
					Items[x].ToStream(s);
		}
#endif
		#endregion
	};
}