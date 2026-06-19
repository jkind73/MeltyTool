using System;

namespace fin.image;

public static class ImageUtils {
  public static uint GetByteCount(uint width, uint height, uint bitsPerPixel)
    => GetByteCount(width * height, bitsPerPixel);

  public static uint GetByteCount(uint pixelCount, uint bitsPerPixel) {
    if (pixelCount == 0) {
      return 0;
    }

    return Math.Max(1, (uint) Math.Ceiling(pixelCount * bitsPerPixel / 8f));
  }
}