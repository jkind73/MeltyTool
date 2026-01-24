using fin.schema;

using gx;
using gx.displayList;

using schema.binary;


namespace jsystem.schema.j3dgraph.bmd.vtx1;

[BinarySchema]
public sealed partial class ArrayFormat : IBinaryConvertible {
  public GxVertexAttribute arrayType;
  public GxComponentCountType ComponentCountType { get; set; }
  public GxComponentType dataType;
  public byte decimalPoint;

  [Unknown]
  public byte unknown1;

  [Unknown]
  public ushort unknown2;
}