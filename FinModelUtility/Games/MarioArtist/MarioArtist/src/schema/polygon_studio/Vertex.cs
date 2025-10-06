using schema.binary;

namespace marioartist.schema.polygon_studio;

[BinarySchema]
public sealed partial class Vertex : IBinaryDeserializable {
  public short X { get; set; }
  public short Y { get; set; }
  public short Z { get; set; }

  public sbyte NormalX { get; set; }
  public sbyte NormalY { get; set; }
  public sbyte NormalZ { get; set; }

  public byte Unk0 { get; set; }
}