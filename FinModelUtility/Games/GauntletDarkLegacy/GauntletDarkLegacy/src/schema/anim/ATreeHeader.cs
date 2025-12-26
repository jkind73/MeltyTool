using fin.data.dictionaries;

using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.anim;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/haekb/io_scene_gdl/blob/master/src/anim_model.py#L79
/// </summary>
[BinarySchema]
[LocalPositions]
public sealed partial class ATreeHeader : IBinaryDeserializable {
  private uint aTreeSequencesPointer_;
  private uint skeletalAnimationHeaderPointer_;
  private uint objectAnimationHeaderPointer_;
  private uint aNodeInfoPointer_;
  private uint aNodeCount_;
  private uint aTreeSequenceCount_;

  [StringLengthSource(32)]
  public string Name { get; set; }

  [RAtPosition(nameof(aNodeInfoPointer_))]
  [RSequenceLengthSource(nameof(aNodeCount_))]
  public ANodeInfo[] ANodeInfos { get; set; }

  [RAtPosition(nameof(aTreeSequencesPointer_))]
  [RSequenceLengthSource(nameof(aTreeSequenceCount_))]
  public ATreeSequence[] ATreeSequences { get; set; }

  [RAtPosition(nameof(skeletalAnimationHeaderPointer_))]
  public SkeletalAnimationHeader SkeletalAnimationHeader { get; } = new();

  [RAtPosition(nameof(objectAnimationHeaderPointer_))]
  public ObjectAnimationHeader ObjectAnimationHeader { get; } = new();
}