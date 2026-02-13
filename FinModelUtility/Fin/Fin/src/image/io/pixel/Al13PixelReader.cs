using System;
using System.Runtime.CompilerServices;

using fin.image.formats;
using fin.math;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.io.pixel;

/// <summary>
///   Helper class for reading 4-bit luminance/alpha pixels.
/// </summary>
public sealed class Al13PixelReader : IPixelReader<La16> {
  public IImage<La16> CreateImage(int width, int height)
    => new La16Image(PixelFormat.AL13, width, height);

  public void Decode(ReadOnlySpan<byte> data, Span<La16> scan0, int offset) {
    var value = data[0];

    var upper = (byte) ((value >> 4) * 17);
    var lower = (byte) ((value & 0xF) * 17);

    scan0[offset + 0] = FromNibble_(upper);
    scan0[offset + 1] = FromNibble_(lower);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static La16 FromNibble_(byte nibble)
    => new(Expand3to8_((nibble >> 1) & 0x7),
           (byte) (nibble.GetBit(0) ? 255 : 0));

  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://github.com/magcius/noclip.website/blob/1d5ee0e1c4fd4f447456f0019b6d64dff2fc9bd1/src/Common/N64/Image.ts#L56C1-L58C2
  /// </summary>
  private static byte Expand3to8_(int n)
    => (byte) ((n << (8 - 3)) | (n << (8 - 6)) | (n >>> (9 - 8)));

  public int PixelsPerRead => 2;
  public int BitsPerPixel => 4;
}