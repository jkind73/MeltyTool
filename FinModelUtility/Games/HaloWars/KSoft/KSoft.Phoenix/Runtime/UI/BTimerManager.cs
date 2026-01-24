
namespace KSoft.Phoenix.Runtime
{
	sealed class BTimerManager
		: IO.IEndianStreamSerializable
	{
		const int K_TIMER_COUNT_ = 0x60 / BTimer.K_SIZE_OF;

		public struct BTimer
			: IO.IEndianStreamSerializable
		{
			internal const int K_SIZE_OF = 0x18; // in-memory rep size

			public uint startTime, stopTime, currentTime, lastTime;
			public int id;
			public bool countUp, active, done, paused;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.startTime); s.Stream(ref this.stopTime); s.Stream(ref this.currentTime); s.Stream(ref this.lastTime);
				s.Stream(ref this.id);
				s.Stream(ref this.countUp); s.Stream(ref this.active); s.Stream(ref this.done); s.Stream(ref this.paused);
			}
			#endregion
		};
		public BTimer[] timers = new BTimer[K_TIMER_COUNT_];
		public int nextTimerId;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			for (int x = 0; x < this.timers.Length; x++)
				s.Stream(ref this.timers[x]);
			s.Stream(ref this.nextTimerId);
		}
		#endregion
	};
}