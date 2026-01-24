using schema.binary;

namespace grezzo.schema.cmb.luts;

[BinarySchema]
public sealed partial class LutKeyframe : IBinaryConvertible {
  public float inSlope;
  public float outSlope;
  public int frame;
  public float value;
}