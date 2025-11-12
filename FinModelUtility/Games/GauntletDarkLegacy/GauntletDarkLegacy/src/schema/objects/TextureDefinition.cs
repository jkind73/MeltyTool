using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.objects;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/haekb/io_scene_gdl/blob/master/src/model.py#L361
/// </summary>
[BinarySchema]
public sealed partial class TextureDefinition : IBinaryDeserializable {
  [StringLengthSource(30)]
  public string Name { get; set; }

  public ushort Index { get; set; }
  public ushort Width { get; set; }
  public ushort Height { get; set; }
}