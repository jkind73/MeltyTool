
using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;
using BPowerUserID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	abstract class BPower
		: IO.IEndianStreamSerializable
	{
		public int id;
		public uint type;
		public BPowerUserID powerUserId;
		public sbyte protoPowerId, powerLevel;
		public float maintenanceSupplies;
		public double elapsed;
		public sbyte playerId;
		public BEntityID ownerId;
		public BVector targetLocation;
		public bool destroy, ignoreAllReqs, checkPowerLocation;

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.id);
			s.Stream(ref this.type);
			s.Stream(ref this.powerUserId);
			s.Stream(ref this.protoPowerId); s.Stream(ref this.powerLevel);
			s.Stream(ref this.maintenanceSupplies);
			s.Stream(ref this.elapsed);
			s.Stream(ref this.playerId);
			s.Stream(ref this.ownerId);
			s.StreamV(ref this.targetLocation);
			s.Stream(ref this.destroy); s.Stream(ref this.ignoreAllReqs); s.Stream(ref this.checkPowerLocation);
		}
		#endregion

		internal static BPower FromType(Phx.BPowerType type)
		{
			switch (type)
			{
			case Phx.BPowerType.CLEANSING: return new BPowerCovGlassing();
			case Phx.BPowerType.ORBITAL: return new BPowerUnscMac();
			case Phx.BPowerType.CARPET_BOMBING: return new BPowerUnscCarpetBomb();
			case Phx.BPowerType.CRYO: return new BPowerUnscCryo();
			case Phx.BPowerType.RAGE: return new BPowerCovRage();
			case Phx.BPowerType.WAVE: return new BPowerCovDebris();
			case Phx.BPowerType.DISRUPTION: return new BPowerUnscDisruption();
			case Phx.BPowerType.TRANSPORT: return new BPowerTransport();
			case Phx.BPowerType.ODST: return new BPowerUnscOdst();
			case Phx.BPowerType.REPAIR: return new BPowerUnscHeal();

			default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}
	};
}