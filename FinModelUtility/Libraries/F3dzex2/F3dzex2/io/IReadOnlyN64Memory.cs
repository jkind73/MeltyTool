using System.Collections.Generic;

using fin.compression;
using fin.util.types;

using schema.binary;

namespace f3dzex2.io;

public interface IReadOnlyN64Memory {
  Endianness Endianness { get; }

  SchemaBinaryReader OpenAtSegmentedAddress(uint segmentedAddress);

  IEnumerable<SchemaBinaryReader> OpenPossibilitiesAtSegmentedAddress(
      uint segmentedAddress);

  bool TryToOpenPossibilitiesAtSegmentedAddress(
      uint segmentedAddress,
      out IEnumerable<SchemaBinaryReader> possibilities);

  SchemaBinaryReader OpenSegment(uint segmentIndex);
  SchemaBinaryReader OpenSegment(ISegment segment, uint? offset = null);

  IEnumerable<SchemaBinaryReader> OpenPossibilitiesForSegment(
      uint segmentIndex);

  ISegment GetSegment(uint segmentIndex);
  bool IsValidSegment(uint segmentIndex);
  bool IsValidSegmentedAddress(uint segmentedAddress);
  bool IsSegmentCompressed(uint segmentIndex);
}

public interface IN64Memory : IReadOnlyN64Memory {
  void AddSegment(uint segmentIndex, ISegment segment);
  void SetSegment(uint segmentIndex, ISegment segment);
}

public interface ISeparateN64Memory : IN64Memory {
  void AddSegment(uint segmentIndex, uint offset, byte[] bytes);
  void SetSegment(uint segmentIndex, uint offset, byte[] bytes);
}

public interface ISlicedN64Memory : ISeparateN64Memory {
  void AddSegment(uint segmentIndex,
                  uint offset,
                  uint length,
                  IArrayToArrayDecompressor? decompressor = null);

  void SetSegment(uint segmentIndex,
                  uint offset,
                  uint length,
                  IArrayToArrayDecompressor? decompressor = null);
}

[UnionCandidate]
public interface ISegment {
  uint Offset { get; }
  uint Length { get; }
}

public class SliceSegment : ISegment {
  public required uint Offset { get; init; }
  public required uint Length { get; init; }
  public IArrayToArrayDecompressor? Decompressor { get; init; }
}

public class BytesSegment : ISegment {
  public required uint Offset { get; init; }
  public uint Length => (uint) this.Bytes.Length;
  public required byte[] Bytes { get; init; }
}