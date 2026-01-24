using schema.binary;

namespace ttyd.schema.tpl;

[BinarySchema]
public sealed partial class TplHeader : IBinaryDeserializable {
  private readonly uint magic_ = 0x0020AF30;
  public int NumTextures { get; set; }
  public uint HeaderSize { get; set; }
}