
using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;
using BUnitOppID = System.Int32;
using BUnitOppType = System.Byte;

namespace KSoft.Phoenix.Runtime
{
	sealed class BUnitOpp
		: IO.IEndianStreamSerializable
	{
		const int C_MAXIMUM_PATH_LENGTH_ = 0xC8;

		internal static readonly FreeListInfo KFreeListInfo = new FreeListInfo(CSaveMarker.UNIT_OPP)
		{
			MaxCount=0x4E20,
		};

		public BVector[] path;
		public BSimTarget Target { get; private set; } = new BSimTarget();
		public BEntityID source;
		public BUnitOppID id;
		public BUnitOppType type;
		public ushort userData;
		public byte priority;
		public byte userData2;
		public ushort waitCount;
		public bool evaluated, existForOneUpdate, existUntilEvaluated,
			allowComplete, notifySource, leash,
			forceLeash, trigger, removeActions,
			complete, completeValue, preserveDps,
			mustComplete, userDataSet;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamVectorArray(s, ref this.path, C_MAXIMUM_PATH_LENGTH_);
			s.Stream(this.Target);
			s.Stream(ref this.source);
			s.Stream(ref this.id);
			s.Stream(ref this.type);
			s.Stream(ref this.userData);
			s.Stream(ref this.priority);
			s.Stream(ref this.userData2);
			s.Stream(ref this.waitCount);
			s.Stream(ref this.evaluated); s.Stream(ref this.existForOneUpdate); s.Stream(ref this.existUntilEvaluated);
			s.Stream(ref this.allowComplete); s.Stream(ref this.notifySource); s.Stream(ref this.leash);
			s.Stream(ref this.forceLeash); s.Stream(ref this.trigger); s.Stream(ref this.removeActions);
			s.Stream(ref this.complete); s.Stream(ref this.completeValue); s.Stream(ref this.preserveDps);
			s.Stream(ref this.mustComplete); s.Stream(ref this.userDataSet);
		}
		#endregion
	};
}
