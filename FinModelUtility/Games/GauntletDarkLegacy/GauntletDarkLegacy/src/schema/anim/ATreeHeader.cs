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
  private uint animHeaderPointer_;
  public uint ObjAnimHeaderPointer { get; set; }
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

  [RAtPosition(nameof(animHeaderPointer_))]
  public AnimHeader AnimHeader { get; } = new();
}