using System.Numerics;

using schema.binary;
using schema.binary.attributes;

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

  [SequenceLengthSource(0x10)]
  public byte[] Unk0 { get; set; }

  public byte GridWidth { get; set; }
  public byte GridHeight { get; set; }

  public Vector2 CellXy { get; set; }
  public float Unk1 { get; set; }

  [Skip]
  public int UvctCount => this.GridWidth * this.GridHeight;

  [RSequenceLengthSource(nameof(UvctCount))]
  public UvtrCell[] Cells { get; set; }
}

[BinarySchema]
public sealed partial class UvtrCell : IBinaryDeserializable {
  public byte Flag { get; set; }

  [Skip]
  public bool HasData => this.Flag != 0;

  [RIfBoolean(nameof(HasData))]
  public UvtrCellData? Data { get; set; }
}

[BinarySchema]
public sealed partial class UvtrCellData : IBinaryDeserializable {
  public Matrix4x4 Transform { get; set; }
  public sbyte Rotation { get; set; }
  public ushort UvctIndex { get; set; }
}