using System.Numerics;

using schema.binary;
using schema.binary.attributes;

namespace pikmin1.schema.mod;

[BinarySchema]
public sealed partial class JointMatPoly : IBinaryConvertible {
  public ushort matIdx = 0;
  public ushort meshIdx = 0;
}

[BinarySchema]
public sealed partial class Joint : IBinaryConvertible {
  public uint parentIdx = 0;
  public uint flags = 0;
  public Vector3 BoundsMax { get; set; }
  public Vector3 BoundsMin { get; set; }
  public float volumeRadius = 0;
  public Vector3 Scale { get; set; }
  public Vector3 Rotation { get; set; }
  public Vector3 Position { get; set; }

  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public JointMatPoly[] matpolys;
}