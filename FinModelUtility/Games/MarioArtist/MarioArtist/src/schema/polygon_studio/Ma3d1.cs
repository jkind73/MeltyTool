using schema.binary;
using schema.binary.attributes;

namespace marioartist.schema.polygon_studio;

[BinarySchema]
[Endianness(Endianness.BigEndian)]
public partial class Ma3d1 : IBinaryDeserializable {
  public MfsThumbnail Thumbnail { get; } = new();
  public Header Header { get; } = new();

  [RAtPositionOrNull(nameof(Header.MeshOffset))]
  public Mesh? FirstMesh { get; private set; }
}

[BinarySchema]
public sealed partial class Header : IBinaryDeserializable {
  public uint ModelCount { get; set; }
  public uint VertexCount { get; set; }
  public uint TriangleCount { get; set; }

  public uint MeshOffset { get; set; }
  public uint ModelSize { get; set; }

  public uint TextureOffset { get; set; }
  public uint TextureSize { get; set; }
}