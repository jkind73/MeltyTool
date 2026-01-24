
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	abstract class BPowerUser : IO.IEndianStreamSerializable
	{
		public uint value;
		public uint type;
		public bool initialized, destroy, noCost, checkPowerLocation;
		public sbyte protoPowerId, powerLevel;
		public BEntityID ownerSquadId;
		public bool usedByPrimaryUser;
		public double elapsed;

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.value);
			s.Stream(ref this.type);
			s.Stream(ref this.initialized); s.Stream(ref this.destroy); s.Stream(ref this.noCost); s.Stream(ref this.checkPowerLocation);
			s.Stream(ref this.protoPowerId); s.Stream(ref this.powerLevel);
			s.Stream(ref this.ownerSquadId);
			s.Stream(ref this.usedByPrimaryUser);
			s.Stream(ref this.elapsed);
		}
		#endregion

		internal static BPowerUser FromType(Phx.BPowerType type)
		{
			switch (type)
			{
			case Phx.BPowerType.CLEANSING: return new BPowerCovGlassingUser();
			case Phx.BPowerType.ORBITAL: return new BPowerUnscMacUser();
			case Phx.BPowerType.CARPET_BOMBING: return new BPowerUnscCarpetBombUser();
			case Phx.BPowerType.CRYO: return new BPowerUnscCryoUser();
			case Phx.BPowerType.RAGE: return new BPowerCovRageUser();
			case Phx.BPowerType.WAVE: return new BPowerCovDebrisUser();
			case Phx.BPowerType.DISRUPTION: return new BPowerUnscDisruptionUser();
			case Phx.BPowerType.TRANSPORT: return new BPowerTransportUser();
			case Phx.BPowerType.ODST: return new BPowerUnscOdstUser();
			case Phx.BPowerType.REPAIR: return new BPowerUnscHealUser();

			default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}
	};
}