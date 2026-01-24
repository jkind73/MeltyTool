using System;
using System.Linq;

namespace HaloWarsTools;

public abstract class HwBinaryResource : HwResource {
  protected HwBinaryResourceChunk[] Chunks { get; private set; }

  protected override void Load(byte[] bytes) {
    uint headerSize = BinaryUtils.ReadUInt32BigEndian(bytes, 4);
    ushort numChunks = BinaryUtils.ReadUInt16BigEndian(bytes, 16);
    int chunkHeaderSize = 24;

    var chunks = new HwBinaryResourceChunk[numChunks];
    for (int i = 0; i < chunks.Length; i++) {
      int offset = (int) headerSize + i * chunkHeaderSize;

      chunks[i] = new HwBinaryResourceChunk() {
          type = ParseChunkType(
              BinaryUtils.ReadUInt64BigEndian(bytes, offset)),
          offset = BinaryUtils.ReadUInt32BigEndian(bytes, offset + 8),
          size = BinaryUtils.ReadUInt32BigEndian(bytes, offset + 12)
      };
    }

    this.Chunks = chunks;
  }

  protected static HwBinaryResourceChunkType ParseChunkType(ulong type) {
    if (Enum.TryParse(type.ToString(),
                      out HwBinaryResourceChunkType result)) {
      return result;
    }

    return HwBinaryResourceChunkType.UNKNOWN;
  }

  protected HwBinaryResourceChunk[] GetAllChunksOfType(
      HwBinaryResourceChunkType type)
    => this.Chunks.Where(chunk => chunk.type == type).ToArray();

  protected HwBinaryResourceChunk GetFirstChunkOfType(
      HwBinaryResourceChunkType type)
    => this.Chunks.FirstOrDefault(chunk => chunk.type == type);
}