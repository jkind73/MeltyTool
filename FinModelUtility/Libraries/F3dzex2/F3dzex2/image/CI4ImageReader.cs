using fin.image;
using fin.image.formats;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace f3dzex2.image;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/Common/N64/Image.ts#L115
/// </summary>
public sealed class CI4ImageReader(int width, int height, bool deinterleave)
    : fin.image.io.IImageReader<L8Image> {
  public L8Image ReadImage(IBinaryReader br) {
    var dstIdx = 0;
    var srcIdx = 0;

    var bytes = br.ReadBytes(width * height / 2);

    var image = new L8Image(PixelFormat.P4, width, height);

    using var fastLock = image.Lock();
    var scan0 = fastLock.Pixels;

    for (var y = 0; y < height; ++y) {
      var di = deinterleave ? ((y & 1) << 2) : 0;
      for (var x = 0; x < width / 2; ++x) {
        var b = bytes[srcIdx ^ di];

        scan0[dstIdx++] = new L8((byte) ((b >>> 4) & 0xF));
        scan0[dstIdx++] = new L8((byte) (b & 0xF));

        ++srcIdx;
      }
    }

    return image;
  }
}