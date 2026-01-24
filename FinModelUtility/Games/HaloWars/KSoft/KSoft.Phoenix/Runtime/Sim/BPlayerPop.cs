
namespace KSoft.Phoenix.Runtime
{
	struct BPlayerPop
		: IO.IEndianStreamSerializable
	{
		public float existing, cap, max, future;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.existing); s.Stream(ref this.cap); s.Stream(ref this.max); s.Stream(ref this.future);
		}
		#endregion
#if false
		public void ToStream(IO.IndentedTextWriter s, string popName)
		{
			s.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}",
				Existing.ToString("r"), Cap.ToString("r"), Max.ToString("r"), Future.ToString("r"),
				popName);
		}
#endif
	};
}