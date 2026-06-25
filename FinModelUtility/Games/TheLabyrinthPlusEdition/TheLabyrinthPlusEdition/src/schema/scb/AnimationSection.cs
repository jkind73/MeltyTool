using System.Numerics;

using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace tlpe.scb;

[BinarySchema]
public sealed partial class AnimationSection : ISection {
  [Unknown]
  private uint unk0_;

  public uint Id { get; set; }

  [Unknown]
  private uint unkCount0_;

  [Unknown]
  private uint unk1_;

  [Unknown]
  private uint unk2_;

  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public ScbKeyframe[] Keyframes { get; set; }
}

[BinarySchema]
public sealed partial class ScbKeyframe : IBinaryConvertible {
  public uint Frame { get; set; }
  public Vector3 EulerRadians { get; set; }
}