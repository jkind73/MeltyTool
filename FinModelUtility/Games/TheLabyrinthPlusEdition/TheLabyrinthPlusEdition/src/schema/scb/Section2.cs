using System.Numerics;

using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace tlpe.scb;

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

  [WLengthOfSequence(nameof(UnkVec3s))]
  private uint vectorCount_;

  [Unknown]
  private uint unk3_;

  [Unknown]
  private uint unk4_;

  [Unknown]
  [RSequenceLengthSource(nameof(vectorCount_))]
  public Vector3[] UnkVec3s { get; set; }
}