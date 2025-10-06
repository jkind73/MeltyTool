using schema.binary;

namespace marioartist.schema.polygon_studio;

[BinarySchema]
public sealed partial class Ma3d1Header : IBinaryDeserializable {
  public uint MeshCount { get; set; }
  public uint VertexCount { get; set; }
  public uint TriangleCount { get; set; }

  public uint MeshDataOffset { get; set; }
  public uint MeshSize { get; set; }

  public uint TextureOffset { get; set; }
  public uint TextureSize { get; set; }
}