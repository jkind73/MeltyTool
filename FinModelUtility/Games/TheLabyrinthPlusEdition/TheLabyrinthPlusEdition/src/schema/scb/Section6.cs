using System.Numerics;

using fin.schema;
using fin.schema.matrix;

using schema.binary;

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

  public Names Names { get; } = new();
}