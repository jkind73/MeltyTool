using System.Numerics;

using fin.schema;

using schema.binary;

namespace tlpe.scb;

[BinarySchema]
public sealed partial class Section1 : ISection {
  [Unknown]
  private uint maybeSize_;

  [Unknown]
  private uint unk0_;

  [Unknown]
  private uint unk1_;

  [Unknown]
  private uint unk2_;

  [Unknown]
  private uint unk3_;

  [Unknown]
  private uint unk4_;

  [Unknown]
  private uint unk5_;

  [Unknown]
  private Vector3 unkVec3_;
  
  [Unknown]
  private uint unk6_;

  public Names Names { get; } = new();
}