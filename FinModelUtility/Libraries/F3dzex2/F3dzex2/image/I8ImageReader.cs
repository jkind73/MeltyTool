using fin.image;
using fin.image.formats;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace f3dzex2.image;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/Common/N64/Image.ts#L241
/// </summary>
public sealed class I8ImageReader(int width, int height, bool deinterleave)
    : fin.image.io.IImageReader<I8Image> {
  public I8Image ReadImage(IBinaryReader br) {
    var dstIdx = 0;
    var srcIdx = 0;

    if (width <= 2 && height <= 2) {
      deinterleave = false;
    }

    var bytes = br.ReadBytes(width * height);

    var image = new I8Image(PixelFormat.I8, width, height);

    using var fastLock = image.Lock();
    var scan0 = fastLock.Pixels;

    for (var y = 0; y < height; ++y) {
      var di = deinterleave ? ((y & 1) << 2) : 0;
      for (var x = 0; x < width; ++x) {
        var b = bytes[srcIdx ^ di];
        
        scan0[dstIdx++] = new La16(b, b);
        
        ++srcIdx;
      }
    }

    return image;
  }
}