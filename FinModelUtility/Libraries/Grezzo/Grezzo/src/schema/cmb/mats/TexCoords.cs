using fin.schema.vector;

using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb.mats;

[BinarySchema]
public sealed partial class TexCoords : IBinaryDeserializable {
  public byte CoordinateIndex { get; private set; }
  public byte ReferenceCameraIndex { get; private set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public TextureMappingType MappingMethod { get; private set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public TextureMatrixMode MatrixMode { get; private set; }

  public Vector2f Scale { get; } = new();
  public Vector2f Translation { get; } = new();
  public float Rotation { get; private set; }
}