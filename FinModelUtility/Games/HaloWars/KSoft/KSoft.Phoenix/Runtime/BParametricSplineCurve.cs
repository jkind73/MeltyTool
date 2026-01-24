
using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.Runtime
{
	sealed class BParametricSplineCurve
		: IO.IEndianStreamSerializable
	{
		public BVector a0, a1, a2;
		public bool valid;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamV(ref this.a0);
			s.StreamV(ref this.a1);
			s.StreamV(ref this.a2);
			s.Stream(ref this.valid);
		}
		#endregion
	};
}