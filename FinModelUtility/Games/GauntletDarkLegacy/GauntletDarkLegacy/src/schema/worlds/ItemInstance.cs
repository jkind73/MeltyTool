using System.Numerics;

using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.worlds;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/haekb/gdl-tools/blob/master/Addons/GDLFormat/Models/Worlds.gd#L147
/// </summary>
[BinarySchema]
public sealed partial class ItemInstance : IBinaryDeserializable {
  public short InstanceIndex { get; set; }
  public sbyte MinPlayers { get; set; }
  public byte Flags { get; set; }
  public short CollisionTriangleIndex { get; set; }
  public short CollisionTriangleCount { get; set; }

  [StringLengthSource(16)]
  public string Description { get; set; }

  public Vector3 Position { get; set; }
  public Vector3 Rotation { get; set; }

  public byte[] Params { get; } = new byte[12];
}