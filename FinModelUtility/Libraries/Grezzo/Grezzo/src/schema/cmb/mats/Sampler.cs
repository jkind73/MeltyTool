using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb.mats;

[BinarySchema]
public sealed partial class Sampler : IBinaryConvertible {
  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool IsAbs { get; private set; }
  public sbyte Index { get; private set; }
  public LutInput Input { get; private set; }

  // TODO: LutScale only accepts these values
  // Quarter = 0.25,
  // Half = 0.5,
  // One = 1.0,
  // Two = 2.0,
  // Four = 4.0,
  // Eight = 8.0
  public float Scale { get; private set; }
}