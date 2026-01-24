using fin.schema.data;

using schema.binary;
using schema.binary.attributes;

namespace visceral.schema.str.content;

[BinarySchema]
public sealed partial class ContentBlock : IBlock {
  public SwitchMagicWrapper<ContentType, IContent> Impl { get; }
    = new(br => (ContentType) br.ReadUInt32(),
          (bw, magic) => bw.WriteUInt32((uint) magic),
          magic => magic switch {
              ContentType.HEADER         => new FileInfo(),
              ContentType.DATA           => new UncompressedData(),
              ContentType.COMPRESSED_DATA => new RefPackCompressedData(),
              _                          => throw new ArgumentOutOfRangeException(nameof(magic), magic, null)
          });

  [Skip]
  public BlockType Type => BlockType.CONTENT;

  public override string ToString() => this.Impl.ToString();
}