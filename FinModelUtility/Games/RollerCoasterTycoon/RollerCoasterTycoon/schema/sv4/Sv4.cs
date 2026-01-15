using schema.binary;

namespace rct.schema.sv4;

/// <summary>
///   Shamelessly stolen from:
///   https://freerct.github.io/RCTTechDepot-Archive/SV4.html
/// </summary>
[BinarySchema]
public sealed partial class Sv4 : IBinaryConvertible {
  public GameTime GameTime { get; } = new();

  public uint RandomNumber0 { get; }
  public uint RandomNumber1 { get; }

  public Map Map { get; } = new();
}