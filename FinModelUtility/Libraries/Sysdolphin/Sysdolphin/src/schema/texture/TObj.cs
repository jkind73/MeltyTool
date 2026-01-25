using System.Numerics;

using fin.image;
using fin.image.formats;
using fin.color;

using gx;
using gx.image;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace sysdolphin.schema.texture;

[Flags]
public enum ObjFlags {
  COORD_UV = 0 << 0,
  COORD_REFLECTION = 1 << 0,
  COORD_HILIGHT = 2 << 0,
  COORD_SHADOW = 3 << 0,
  COORD_TOON = 4 << 0,
  COORD_GRADATION = 5 << 0,
  LIGHTMAP_DIFFUSE = 1 << 4,
  LIGHTMAP_SPECULAR = 1 << 5,
  LIGHTMAP_AMBIENT = 1 << 6,
  LIGHTMAP_EXT = 1 << 7,
  LIGHTMAP_SHADOW = 1 << 8,

  //COLORMAP_NONE = (0 << 16),
  COLORMAP_ALPHA_MASK = 1 << 16,
  COLORMAP_RGB_MASK = 2 << 16,
  COLORMAP_BLEND = 3 << 16,
  COLORMAP_MODULATE = 4 << 16,
  COLORMAP_REPLACE = 5 << 16,
  COLORMAP_PASS = 6 << 16,
  COLORMAP_ADD = 7 << 16,
  COLORMAP_SUB = 8 << 16,

  //ALPHAMAP_NONE = (0 << 20),
  ALPHAMAP_ALPHA_MASK = 1 << 20,
  ALPHAMAP_BLEND = 2 << 20,
  ALPHAMAP_MODULATE = 3 << 20,
  ALPHAMAP_REPLACE = 4 << 20,
  ALPHAMAP_PASS = 5 << 20,
  ALPHAMAP_ADD = 6 << 20,
  ALPHAMAP_SUB = 7 << 20,
  BUMP = 1 << 24,
  MTX_DIRTY = 1 << 31
}

public enum Coord {
  UV = ObjFlags.COORD_UV,
  REFLECTION = ObjFlags.COORD_REFLECTION,
  HILIGHT = ObjFlags.COORD_HILIGHT,
  SHADOW = ObjFlags.COORD_SHADOW,
  TOON = ObjFlags.COORD_TOON,
  GRADATION = ObjFlags.COORD_GRADATION,
}

public enum ColorMap {
  NONE = 0,
  ALPHA_MASK = ObjFlags.COLORMAP_ALPHA_MASK,
  RGB_MASK = ObjFlags.COLORMAP_RGB_MASK,
  BLEND = ObjFlags.COLORMAP_BLEND,
  MODULATE = ObjFlags.COLORMAP_MODULATE,
  REPLACE = ObjFlags.COLORMAP_REPLACE,
  PASS = ObjFlags.COLORMAP_PASS,
  ADD = ObjFlags.COLORMAP_ADD,
  SUB = ObjFlags.COLORMAP_SUB,
}

public enum AlphaMap {
  NONE = 0,
  ALPHA_MASK = ObjFlags.ALPHAMAP_ALPHA_MASK,
  BLEND = ObjFlags.ALPHAMAP_BLEND,
  MODULATE = ObjFlags.ALPHAMAP_MODULATE,
  REPLACE = ObjFlags.ALPHAMAP_REPLACE,
  PASS = ObjFlags.ALPHAMAP_PASS,
  ADD = ObjFlags.ALPHAMAP_ADD,
  SUB = ObjFlags.ALPHAMAP_SUB,
}

public static class ObjFlagsExtensions {
  public static Coord GetCoord(this ObjFlags flags) {
    var mask = 7 << 0;
    var maskedFlags = (ObjFlags) ((int) flags & mask);
    return maskedFlags switch {
        ObjFlags.COORD_UV         => Coord.UV,
        ObjFlags.COORD_REFLECTION => Coord.REFLECTION,
        ObjFlags.COORD_HILIGHT    => Coord.HILIGHT,
        ObjFlags.COORD_SHADOW     => Coord.SHADOW,
        ObjFlags.COORD_TOON       => Coord.TOON,
        ObjFlags.COORD_GRADATION  => Coord.GRADATION,
        _                          => throw new ArgumentOutOfRangeException()
    };
  }

