using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.anim;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/haekb/io_scene_gdl/blob/master/src/anim_model.py#L243
/// </summary>
[BinarySchema]
public sealed partial class ATreeSequence : IBinaryDeserializable {
  [StringLengthSource(32)]
  public string Name { get; set; }

  public ushort FrameCount { get; set; }
  public ushort FrameRate { get; set; }

  [IntegerFormat(SchemaIntegerType.UINT16)]
  public bool Loop { get; set; }

  public ushort FixPos { get; set; }
  public ushort EffectCount { get; set; }
  public ushort Flags { get; set; }
  public int EffectPointer { get; set; }
}