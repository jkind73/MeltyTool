
using BVector = System.Numerics.Vector4;
using BAbilityID = System.Int32;
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BSimTarget
		: IO.IEndianStreamSerializable
	{
		public BVector position;
		public BEntityID id;
		public float range;
		public BAbilityID abilityId;
		public bool positionValid, rangeValid, abilityIdValid;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamV(ref this.position);
			s.Stream(ref this.id);
			s.Stream(ref this.range);
			s.Stream(ref this.abilityId);
			s.Stream(ref this.positionValid); s.Stream(ref this.rangeValid); s.Stream(ref this.abilityIdValid);
		}
		#endregion
	};
}