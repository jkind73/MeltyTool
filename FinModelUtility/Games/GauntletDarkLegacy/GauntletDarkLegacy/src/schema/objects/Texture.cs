using fin.image;

using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.objects;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/MosesofEgypt/gdl_tools/blob/main/gdl/defs/texdef.py#L23
/// </summary>
public enum ImageFormat : byte {
  RGBA_5551 = 0,
  IDX_4_RGBA_5551 = 16,
  IDX_4_RGBA_5553 = 18,

  //IDX_4_BGR_555 = 1,
  //IDX_4_ABGR_8888 = 18,
  //IDX_4_BGR_888 = 19,
  IDX_8_RGB_5551 = 48,
  IDX_8_RGBA_5553 = 50,
  //IDXA_88 = 40,
  //IDX_8_ABGR_8888 = 52,
  //IDX_8_BGR_888 = 51,
  //IDX_8_A_8 = 114,
  //IDX_8_I_8 = 115,
  //IDX_4_A_4 = 130,
  //IDX_4_I_4 = 131,
}

public static class ImageFormatExtensions {
  public static bool IsIndexed(this ImageFormat format,
                               out int paletteCount,
                               out PixelFormat pixelFormat) {
    switch (format) {
      case ImageFormat.IDX_4_RGBA_5551: {
        paletteCount = 16;
        pixelFormat = PixelFormat.RGBA5551;
        return true;
      }
      case ImageFormat.IDX_4_RGBA_5553: {
        paletteCount = 16;
        pixelFormat = PixelFormat.RGBA5553;
        return true;
      }
      case ImageFormat.IDX_8_RGB_5551: {
        paletteCount = 256;
        pixelFormat = PixelFormat.RGBA5551;
        return true;
      }
      case ImageFormat.IDX_8_RGBA_5553: {
        paletteCount = 256;
        pixelFormat = PixelFormat.RGBA5553;
        return true;
      }
      default: {
        paletteCount = default;
        pixelFormat = default;
        return false;
      }
    }
  }
}

public enum TextureFlags : ushort {
  HALF_RES = 1 << 0,
  CLAMP_U = 1 << 2,
  CLAMP_V = 1 << 3,
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
  public TextureFlags Flags { get; set; }
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