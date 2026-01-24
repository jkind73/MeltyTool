using fin.io;
using fin.math;
using fin.schema;
using fin.util.enums;

using schema.binary;
using schema.util.streams;

namespace pc;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/gamemasterplc/pokemon_fsys_tool/blob/main/pokemon_fsys_tool.cpp
/// </summary>
public sealed partial class FsysExtractor {
  public static readonly Dictionary<uint, string> FILE_TYPE_BY_ID = new() {
      { 0, "bin" },
      { 1, "rdat" },
      { 2, "dat" },
      { 3, "ccd" },
      { 4, "samp" },
      { 5, "msg" },
      { 6, "fnt" },
      { 7, "scd" },
      { 9, "gtx" },
      { 10, "gpt1" },
      { 12, "cam" },
      { 14, "rel" },
      { 15, "pkx" },
      { 16, "wzx" },
      { 17, "gfl" },
      { 18, "gpt1" },
      { 19, "rel" },
      { 20, "isf" },
      { 21, "isfd" },
      { 22, "thp" },
      { 23, "thpd" },
      { 24, "gsw" },
      { 25, "atx" },
      { 26, "bin" },
  };

  public static bool TryToExtractFilesFrom(ISystemFile file) {
    var directory = file.AssertGetParent()
                        .GetOrCreateSubdir(file.NameWithoutExtension);
    if (!directory.IsEmpty) {
      return false;
    }

    using var br = file.OpenReadAsBinary(Endianness.BigEndian);
    var header = br.ReadNew<FsysHeader>();

    var fsysEnableOverride
        = header.Flags.CheckFlag(FsysFlags.FSYS_ENABLE_OVERRIDE);

    br.Position = header.OffsetTableOffset;
    var offsetTable = br.ReadNew<OffsetTable>();

    var fsysFiles = new FsysFile[header.FileCount];
    var fsysFileNames = new string[header.FileCount];

    for (var i = 0; i < header.FileCount; ++i) {
      br.Position = offsetTable.FileListOffset + 4 * i;

      var fileOffset = br.ReadUInt32();

      br.Position = fileOffset;
      var fsysFile = br.ReadNew<FsysFile>();
      fsysFiles[i] = fsysFile;

      br.Position = fsysFile.NameOffset;
      var fileName = br.ReadStringNT();
      fsysFileNames[i] = $"{fileName}.{FILE_TYPE_BY_ID[fsysFile.Type]}";
    }

    foreach (var (fsysFile, fileName) in fsysFiles.Zip(fsysFileNames)) {
      var outputFile = new FinFile(Path.Join(directory.FullPath, fileName));

      using var fs = outputFile.OpenWrite();
      br.Position = fsysFile.Offset;

      if (fsysFile.Flags.CheckFlag(FsysFileFlags.COMPRESSED)) {
        br.Subread(fsysFile.Size,
                   () => {
                     var decompressedData = DecodeLzss_(br);
                     fs.Write(decompressedData);
                   });
      } else {
        br.Subread(fsysFile.Size, () => br.CopyTo(fs));
      }
    }

    return true;
  }

  [Flags]
  public enum FsysFlags : uint {
    FSYS_ENABLE_OVERRIDE = 1 << 0,
  }

  [BinarySchema]
  private sealed partial class FsysHeader : IBinaryDeserializable {
    private readonly string magic_ = "FSYS";

    public uint Version { get; set; }
    public uint ArchiveId { get; set; }
    public uint FileCount { get; set; }
    public FsysFlags Flags { get; set; }

    [Unknown]
    public uint Unk { get; set; }

    public uint OffsetTableOffset { get; set; }
    public uint DataStartOffset { get; set; }
    public uint FsysSize { get; set; }
  }

  [BinarySchema]
  private sealed partial class OffsetTable : IBinaryDeserializable {
    public uint FileListOffset { get; set; }
    public uint StringOffset { get; set; }
    public uint DataOffset { get; set; }
  }

  public enum FsysFileFlags : uint {
    COMPRESSED = 0x80000000,
  }

  [BinarySchema]
  private sealed partial class FsysFile : IBinaryDeserializable {
    public uint Id { get; set; }
    public uint Offset { get; set; }
    public uint Size { get; set; }
    public FsysFileFlags Flags { get; set; }
    public uint Unk0 { get; set; }
    public uint CompressedSize { get; set; }
    public uint Unk1 { get; set; }
    public uint FilenameOffset { get; set; }
    public uint Type { get; set; }
    public uint NameOffset { get; set; }
  }

  private static byte[] DecodeLzss_(IBinaryReader br) {
    br.AssertString("LZSS");
    var outSize = br.ReadUInt32();
    var inSize = br.ReadUInt32();
    br.Position += 4;

    var dstPos = 0;
    var dst = new byte[outSize];

    uint flag = 0;

    var n = 4096;
    var f = 18;
    var threshold = 2;

    var textBuf = new byte[n + f - 1];
    var textBufPos = n - f;

    while (dstPos < outSize) {
      if ((flag & 0x100) == 0) {
        var value = br.ReadByte();
        flag = (uint) (0xFF00 | value);
      }

      if (flag.GetBit(0)) {
        var value = br.ReadByte();
        textBuf[textBufPos] = dst[dstPos++] = value;
        textBufPos = (textBufPos + 1) % n;
      } else {
        var byte1 = br.ReadByte();
        var byte2 = br.ReadByte();
        var ofs = ((byte2 & 0xF0) << 4) | byte1;
        var copySize = (byte2 & 0xF) + threshold + 1;
        for (var i = 0; i < copySize; i++) {
          dst[dstPos++] = textBuf[textBufPos] = textBuf[ofs];
          ofs = (ofs + 1) % n;
          textBufPos = (textBufPos + 1) % n;
        }
      }

      flag >>= 1;
    }

    return dst;
  }
}