using schema.binary;

namespace mdl.schema {
  [BinarySchema]
  public sealed partial class DrawElement : IBinaryConvertible {
    public ushort MaterialIndex { get; set; }
    public ushort ShapeIndex { get; set; }
  }
}