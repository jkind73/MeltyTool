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

    var fileTableOffset = crc32 switch {
        0xf4a97c73 => 0x237d0,
        _ => throw new NotImplementedException(
            $"Unsupported Beetle Adventure Racing ROM crc32: {crc32:0X}")
    };

    readStream.Position = fileTableOffset;

    var romBr = new SchemaBinaryReader(readStream, Endianness.BigEndian);

    var section = new AutoStringMagicUInt32SizedSection<Uvft>("FORM");
    section.Read(romBr);

    romBr.Align(0x10);
    var fileDataOffset = romBr.Position;

    var uvft = section.Data;
    foreach (var uvftFileType in uvft.FileTypes) {
      for (var i = 0; i < uvftFileType.Offsets.Length; ++i) {
        var entryOffset = uvftFileType.Offsets[i];

        uint currentFilePosition;
        uint fileLength;
        if (entryOffset == -1) {
          currentFilePosition = 0;
          fileLength = 0;
        } else {
          currentFilePosition = (uint) (fileDataOffset + entryOffset);
          fileLength = romBr.SubreadAt(
              currentFilePosition,
              () => {
                var str = romBr.ReadString(4);
                if (str == "FORM") {
                  return romBr.ReadUInt32();
                }

                return (uint) 0;
              });
        }

        builderRoot.AddFile(
            $"{i}.{uvftFileType.Type}",
            currentFilePosition,
            fileLength);
      }
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