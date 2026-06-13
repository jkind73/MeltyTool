using fin.image;
using fin.image.formats;
using fin.image.io.pixel;
using fin.math;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace f3dzex2.image;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/Common/N64/Image.ts#L216
/// </summary>
public sealed class I4ImageReader(int width, int height, bool deinterleave)
    : fin.image.io.IImageReader<I8Image> {
  public I8Image ReadImage(IBinaryReader br) {
    var dstIdx = 0;
    var srcIdx = 0;

    var bytes = br.ReadBytes(width * height / 2);

    var image = new I8Image(PixelFormat.I4, width, height);

    using var fastLock = image.Lock();
    var scan0 = fastLock.Pixels;

    for (var y = 0; y < height; ++y) {
      var di = deinterleave ? ((y & 1) << 2) : 0;
      for (var x = 0; x < width / 2; ++x) {
        var b = bytes[srcIdx ^ di];

        var upper = BitLogic.Expand4To8((b >> 4) & 0xF);
        var lower = BitLogic.Expand4To8(b & 0xF);

        scan0[dstIdx++] = new La16(upper, upper);
        scan0[dstIdx++] = new La16(lower, lower);

        ++srcIdx;
      }
    }

    return image;
  }
}