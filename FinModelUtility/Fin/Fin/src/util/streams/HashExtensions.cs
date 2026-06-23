using System;
using System.IO;
using System.IO.Hashing;

using schema.binary;

namespace fin.util.streams;

public static class HashExtensions {
  /// <summary>
  ///   (Straight-up copied from the implementation of Stream.CopyTo())
  ///   We pick a value that is the largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
  ///   The CopyTo/CopyToAsync buffer is short-lived and is likely to be collected at Gen0, and it offers a significant
  ///   improvement in Copy performance.
  /// </summary>
  private const int DEFAULT_COPY_BUFFER_SIZE_ = 81920;

  public static uint CalculateCrc32(this Stream src) {
    var originalPosition = src.Position;

    var crc = new Crc32();
    crc.Append(src);

    var hash = crc.GetCurrentHashAsUInt32();
    src.Position = originalPosition;

    return hash;
  }

  public static uint CalculateCrc32(this IBinaryReader src) {
    var originalPosition = src.Position;

    var crc = new Crc32();

    Span<byte> buffer = stackalloc byte[DEFAULT_COPY_BUFFER_SIZE_];
    int bytesRead;
    while ((bytesRead = src.TryToReadIntoBuffer(buffer)) != 0) {
      crc.Append(buffer.Slice(bytesRead));
    }

    var hash = crc.GetCurrentHashAsUInt32();
    src.Position = originalPosition;

    return hash;
  }
}