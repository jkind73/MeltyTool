using schema.binary;
using schema.binary.attributes;

namespace bar.schema;

public enum UvtsAnimationMode : byte {
  PlayOnce = 0,
  Loop = 1,
  Bounce = 2
}

/// <summary>
///   "Texture Sequence"
/// 
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/BeetleAdventureRacing/ParsedFiles/UVTS.ts
/// </summary>
[BinarySchema]
public sealed partial class Uvts : IBinaryConvertible {
  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool LoadUvtxesImmediately { get; set; }

  [SequenceLengthSource(SchemaIntegerType.BYTE)]
  public UvtsFrame[] Frames { get; private set; }

  public UvtsAnimationMode AnimationMode { get; set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool PlayAnimationInReverse { get; set; }
  public float UnitsPerSecond { get; set; }
}

[BinarySchema]
public sealed partial class UvtsFrame : IBinaryDeserializable {
  public ushort UvtxIndex { get; set; }
  public float FrameLengthUnits { get; set; }
}