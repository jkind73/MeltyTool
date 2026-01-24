using BEntityID = System.Int32;
using BPlayerID = System.Int32;
using BTeamID = System.Int32;
using BObjectTypeID = System.Int32;
using BProtoObjectID = System.Int32;
using BProtoSquadID = System.Int32;
using BRelationType = System.Byte;

namespace KSoft.Phoenix.Runtime
{
	abstract class BEntityFilterBase
		: IO.IEndianStreamSerializable
	{
		public byte type;
		public bool isInverted, appliesToUnits, 
			appliesToSquads, appliesToEntities;

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.type);
			s.Stream(ref this.isInverted); s.Stream(ref this.appliesToUnits); 
			s.Stream(ref this.appliesToSquads); s.Stream(ref this.appliesToEntities);
		}
		#endregion

		internal static BEntityFilterBase FromType(int type)
		{
			switch (type)
			{
			case BEntityFilterIsAlive.K_TYPE: return new BEntityFilterIsAlive();
			case BEntityFilterIsIdle.K_TYPE: return new BEntityFilterIsIdle();
			case BEntityFilterEntities.K_TYPE: return new BEntityFilterEntities();
			case BEntityFilterPlayers.K_TYPE: return new BEntityFilterPlayers();
			case BEntityFilterTeams.K_TYPE: return new BEntityFilterTeams();
			case BEntityFilterProtoObjects.K_TYPE: return new BEntityFilterProtoObjects();
			case BEntityFilterProtoSquads.K_TYPE: return new BEntityFilterProtoSquads();
			case BEntityFilterObjectTypes.K_TYPE: return new BEntityFilterObjectTypes();
			case BEntityFilterRefCountTypes.K_TYPE: return new BEntityFilterRefCountTypes();
			case BEntityFilterDiplomacy.K_TYPE: return new BEntityFilterDiplomacy();
			case BEntityFilterMaxObjectType.K_TYPE: return new BEntityFilterMaxObjectType();
			case BEntityFilterIsSelected.K_TYPE: return new BEntityFilterIsSelected();
			case BEntityFilterCanChangeOwner.K_TYPE: return new BEntityFilterCanChangeOwner();
			case BEntityFilterJacking.K_TYPE: return new BEntityFilterJacking();

			default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}
	};
	sealed class BEntityFilterIsAlive
		: BEntityFilterBase
	{
		public const int K_TYPE = 0;
	};
	sealed class BEntityFilterIsIdle
		: BEntityFilterBase
	{
		public const int K_TYPE = 1;
	};
	sealed class BEntityFilterEntities
		: BEntityFilterBase
	{
		public const int K_TYPE = 2;

		public BEntityID[] entityList;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray(s, ref this.entityList);

			base.Serialize(s);
		}
		#endregion
	};
	sealed class BEntityFilterPlayers
		: BEntityFilterBase
	{
		public const int K_TYPE = 3;

		public BPlayerID[] players;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray(s, ref this.players);

			base.Serialize(s);
		}
		#endregion
	};
	sealed class BEntityFilterTeams
		: BEntityFilterBase
	{
		public const int K_TYPE = 4;

		public BTeamID[] teams;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray(s, ref this.teams);

			base.Serialize(s);
		}
		#endregion
	};
	sealed class BEntityFilterProtoObjects
		: BEntityFilterBase
	{
		public const int K_TYPE = 5;

		public BProtoObjectID[] protoObjects;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray16(s, ref this.protoObjects);

			base.Serialize(s);
		}
		#endregion
	};
	sealed class BEntityFilterProtoSquads
		: BEntityFilterBase
	{
		public const int K_TYPE = 6;

		public BProtoSquadID[] protoSquads;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray16(s, ref this.protoSquads);

			base.Serialize(s);
		}
		#endregion
	};
	sealed class BEntityFilterObjectTypes
		: BEntityFilterBase
	{
		public const int K_TYPE = 7;

		public BObjectTypeID[] objectTypes;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray16(s, ref this.objectTypes);

			base.Serialize(s);
		}
		#endregion
	};
	sealed class BEntityFilterRefCountTypes
		: BEntityFilterBase
	{
		public const int K_TYPE = 8;

		public int refCountType, compareType, count;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.refCountType); s.Stream(ref this.compareType); s.Stream(ref this.count);

			base.Serialize(s);
		}
		#endregion
	};
	sealed class BEntityFilterDiplomacy
		: BEntityFilterBase
	{
		public const int K_TYPE = 9;

		public BRelationType relationType;
		public BTeamID teamId;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.relationType);
			s.Stream(ref this.teamId);

			base.Serialize(s);
		}
		#endregion
	};
	sealed class BEntityFilterMaxObjectType
		: BEntityFilterBase
	{
		public const int K_TYPE = 10;

		public BObjectTypeID objectTypeId;
		public uint maxCount;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.objectTypeId);
			s.Stream(ref this.maxCount);

			base.Serialize(s);
		}
		#endregion
	};
	sealed class BEntityFilterIsSelected
		: BEntityFilterBase
	{
		public const int K_TYPE = 11;

		public BPlayerID playerId;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.playerId);

			base.Serialize(s);
		}
		#endregion
	};
	sealed class BEntityFilterCanChangeOwner
		: BEntityFilterBase
	{
		public const int K_TYPE = 12;
	};
	sealed class BEntityFilterJacking
		: BEntityFilterBase
	{
		public const int K_TYPE = 13;
	};

	struct BEntityFilter : IO.IEndianStreamSerializable
	{
		public byte type;
		public BEntityFilterBase filter;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.type);
			int type = this.type; // since this is a struct we have to declare a local copy for the lambda
			s.Stream(ref this.filter,
				() => BEntityFilterBase.FromType(type));
		}
		#endregion
	};
	sealed class BEntityFilterSet
		: IO.IEndianStreamSerializable
	{
		public const int K_MAX_COUNT = 0x3E8;

		public BEntityFilter[] filters;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray16(s, ref this.filters, maxCount:K_MAX_COUNT);
		}
		#endregion
	};
}
