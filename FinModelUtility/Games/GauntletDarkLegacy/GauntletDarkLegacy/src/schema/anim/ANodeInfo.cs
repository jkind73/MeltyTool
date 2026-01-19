using System.Numerics;

using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.anim;

public enum AnimType : ushort {
  NULL = 0,
  SKEL_ANIM = 1,
  OBJ_ANIM = 2, //  Vertex Animation
  TEX_ANIM = 3,
  PSYS_ANIM = 4,
  EMPTY = 0xFFFF,
}

[Flags]
public enum MbFlags : uint {
  NO_Z_TEST = 1 << 6,
  NO_Z_WRITE = 1 << 7,
  NO_SHADING = 1 << 12,
  BLEND_ADD = 1 << 23,
  YAW_ONLY_BILLBOARD = 1 << 24,
  YAW_AND_PITCH_BILLBOARD = 1 << 26,
  BLEND_MULTIPLY = 1 << 30,
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/haekb/io_scene_gdl/blob/master/src/anim_model.py#L148
/// </summary>
[BinarySchema]
public sealed partial class ANodeInfo : IBinaryDeserializable {
  [StringLengthSource(32)]
  public string MbDesc { get; set; }

  public Vector3 InitialPosition { get; set; }
  public AnimType Type { get; set; }
  public ushort Flags { get; set; }
  public MbFlags MbFlags { get; set; }
  public uint SequenceOffset { get; set; }
  public int ParentId { get; set; }
}