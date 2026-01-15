using schema.binary;

namespace rct.schema.sv4;

[BinarySchema]
public sealed partial class GameTime : IBinaryConvertible {
  public ushort Month { get; set; }
  public ushort Day { get; set; }
  public uint Counter { get; set; }
}