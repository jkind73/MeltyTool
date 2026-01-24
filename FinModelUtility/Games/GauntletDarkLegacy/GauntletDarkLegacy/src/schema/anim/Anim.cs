using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.anim;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/haekb/io_scene_gdl/blob/master/src/anim_model.py#L5
/// </summary>
[BinarySchema]
[Endianness(Endianness.LittleEndian)]
public sealed partial class Anim : IBinaryDeserializable {
  public ushort ATreeCount { get; set; }
  public ushort Version { get; set; }
  public uint ATreeInfoPointer { get; set; }
  public uint EffectCount { get; set; }
  public uint EffectPointer { get; set; }

  [Skip]
  private bool HasPsys => this.Version != 0;

  [RIfBoolean(nameof(HasPsys))]
  public int? PsysCount { get; set; }

  [RIfBoolean(nameof(HasPsys))]
  public int? PsysPointer { get; set; }

  [RSequenceLengthSource(nameof(ATreeCount))]
  public ATreeInfo[] Atrees { get; set; }
}