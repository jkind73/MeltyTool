using System.Numerics;

using fin.schema;

using schema.binary;

namespace grezzo.schema.cmb.qtrs;

[BinarySchema]
public sealed partial class BoundingBox : IBinaryConvertible {
  // M-1 checked all files, and Min/Max are the only values to ever change
  [Unknown]
  public uint Unk0 { get; private set; }

  [Unknown]
  public uint Unk1 { get; private set; }

  public Vector3 Min { get; private set; }
  public Vector3 Max { get; private set; }

  [Unknown]
  public int Unk2 { get; private set; }

  [Unknown]
  public int Unk3 { get; private set; }

  [Unknown]
  public uint Unk4 { get; private set; }
}