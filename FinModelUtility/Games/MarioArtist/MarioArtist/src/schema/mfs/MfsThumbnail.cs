using schema.binary;
namespace marioartist.schema;

[BinarySchema]
public sealed partial class MfsThumbnail : IBinaryDeserializable {
  public Argb1555Image Image { get; } = new(24, 24);
}