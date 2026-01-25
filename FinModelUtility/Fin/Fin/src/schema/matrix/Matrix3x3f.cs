using schema.binary;

namespace fin.schema.matrix;

[BinarySchema]
public sealed partial class Matrix3X3F : IBinaryConvertible {
  public float[] Values { get; } = new float[3 * 3];

  public float this[int row, int column] {
    get => this.Values[3 * row + column];
    set => this.Values[3 * row + column] = value;
  }
}