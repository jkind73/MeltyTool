using System.Numerics;

using schema.binary;
using schema.binary.attributes;

namespace bar.schema;

/// <summary>
///   Texture
/// 
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/BeetleAdventureRacing/ParsedFiles/UVTX.ts
/// </summary>
[BinarySchema]
public sealed partial class Uvtx : IBinaryDeserializable {
  public ushort TexelDataSize { get; set; }
  public ushort DlCommandCount { get; set; }

  public Vector2 TexScrollAnim0Velocity { get; }
  public Vector2 TexScrollAnim1Velocity { get; }
  public Vector2 TexScrollAnim1Offset { get; }

  [RSequenceLengthSource(nameof(TexelDataSize))]
  public byte[] TexelData { get; private set; }

  [RSequenceLengthSource(nameof(DlCommandCount))]
  public byte[] DlCommands { get; private set; }
}