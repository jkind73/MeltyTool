using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.anim;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/haekb/io_scene_gdl/blob/master/src/anim_model.py#L54
/// </summary>
[BinarySchema]
public sealed partial class ATreeInfo : IBinaryDeserializable {
  [StringLengthSource(32)]
  public string Name { get; set; }

  public uint Offset { get; set; }

  [RAtPosition(nameof(Offset))]
  public ATreeHeader Data { get; } = new();
}