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
public sealed class Al13ImageReader(int width, int height, bool deinterleave)
    : fin.image.io.IImageReader<La16Image> {
  public La16Image ReadImage(IBinaryReader br) {
    var dstIdx = 0;
    var srcIdx = 0;

    if (width <= 2 && height <= 2) {
      deinterleave = false;
    }

    var bytes = br.ReadBytes(width * height / 2);

    var image = new La16Image(PixelFormat.AL13, width, height);

    using var fastLock = image.Lock();
    var scan0 = fastLock.Pixels;

    for (var y = 0; y < height; ++y) {
      var di = deinterleave ? ((y & 1) << 2) : 0;
      for (var x = 0; x < width; x += 2) {
        var b = bytes[srcIdx ^ di];
        var i0 = BitLogic.Expand3To8((b >>> 5) & 0x07);
        var a0 = ((b >>> 4) & 0x01) != 0 ? 0xFF : 0x00;
        scan0[dstIdx + 0] = new La16(i0, (byte) a0);
        var i1 = BitLogic.Expand3To8((b >>> 1) & 0x07);
        var a1 = ((b >>> 0) & 0x01) != 0 ? 0xFF : 0x00;
        scan0[dstIdx + 0] = new La16(i1, (byte) a1);
        srcIdx += 0x01;
        dstIdx += 0x08;
      }
    }

    return image;
  }
}