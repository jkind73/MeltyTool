using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.anim;

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
  private uint animationHeaderPointer_;
  private uint animationDataPointer_;
  public uint UnkPointer { get; set; }
  private uint bonePointer_;
  private uint boneCount_;
  private uint animationCount_;

  [StringLengthSource(32)]
  public string Name { get; set; }

  [RAtPosition(nameof(bonePointer_))]
  [RSequenceLengthSource(nameof(boneCount_))]
  public Bone[] Bones { get; set; }

  [RAtPosition(nameof(animationHeaderPointer_))]
  [RSequenceLengthSource(nameof(animationCount_))]
  public AnimationHeader[] AnimationHeaders { get; set; }

  [RAtPosition(nameof(animationDataPointer_))]
  [RSequenceLengthSource(nameof(animationCount_))]
  public AnimationData[] AnimationDatas { get; set; }
}