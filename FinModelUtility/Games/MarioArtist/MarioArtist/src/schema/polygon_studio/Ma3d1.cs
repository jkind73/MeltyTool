using schema.binary;
using schema.binary.attributes;

namespace marioartist.schema.polygon_studio;

[BinarySchema]
[Endianness(Endianness.BigEndian)]
public sealed partial class Ma3d1 : IBinaryDeserializable {
  public MfsThumbnail Thumbnail { get; } = new();
  public Ma3d1Header Header { get; } = new();
  public MeshData MeshData { get; } = new();
}