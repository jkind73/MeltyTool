using fin.math;

using schema.binary;
using schema.binary.attributes;

namespace sappy.schema;

[BinarySchema]
public partial class InstrumentData : IBinaryConvertible {
  private uint word0_;
  private uint word1_;
  private uint word2_;

  // TODO: Only for some instruments

  [Skip]
  public byte Attack => (byte) this.word2_.ExtractFromRight(0, 8);

  [Skip]
  public byte Decay => (byte) this.word2_.ExtractFromRight(8, 8);

  [Skip]
  public byte Sustain => (byte) this.word2_.ExtractFromRight(16, 8);

  [Skip]
  public byte Release => (byte) this.word2_.ExtractFromRight(24, 8);
}