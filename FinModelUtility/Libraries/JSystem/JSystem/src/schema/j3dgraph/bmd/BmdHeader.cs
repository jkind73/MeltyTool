using schema.binary;
using schema.binary.attributes;


namespace jsystem.schema.j3dgraph.bmd;

[BinarySchema]
public sealed partial class BmdHeader : IBinaryConvertible {
  private readonly string magic_ = "J3D2bmd3";

  [WSizeOfStreamInBytes]
  public uint fileSize;

  public uint nrSections;
  public readonly byte[] padding = new byte[16];
}