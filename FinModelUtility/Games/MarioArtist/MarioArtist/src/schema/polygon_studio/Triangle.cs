using schema.binary;

namespace marioartist.schema.polygon_studio;

[BinarySchema]
public sealed partial class Triangle : IBinaryDeserializable {
  public ushort VertexIndex0 { get; set; }
  public ushort VertexIndex1 { get; set; }
  public ushort VertexIndex2 { get; set; }

  public uint Unk0 { get; set; }
  public uint Unk1 { get; set; }
}