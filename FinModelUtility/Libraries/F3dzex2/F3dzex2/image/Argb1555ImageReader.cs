using fin.color;
using fin.image;
using fin.image.formats;
using fin.image.io.pixel;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace f3dzex2.image;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/Common/N64/Image.ts#L82
/// </summary>
public sealed class Argb1555ImageReader(int width, int height, bool deinterleave)
    : fin.image.io.IImageReader<Rgba32Image> {
  public Rgba32Image ReadImage(IBinaryReader br) {
    var dstIdx = 0;
    var srcIdx = 0;

    var bytes = br.ReadUInt16s(width * height);

    var image = new Rgba32Image(PixelFormat.ARGB1555, width, height);

    using var fastLock = image.Lock();
    var scan0 = fastLock.Pixels;

    for (var y = 0; y < height; ++y) {
      var di = deinterleave ? ((y & 1) << 2) : 0;
      for (var x = 0; x < width; ++x) {
        var p = bytes[(srcIdx ^ di) / 2];
        ColorUtil.SplitArgb1555(p,
                                out var r,
                                out var g,
                                out var b,
                                out var a);
        scan0[dstIdx++] = new Rgba32(r, g, b, a);
        srcIdx += 2;
      }
    }

    return image;
  }
}