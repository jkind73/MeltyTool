using System.Numerics;

using gdl.schema.anim;

using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.worlds;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/MosesofEgypt/gdl_tools/blob/47799f19ec85fa49af87eec4c64a27ee1fc87d7c/gdl/defs/worlds.py#L330
/// </summary>
[BinarySchema]
public sealed partial class WorldObject : IBinaryDeserializable {
  [StringLengthSource(16)]
  public string Name { get; set; }

  public uint Flags { get; set; }

  public short TriggerType { get; set; }
  public sbyte TriggerState { get; set; }
  public sbyte PTriggerState { get; set; }
 
  public MbFlags MbFlags { get; set; }
  public Vector3 Position { get; set; }
  public uint MbNodePointer { get; set; }
  public short NextIndex { get; set; }
  public short ChildIndex { get; set; }
  public float Radius { get; set; }
  public byte Checked { get; set; }
  public byte NoCollision { get; set; }
  public short CollisionTriangleCount { get; set; }
  public int CollisionTriangleIndex { get; set; }
}