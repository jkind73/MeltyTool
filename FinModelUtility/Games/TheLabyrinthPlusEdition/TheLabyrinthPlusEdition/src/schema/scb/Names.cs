using schema.binary;
using schema.binary.attributes;

namespace tlpe.scb;

[BinarySchema]
public sealed partial class Names : IBinaryConvertible {
  [WLengthOfString(nameof(Name0))]
  private uint length0_;

  [WLengthOfString(nameof(Name1))]
  private uint length1_;

  [RStringLengthSource(nameof(length0_))]
  public string Name0 { get; set; }

  [RStringLengthSource(nameof(length1_))]
  public string Name1 { get; set; }
}