using schema.binary;

namespace gdl.schema.objects;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/haekb/io_scene_gdl/blob/master/src/model.py#L420
/// </summary>
[BinarySchema]
public sealed partial class SubObject : IBinaryDeserializable {
  public ushort Qwc { get; set; }
  public ushort TextureIndex { get; set; }
  public ushort Lodk { get; set; }
  public ushort LmIndex { get; set; }
}