using System.Numerics;

using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace tlpe.scb;

/// <summary>
///   Probably animations.
/// </summary>
[BinarySchema]
public sealed partial class Section2 : ISection {
  [Unknown]
  private uint unk0_;

  [Unknown]
  private uint unkIndex0_;

  [Unknown]
  private uint unkCount0_;

  [Unknown]
  private uint unk1_;

  [Unknown]
  private uint unk2_;

  [WLengthOfSequence(nameof(Unk3s))]
  private uint unk3Count_;

  [Unknown]
  [RSequenceLengthSource(nameof(unk3Count_))]
  public Unk3[] Unk3s { get; set; }
}

[BinarySchema]
public sealed partial class Unk3 : IBinaryConvertible {
  [Unknown]
  public uint Unk0 { get; set; }

  [Unknown]
  public Vector3 Unk1 { get; set; }
}