using fin.schema.data;

using schema.binary;
using schema.binary.attributes;

namespace nitro.schema.narc;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/nickworonekin/narchive/blob/master/src/Narchive/Formats/NarcArchive.cs
/// </summary>
[BinarySchema]
public sealed partial class Fimg : IBinaryConvertible {
  public AutoStringMagicUInt32SizedSection<FimgData> Data { get; }
    = new("FIMG");
}

[BinarySchema]
public sealed partial class FimgData : IBinaryConvertible {
  [RPositionRelativeToStream]
  public long Offset { get; private set; }
}