using fin.image.io;

using grezzo.image;
using grezzo.schema.ctxb;

using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb.tex;

[BinarySchema]
public sealed partial class Texture : IBinaryConvertible {
  public uint DataLength { get; private set; }
  public ushort MimapCount { get; private set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool IsEtc1 { get; private set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool IsCubemap { get; private set; }

  public ushort Width { get; private set; }
  public ushort Height { get; private set; }

  public GlTextureFormat ImageFormat { get; private set; }

  public uint DataOffset { get; private set; }

  [StringLengthSource(0x10)]
  public string Name { get; private set; }

  public IImageReader GetImageReader()
    => new CmbImageReader(this.Width,
                          this.Height,
                          this.CollapseFormat_(this.ImageFormat));

  private GlTextureFormat CollapseFormat_(GlTextureFormat format) {
    var lowerFormat = (GlTextureFormat) ((int) format & 0xFFFF);

    if (lowerFormat == GlTextureFormat.ETC1) {
      format = GlTextureFormat.ETC1;
    } else if (lowerFormat == GlTextureFormat.ETC1_A4) {
      format = GlTextureFormat.ETC1_A4;
    }

    return format;
  }
}