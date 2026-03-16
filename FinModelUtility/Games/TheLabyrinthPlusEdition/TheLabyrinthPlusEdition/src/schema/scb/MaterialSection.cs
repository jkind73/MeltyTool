using System.Numerics;

using fin.schema;
using fin.schema.color;
using fin.schema.matrix;

using schema.binary;
using schema.binary.attributes;

namespace tlpe.scb;

[BinarySchema]
public sealed partial class MaterialSection : ISection {
  [Unknown]
  private uint maybeSize_;

  public uint Id { get; set; }

  [Unknown]
  private uint unk0_;

  [Unknown]
  private uint unk1_;

  public Rgba4f DiffuseColor { get; } = new();

  [Unknown]
  public Rgba4f UnkColor0 { get; } = new();

  [Unknown]
  public Rgba4f UnkColor1 { get; } = new();

  [WLengthOfString(nameof(Name))]
  private uint length0_;

  [WLengthOfString(nameof(TextureName))]
  private uint length1_;

  [RStringLengthSource(nameof(length0_))]
  public string Name { get; set; }

  [RStringLengthSource(nameof(length1_))]
  public string TextureName { get; set; }
}