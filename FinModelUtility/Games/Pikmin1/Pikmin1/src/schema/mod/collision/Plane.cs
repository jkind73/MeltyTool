using System.Numerics;

using schema.binary;

namespace pikmin1.schema.mod.collision;

[BinarySchema]
public sealed partial class Plane : IBinaryConvertible {
  public Vector3 Position { get; private set; }
  public float diameter;
}