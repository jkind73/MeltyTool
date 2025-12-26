using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.anim;

[BinarySchema]
public sealed partial class ObjectAnimation : IBinaryDeserializable {
  [StringLengthSource(32)]
  public string Name { get; set; }

  private readonly int mbIndex_ = -1;

  public short FrameCount { get; set; }
  public short StartFrame { get; set; }
}