using fin.schema.data;

using schema.binary;
using schema.binary.attributes;

namespace bar.schema;

/// <summary>
///   "File Table"
/// </summary>
[BinarySchema]
public sealed partial class Uvft : IBinaryConvertible {
  private readonly string magic_ = "UVFT";

  [RSequenceUntilEndOfStream]
  public UvtfFileType[] FileTypes { get; set; }
}

/// <summary>
///   "File Table"
/// </summary>
[BinarySchema]
public sealed partial class UvtfFileType : IBinaryConvertible {
  private readonly AutoThruUnknownStringMagicUInt32SizedSection<UvtfOffsets>
      impl_ = new();

  [Skip]
  public string Type => this.impl_.Magic;

  [Skip]
  public int[] Offsets => this.impl_.Data.Offsets;
}

/// <summary>
///   "File Table"
/// </summary>
[BinarySchema]
public sealed partial class UvtfOffsets : IBinaryConvertible {
  [RSequenceUntilEndOfStream]
  public int[] Offsets { get; set; }
}