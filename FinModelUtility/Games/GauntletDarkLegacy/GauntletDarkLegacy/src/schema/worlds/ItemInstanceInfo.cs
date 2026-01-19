using System.Numerics;

using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.worlds;

public enum ItemType : uint {
  RANDOM = 0xFFFFFFFF,
  POWERUP = 0x1,
  CONTAINER = 0x2,
  GENERATOR = 0x3,
  ENEMYINFO = 0x4,
  TRIGGER = 0x5,
  TRAP = 0x6,
  DOOR = 0x7,
  DAMAGETILE = 0x8,
  EXIT = 0x9,
  OBSTACLE = 0xA,
  TRANSPORTER = 0xB,
  ROTATOR = 0xC,
  SOUND = 0xD,
}

public enum ItemSubType : uint {
  GOLD = 0x1,
  KEY = 0x2,
  FOOD = 0x3,
  POTION = 0x4,
  WEAPON = 0x5,
  ARMOR = 0x6,
  SPEED = 0x7,
  MAGIC = 0x8,
  SPECIAL = 0x9,
  RUNESTONE = 0xA,
  BOSSKEY = 0xB,
  OBELISK = 0xC,
  QUEST = 0xD,
  SCROLL = 0xE,
  GEMSTONE = 0xF,
  FEATHER = 0x10,
  BRIDGEPAD = 0x14,
  DOORPAD = 0x15,
  BRIDGESWITCH = 0x16,
  DOORSWITCH = 0x17,
  ACTIVESWITCH = 0x18,
  ELEVPAD = 0x19,
  ELEVSWITCH = 0x1A,
  LIFTPAD = 0x1B,
  LIFTSTART = 0x1C,
  LIFTEND = 0x1D,
  NOWEAPCOL = 0x1E,
  SHOOTTRIG = 0x1F,
  ROCKFALL = 0x28,
  SAFEROCK = 0x29,
  WALL = 0x2A,
  BARREL = 0x2B,
  BARREL_EXP = 0x2C,
  BARREL_POI = 0x2D,
  CHEST = 0x2E,
  CHEST_GOLD = 0x2F,
  CHEST_SILVER = 0x30,
  LEAFFALL = 0x31,
  SECRET = 0x32,
  ROCKFLY = 0x33,
  SHOOTFALL = 0x34,
  ROCKSINK = 0x35,
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/haekb/gdl-tools/blob/master/Addons/GDLFormat/Models/Worlds.gd#L125
/// </summary>
[BinarySchema]
public sealed partial class ItemInstanceInfo : IBinaryDeserializable {
  public ItemType Type { get; set; }
  public ItemSubType SubType { get; set; }
  public ushort CollisionType { get; set; }
  public ushort CollisionFlags { get; set; }
  public float Radius { get; set; }
  public float Height { get; set; }
  public float XDim { get; set; }
  public float ZDim { get; set; }
  public Vector3 CollisionOffset { get; set; }

  [StringLengthSource(16)]
  public string Description { get; set; }

  public uint MbFlags { get; set; }
  public uint Properties { get; set; }
  public ushort Value { get; set; }
  public short Armor { get; set; }
  public short Hitpoints { get; set; }
  public short ActiveType { get; set; }
  public short ActiveOff { get; set; }
  public short ActiveOn { get; set; }
  public uint AnimationTreeHeaderPointer { get; set; }

  public override string ToString() => this.Description;
}