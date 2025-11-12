using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.objects;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/haekb/io_scene_gdl/blob/master/src/model.py#L313
/// </summary>
[BinarySchema]
public sealed partial class ObjectDefinition : IBinaryDeserializable {
  [StringLengthSource(16)]
  public string Name { get; set; }

  public float BndRad { get; set; }
  public ushort Index { get; set; }
  public ushort FrameCount { get; set; }
}