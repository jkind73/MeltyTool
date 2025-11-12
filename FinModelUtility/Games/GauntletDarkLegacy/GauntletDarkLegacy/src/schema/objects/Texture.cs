using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.objects;

public enum ImageFormat : byte {
  ABGR_1555 = 0,
  BGR_555 = 1,
  ABGR_8888 = 2,
  BGR_888 = 3,
  IDX_4_ABGR_1555 = 16,
  IDX_4_BGR_555 = 17,
  IDX_4_ABGR_8888 = 34,
  IDX_4_BGR_888 = 35,
  IDX_8_ABGR_1555 = 48,
  IDX_8_BGR_555 = 49,
  IDXA_88 = 56,
  IDX_8_ABGR_8888 = 66,
  IDX_8_BGR_888 = 67,
  IDX_8_A_8 = 130,
  IDX_8_I_8 = 131,
  IDX_4_A_4 = 146,
  IDX_4_I_4 = 147,
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/haekb/io_scene_gdl/blob/master/src/model.py#L374
/// </summary>
[BinarySchema]
public sealed partial class Texture : IBinaryDeserializable {
  public ImageFormat Format { get; set; }
  public byte Lodk { get; set; }
  public byte Mipmaps { get; set; }
  public byte Width64 { get; set; }
  public ushort WidthLog2 { get; set; }
  public ushort HeightLog2 { get; set; }
  public ushort Flags { get; set; }
  public ushort TexturePaletteIndex { get; set; }
  public uint TextureDataPointer { get; set; }
  public ushort TexturePaletteCount { get; set; }
  public ushort TextureShiftIndex { get; set; }
  public ushort FrameCount { get; set; }
  public ushort Width { get; set; }
  public ushort Height { get; set; }
  public ushort Size { get; set; }
  public uint TextureDefinitionPointer { get; set; }

  [SequenceLengthSource(8)]
  public uint[] Unk0 { get; set; }
}