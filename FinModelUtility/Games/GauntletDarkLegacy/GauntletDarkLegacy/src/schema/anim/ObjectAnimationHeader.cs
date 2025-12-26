using fin.data.dictionaries;

using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.anim;

[BinarySchema]
[LocalPositions]
public sealed partial class ObjectAnimationHeader
    : IBinaryDeserializable, IChildOf<ATreeHeader> {
  public ATreeHeader Parent { get; set; }

  private uint objAnimPointer_;
  private uint objAnimCount_;

  [RAtPosition(nameof(objAnimPointer_))]
  [RSequenceLengthSource(nameof(objAnimCount_))]
  public ObjectAnimation[] ObjectAnimations { get; private set; }
}