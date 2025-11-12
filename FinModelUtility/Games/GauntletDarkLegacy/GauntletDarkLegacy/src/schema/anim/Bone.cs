using System.Numerics;

using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.anim;

public enum BoneType : ushort {
  NULL = 0,
  SKEL_ANIM = 1,
  OBJ_ANIM = 2, //  Vertex Animation
  TEX_ANIM = 3,
  PSYS_ANIM = 4,
  EMPTY = 0xFFFF,
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/haekb/io_scene_gdl/blob/master/src/anim_model.py#L148
/// </summary>
[BinarySchema]
public sealed partial class Bone : IBinaryDeserializable {
  [StringLengthSource(32)]
  public string Name { get; set; }

  public Vector3 Position { get; set; }
  public BoneType Type { get; set; }
  public ushort Flags { get; set; }
  public uint MbFlags { get; set; }
  public uint SequencePointer { get; set; }
  public int ParentId { get; set; }
}