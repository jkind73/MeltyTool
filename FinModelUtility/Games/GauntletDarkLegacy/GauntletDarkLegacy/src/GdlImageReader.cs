using System.Drawing;

using fin.color;
using fin.image;
using fin.image.formats;
using fin.image.io;
using fin.image.io.pixel;

using gdl.schema.objects;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace gdl;

public sealed class GdlImageReader(Texture gdlTexture) : IImageReader {
  public IImage ReadImage(byte[] data, Endianness endianness) {
    using var br = new SchemaBinaryReader(data, endianness);
    return this.ReadImage(br);
  }

  public IImage ReadImage(IBinaryReader br) {
    IImage? finImage = null;
    if (!gdlTexture.Format.IsIndexed(out var paletteCount,
                                     out var pixelFormat)) {
      switch (gdlTexture.Format) {
        case ImageFormat.RGBA_5551: {
          finImage = TiledImageReader
                     .New(gdlTexture.Width,
                          gdlTexture.Height,
                          4,
                          4,
                          new Rgba5551PixelReader())
                     .ReadImage(br);
          break;
        }
        case ImageFormat.L4: {
          finImage = TiledImageReader
                     .New(gdlTexture.Width,
                          gdlTexture.Height,
                          8,
                          8,
                          new L4PixelReader())
                     .ReadImage(br);
          break;
        }
      }
    } else {
      IColor[]? palette = null;
      IImage<L8>? indexedImage = null;
      switch (pixelFormat) {
        case PixelFormat.RGBA5551: {
          palette = br.ReadUInt16s(paletteCount)
                      .Select(v => {
                        ColorUtil.SplitRgb5A1(v,
                                              out var r,
                                              out var g,
                                              out var b,
                                              out var a);
                        return FinColor.FromRgbaBytes(
                            r,
                            g,
                            b,
                            a);
                      })
                      .ToArray();
          break;
        }
        case PixelFormat.RGBA5553: {
          palette = br.ReadUInt16s(paletteCount)
                      .Select(v => {
                        ColorUtil.SplitRgb5A3(v,
                                              out var r,
                                              out var g,
                                              out var b,
                                              out var a);
                        return FinColor.FromRgbaBytes(
                            r,
                            g,
                            b,
                            a);
                      })
                      .ToArray();
          break;
        }
      }

      switch (paletteCount) {
        case 16: {
          indexedImage = TiledImageReader.New(
                                             gdlTexture.Width,
                                             gdlTexture.Height,
                                             8,
                                             8,
                                             new P4PixelReader())
                                         .ReadImage(br);
          break;
        }
        case 256: {
          indexedImage = TiledImageReader.New(
                                             gdlTexture.Width,
                                             gdlTexture.Height,
                                             8,
                                             4,
                                             new L8PixelReader())
                                         .ReadImage(br);
          break;
        }
      }

      if (palette != null && indexedImage != null) {
        finImage = new IndexedImage8(pixelFormat, indexedImage, palette);
      }
    }

    return finImage ??
           FinImage.CreateFromColor(
               Color.Magenta,
               gdlTexture.Width,
               gdlTexture.Height);
  }
}