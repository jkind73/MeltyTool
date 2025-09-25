using schema.binary;

namespace visceral.schema.dat;

[BinarySchema]
public sealed partial class Dat : IBinaryConvertible {
  private readonly string magic_ = "BIGH";
}