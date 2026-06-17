using fin.math;

namespace fin.image.io.tile;

/// <summary>
///   Basic pixels indexer, where it assumes the pixels are laid out row-wise.
/// </summary>
public sealed class DeinterleavedPixelIndexer(int width, int bitsPerPixel)
    : IPixelIndexer {
  private readonly int width_ = width;
  private readonly int line_ = width.CeilToNearest(16);

  public void GetPixelCoordinates(int index, out int x, out int y) {
    y = index / this.line_;
    x = index % this.line_;

    if ((y & 1) == 1) {
      index = index * bitsPerPixel / 8;
      index ^= 4;
      index = index * 8 / bitsPerPixel;


      y = index / this.line_;
      x = index % this.line_;
    }
  }

  private int GetInterleavedCoordinate_(int x) {
    var inGroup = x / 4;
    var elementI = x % 4;

    var groupsPerLine = this.line_ / 4;

    var outGroup = groupsPerLine - 1 - inGroup;
    var outI = outGroup * 4 + elementI;

    return outI;
  }
}