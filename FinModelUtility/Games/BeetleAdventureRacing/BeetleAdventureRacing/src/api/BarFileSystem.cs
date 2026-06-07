using fin.archives;
using fin.io;
using fin.schema.data;
using fin.util.streams;

using schema.binary;
using schema.binary.attributes;

namespace bar.api;

public sealed record BarRomFileBundle(IReadOnlyTreeFile MainFile)
    : ISimpleArchiveFileBundle<BarRomFileBundle> {
  public static BarRomFileBundle FromFile(IReadOnlyTreeFile file) => new(file);
}

public sealed class BarFileTableImporter
    : BSimpleArchiveImporter<BarRomFileBundle> {
  protected override void BuildHierarchyAndGetFileStream(
      BarRomFileBundle bundle,
      ISet<IReadOnlyGenericFile> fileSet,
      ISimpleArchiveDirectory builderRoot,
      out Stream baseStream,
      out Stream readStream) {
    baseStream = readStream = bundle.MainFile.OpenRead();

    var crc32 = readStream.CalculateCrc32();

    var fileSystemOffset = crc32 switch {
        0xf4a97c73 => 0x237d0,
        _ => throw new NotImplementedException(
            $"Unsupported Beetle Adventure Racing ROM crc32: {crc32:0X}")
    };

    readStream.Position = fileSystemOffset;

    var romBr = new SchemaBinaryReader(readStream);

    var uvft = new Uvft();
    var section
        = new PassThruStringMagicUInt32SizedSection<Uvft>("UVFT", uvft);
    section.Read(romBr);

    foreach (var uvftFileType in uvft.FileTypes) {
    }
  }
}

/// <summary>
///   "File Table"
/// </summary>
[BinarySchema]
public sealed partial class Uvft : IBinaryConvertible {
  private readonly string magic_ = "UVFT";

  [RSequenceUntilEndOfStream]
  public UvftFileType[] FileTypes { get; set; }
}

/// <summary>
///   "File Table"
/// </summary>
[BinarySchema]
public sealed partial class UvftFileType : IBinaryConvertible {
  [SequenceLengthSource(4)]
  public string Type { get; set; }

  public uint Count { get; set; }
}