  public static ColorMap GetColorMap(this ObjFlags flags) {
    var mask = 15 << 16;
    var maskedFlags = (ObjFlags) ((int) flags & mask);
    return maskedFlags switch {
        ObjFlags.COLORMAP_ALPHA_MASK => ColorMap.ALPHA_MASK,
        ObjFlags.COLORMAP_RGB_MASK   => ColorMap.RGB_MASK,
        ObjFlags.COLORMAP_BLEND      => ColorMap.BLEND,
        ObjFlags.COLORMAP_MODULATE   => ColorMap.MODULATE,
        ObjFlags.COLORMAP_REPLACE    => ColorMap.REPLACE,
        ObjFlags.COLORMAP_PASS       => ColorMap.PASS,
        ObjFlags.COLORMAP_ADD        => ColorMap.ADD,
        ObjFlags.COLORMAP_SUB        => ColorMap.SUB,
        0                             => ColorMap.NONE,
        _                             => throw new ArgumentOutOfRangeException()
    };
  }

  public static AlphaMap GetAlphaMap(this ObjFlags flags) {
    var mask = 7 << 20;
    var maskedFlags = (ObjFlags) ((int) flags & mask);
    return maskedFlags switch {
        ObjFlags.ALPHAMAP_ALPHA_MASK => AlphaMap.ALPHA_MASK,
        ObjFlags.ALPHAMAP_BLEND      => AlphaMap.BLEND,
        ObjFlags.ALPHAMAP_MODULATE   => AlphaMap.MODULATE,
        ObjFlags.ALPHAMAP_REPLACE    => AlphaMap.REPLACE,
        ObjFlags.ALPHAMAP_PASS       => AlphaMap.PASS,
        ObjFlags.ALPHAMAP_ADD        => AlphaMap.ADD,
        ObjFlags.ALPHAMAP_SUB        => AlphaMap.SUB,
        0                             => AlphaMap.NONE,
        _                             => throw new ArgumentOutOfRangeException()
    };
  }
}

/// <summary>
///   Texture object.
///
///   Shamelessly stolen from:
///    - https://github.com/jam1garner/Smash-Forge/blob/c0075bca364366bbea2d3803f5aeae45a4168640/Smash%20Forge/Filetypes/Melee/DAT.cs#L1281
///    - https://github.com/jam1garner/Smash-Forge/blob/c0075bca364366bbea2d3803f5aeae45a4168640/Smash%20Forge/Filetypes/Melee/LibWii/TLP.cs#L166
///    - https://github.com/Ploaj/HSDLib/blob/93a906444f34951c6eed4d8c6172bba43d4ada98/HSDRaw/Common/HSD_TOBJ.cs#L92
/// </summary>
public sealed class Obj : IBinaryDeserializable {
  public uint StringOffset { get; private set; }
  public string? Name { get; set; }

  public uint NextTObjOffset { get; private set; }
  public Obj? NextTObj { get; private set; }

  public GxTexGenSrc TexGenSrc { get; private set; }

  public Vector3 RotationRadians { get; private set; }
  public Vector3 Scale { get; private set; }
  public Vector3 Translation { get; private set; }

  public GxWrapMode WrapS { get; private set; }
  public GxWrapMode WrapT { get; private set; }

  public byte RepeatS { get; private set; }
  public byte RepeatT { get; private set; }

  public ObjFlags Flags { get; private set; }

  public float Blending { get; private set; }

  public GxMagTextureFilter MagFilter { get; private set; }
  public ObjLod? Lod { get; private set; }

  public IImage Image { get; private set; }

