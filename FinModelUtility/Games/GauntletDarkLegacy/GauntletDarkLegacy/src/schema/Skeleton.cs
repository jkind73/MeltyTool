using schema.binary;
using schema.binary.attributes;

namespace gdl.schema;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/haekb/io_scene_gdl/blob/master/src/anim_model.py#L54
/// </summary>
[BinarySchema]
public sealed partial class Skeleton : IBinaryDeserializable {
  [StringLengthSource(32)]
  public string Name { get; set; }

  public uint SkeletonDataPointer { get; set; }

  [RAtPosition(nameof(SkeletonDataPointer))]
  public SkeletonData Data { get; } = new();
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/haekb/io_scene_gdl/blob/master/src/anim_model.py#L79
/// </summary>
[BinarySchema]
[LocalPositions]
public sealed partial class SkeletonData : IBinaryDeserializable {
  public uint AnimationHeaderPointer { get; set; }
  public uint AnimationDataPointer { get; set; }
  public uint UnkPointer { get; set; }
  public uint BonePointer { get; set; }
  public uint BoneCount { get; set; }
  public uint AnimationCount { get; set; }

  [StringLengthSource(32)]
  public string Name { get; set; }

  [RAtPosition(nameof(BonePointer))]
  [RSequenceLengthSource(nameof(BoneCount))]
  public Bone[] Bones { get; set; }
}