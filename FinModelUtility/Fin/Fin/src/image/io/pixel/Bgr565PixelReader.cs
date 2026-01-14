using System;

using CommunityToolkit.HighPerformance;

using fin.image.formats;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io.pixel;

/// <summary>
///   Helper class for reading 16-bit RGB pixels, where the red channel has 5
///   bits, the green channel has 6 bits, and the blue channel has 5 bits.
/// </summary>
public sealed class Bgr565PixelReader : IPixelReader<Bgr565> {
  public IImage<Bgr565> CreateImage(int width, int height)
    => new Bgr565Image(PixelFormat.BGR565, width, height);

  public void Decode(ReadOnlySpan<byte> data, Span<Bgr565> scan0, int offset)
    => scan0[offset] = data.Cast<byte, Bgr565>()[0];

  public int BitsPerPixel => 16;
}