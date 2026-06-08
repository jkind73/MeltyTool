using schema.binary;
using schema.binary.attributes;

namespace fin.schema.data;

[BinarySchema]
public sealed partial class PassThruUnknownStringMagicUInt32SizedSection<T>(
    T data) : IMagicSection<T>
    where T : IBinaryConvertible {
  [StringLengthSource(4)]
  public string Magic { get; set; }

  private readonly PassThruUInt32SizedSection<T> impl_ = new(data);

  [Skip]
  public int TweakReadSize {
    get => this.impl_.TweakReadSize;
    set => this.impl_.TweakReadSize = value;
  }

  [Skip]
  public uint Size => this.impl_.Size;

  [Skip]
  public T Data {
    get => this.impl_.Data;
    set => this.impl_.Data = value;
  }
}