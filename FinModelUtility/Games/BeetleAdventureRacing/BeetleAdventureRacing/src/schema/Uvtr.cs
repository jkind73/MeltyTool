using System.Numerics;

using schema.binary;

namespace bar.schema;

/// <summary>
///   "TeRrain"
/// 
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/BeetleAdventureRacing/ParsedFiles/UVTR.ts
/// </summary>
[BinarySchema]
public sealed partial class Uvtr : IBinaryDeserializable {
  public Vector2 MinXy { get; set; }
}