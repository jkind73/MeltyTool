using System.Numerics;

using fin.schema;
using fin.schema.matrix;

using schema.binary;
using schema.binary.attributes;

namespace tlpe.scb;

[BinarySchema]
public sealed partial class Section6 : ISection {
  [Unknown]
  private uint maybeSize_;

  [Unknown]
  private uint maybeCount_;

  [Unknown]
  private uint unk0_;

  [Unknown]
  private uint unk1_;

  [Unknown]
  public Matrix3x4f UnkMatrix { get; } = new();

  [WLengthOfString(nameof(Name0))]
  private uint length0_;

  [WLengthOfString(nameof(TextureName))]
  private uint length1_;

  [RStringLengthSource(nameof(length0_))]
  public string Name0 { get; set; }

  [RStringLengthSource(nameof(length1_))]
  public string TextureName { get; set; }
}