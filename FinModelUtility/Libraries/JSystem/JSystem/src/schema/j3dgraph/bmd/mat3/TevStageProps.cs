using gx;

using schema.binary;
using schema.binary.attributes;


namespace jsystem.schema.j3dgraph.bmd.mat3;

[BinarySchema]
public sealed partial class TevStageProps : ITevStageProps, IBinaryConvertible {
  private readonly byte padding0_ = byte.MaxValue;

  public GxCc ColorA { get; set; }
  public GxCc ColorB { get; set; }
  public GxCc ColorC { get; set; }
  public GxCc ColorD { get; set; }
  public TevOp ColorOp { get; set; }
  public TevBias ColorBias { get; set; }
  public TevScale ColorScale { get; set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool ColorClamp { get; set; }

  public ColorRegister ColorRegid { get; set; }

  public GxCa AlphaA { get; set; }
  public GxCa AlphaB { get; set; }
  public GxCa AlphaC { get; set; }
  public GxCa AlphaD { get; set; }
  public TevOp AlphaOp { get; set; }
  public TevBias AlphaBias { get; set; }
  public TevScale AlphaScale { get; set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool AlphaClamp { get; set; }

  public ColorRegister AlphaRegid { get; set; }

  private readonly byte padding1_ = byte.MaxValue;
}