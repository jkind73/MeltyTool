using System.Linq;

using schema.binary;
using schema.binary.attributes;

namespace hw.schema.binary;

[BinarySchema]
[Endianness(Endianness.BigEndian)]
public partial class BinaryResource : IBinaryDeserializable {
  private uint unk0_;

  public uint HeaderSize { get; set; }

  private uint unk1_;
  private uint unk2_;

  [WLengthOfSequence(nameof(Chunks))]
  public ushort NumChunks { get; set; }

  [Skip]
  private uint LengthOfUnk3 => this.HeaderSize - 18;

  [RSequenceLengthSource(nameof(LengthOfUnk3))]
  private byte[] unk3_;

  [RSequenceLengthSource(nameof(NumChunks))]
  public BinaryResourceChunk[] Chunks { get; private set; }

  public BinaryResourceChunk[] GetAllChunksOfType(
      BinaryResourceChunkType type)
    => this.Chunks.Where(chunk => chunk.Type == type).ToArray();

  public BinaryResourceChunk GetFirstChunkOfType(
      BinaryResourceChunkType type)
    => this.Chunks.FirstOrDefault(chunk => chunk.Type == type);
}