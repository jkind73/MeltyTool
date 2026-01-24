using fin.model;

namespace gx;

public enum GxCompareType : byte {
  NEVER = 0,
  LESS = 1,
  EQUAL = 2,
  L_EQUAL = 3,
  GREATER = 4,
  N_EQUAL = 5,
  G_EQUAL = 6,
  ALWAYS = 7
}

public static class GxCompareTypeExtensions {
  public static DepthCompareType ToFinDepthCompareType(
      this GxCompareType gxDepthCompareType)
    => gxDepthCompareType switch {
        GxCompareType.NEVER   => DepthCompareType.NEVER,
        GxCompareType.LESS    => DepthCompareType.LESS,
        GxCompareType.EQUAL   => DepthCompareType.EQUAL,
        GxCompareType.L_EQUAL  => DepthCompareType.L_EQUAL,
        GxCompareType.GREATER => DepthCompareType.GREATER,
        GxCompareType.N_EQUAL  => DepthCompareType.N_EQUAL,
        GxCompareType.G_EQUAL  => DepthCompareType.G_EQUAL,
        GxCompareType.ALWAYS  => DepthCompareType.ALWAYS,
        _ => throw new ArgumentOutOfRangeException(
            nameof(gxDepthCompareType),
            gxDepthCompareType,
            null)
    };

  public static AlphaCompareType ToFinAlphaCompareType(
      this GxCompareType gxAlphaCompareType)
    => gxAlphaCompareType switch {
        GxCompareType.NEVER   => AlphaCompareType.NEVER,
        GxCompareType.LESS    => AlphaCompareType.LESS,
        GxCompareType.EQUAL   => AlphaCompareType.EQUAL,
        GxCompareType.L_EQUAL  => AlphaCompareType.L_EQUAL,
        GxCompareType.GREATER => AlphaCompareType.GREATER,
        GxCompareType.N_EQUAL  => AlphaCompareType.N_EQUAL,
        GxCompareType.G_EQUAL  => AlphaCompareType.G_EQUAL,
        GxCompareType.ALWAYS  => AlphaCompareType.ALWAYS,
        _ => throw new ArgumentOutOfRangeException(
            nameof(gxAlphaCompareType),
            gxAlphaCompareType,
            null)
    };
}


public enum GxAlphaOp : byte {
  AND = 0,
  OR = 1,
  XOR = 2,
  XNOR = 3
}

public static class GxAlphaOpExtensions {
  public static AlphaOp ToFinAlphaOp(this GxAlphaOp gxAlphaOp)
    => gxAlphaOp switch {
        GxAlphaOp.AND  => AlphaOp.AND,
        GxAlphaOp.OR   => AlphaOp.OR,
        GxAlphaOp.XOR  => AlphaOp.XOR,
        GxAlphaOp.XNOR => AlphaOp.XNOR,
        _ => throw new ArgumentOutOfRangeException(
            nameof(gxAlphaOp),
            gxAlphaOp,
            null)
    };
}


public interface IAlphaCompare {
  GxCompareType Func0 { get; }
  float Reference0 { get; }

  GxCompareType Func1 { get; }
  float Reference1 { get; }

  GxAlphaOp MergeFunc { get; }
}

public sealed class AlphaCompareImpl : IAlphaCompare {
  public GxCompareType Func0 { get; set; }
  public float Reference0 { get; set; }
  public GxCompareType Func1 { get; set; }
  public float Reference1 { get; set; }
  public GxAlphaOp MergeFunc { get; set; }
}