  public void Read(IBinaryReader br) {
    var offset = br.Position;

    if (br.Position == 24148) {
      ;
    }

    this.StringOffset = br.ReadUInt32();
    this.NextTObjOffset = br.ReadUInt32();

    br.Position += 4;

    this.TexGenSrc = (GxTexGenSrc) br.ReadUInt32();

    this.RotationRadians = br.ReadVector3();
    this.Scale = br.ReadVector3();
    this.Translation = br.ReadVector3();

    this.WrapS = (GxWrapMode) br.ReadUInt32();
    this.WrapT = (GxWrapMode) br.ReadUInt32();

    this.RepeatS = br.ReadByte();
    this.RepeatT = br.ReadByte();

    br.Position += 2;

    this.Flags = (ObjFlags) br.ReadUInt32();

    this.Blending = br.ReadSingle();

    this.MagFilter = (GxMagTextureFilter) br.ReadInt32();

    var imageOffset = br.ReadUInt32();
    var paletteOffset = br.ReadUInt32();
    var lodOffset = br.ReadUInt32();

    br.Position = imageOffset;
    var imageDataOffset = br.ReadUInt32();
    var width = br.ReadUInt16();
    var height = br.ReadUInt16();
    var format = (GxTextureFormat) br.ReadUInt32();

    br.Position = paletteOffset;
    var paletteDataOffset = br.ReadUInt32();
    var paletteFormat = (GxPaletteFormat) br.ReadUInt32();
    var unk1 = br.ReadUInt32();
    var paletteEntryCount = br.ReadUInt16();
    var unk2 = br.ReadUInt16();

    if (lodOffset != 0) {
      br.Position = lodOffset;
      this.Lod = br.ReadNew<ObjLod>();
    }

    // TODO: Add support for indexed textures
    var isIndex4 = format == GxTextureFormat.INDEX4;
    var isIndex8 = format == GxTextureFormat.INDEX8;
    if (!isIndex4 && !isIndex8) {
      br.Position = imageDataOffset;
      this.Image = new GxImageReader(width, height, format).ReadImage(br);
    } else {
      var palette = new Rgba32[paletteEntryCount];
      br.Position = paletteDataOffset;
      for (var i = 0; i < paletteEntryCount; ++i) {
        switch (paletteFormat) {
          case GxPaletteFormat.PAL_A8_I8: {
            var alpha = br.ReadByte();
            var intensity = br.ReadByte();
            palette[i] =
                new Rgba32(intensity, intensity, intensity, alpha);
            break;
          }
          case GxPaletteFormat.PAL_R5_G6_B5: {
            // Curiously, the colors are flipped here.
            ColorUtil.SplitRgb565(br.ReadUInt16(),
                                  out var r,
                                  out var g,
                                  out var b);
            palette[i] = new Rgba32(r, g, b);
            break;
          }
          // TODO: There seems to be a bug reading the palette, these colors look weird
          case GxPaletteFormat.PAL_A3_RGB5: {
            ColorUtil.SplitRgb5A3(br.ReadUInt16(),
                                  out var r,
                                  out var g,
                                  out var b,
                                  out var a);
            palette[i] = new Rgba32(r, g, b, a);
            break;
          }
          default:
            throw new ArgumentOutOfRangeException();
        }
      }

      var bitmap = new Rgba32Image(
          isIndex4 ? PixelFormat.P4 : PixelFormat.P8,
          width,
          height);
      this.Image = bitmap;

      using var imageLock = bitmap.Lock();
      var ptr = imageLock.Pixels;

      br.Position = imageDataOffset;

      var blockWidth = 8;
      var blockHeight = isIndex4 ? 8 : 4;

      var tileCountX = (int) Math.Ceiling(1f * height / blockHeight);
      var tileCountY = (int) Math.Ceiling(1f * width / blockWidth);

      var paddedWidth = blockWidth * tileCountX;
      var paddedHeight = blockHeight * tileCountY;

      var indexCount = paddedWidth * paddedHeight;
      var dataLength = indexCount;
      if (isIndex4) {
        dataLength >>= 1;
      }

      var data = br.ReadBytes(dataLength);
      byte[] indices;
      if (isIndex4) {
        indices = new byte[indexCount];
        for (var i = 0; i < data.Length; ++i) {
          var two = data[i];

          var firstIndex = two >> 4;
          var secondIndex = two & 0x0F;

          indices[2 * i + 0] = (byte) firstIndex;
          indices[2 * i + 1] = (byte) secondIndex;
        }
      } else {
        indices = data;
      }

      var index = 0;
      for (var ty = 0; ty < tileCountX; ty++) {
        for (var tx = 0; tx < tileCountY; tx++) {
          for (var y = 0; y < blockHeight; ++y) {
            for (var x = 0; x < blockWidth; ++x) {
              var px = tx * blockWidth + x;
              var py = ty * blockHeight + y;

              if (px < width && py < height) {
                ptr[py * width + px] = palette[indices[index]];
              }

              ++index;
            }
          }
        }
      }
    }

    if (this.NextTObjOffset != 0) {
      br.Position = this.NextTObjOffset;
      this.NextTObj = br.ReadNew<Obj>();
    }
  }
}