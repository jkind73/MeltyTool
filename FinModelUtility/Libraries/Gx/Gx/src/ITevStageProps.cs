namespace gx;

public enum GxCc : byte {
  GX_CC_CPREV,
  GX_CC_APREV,
  GX_CC_C0,
  GX_CC_A0,
  GX_CC_C1,
  GX_CC_A1,
  GX_CC_C2,
  GX_CC_A2,
  GX_CC_TEXC,
  GX_CC_TEXA,
  GX_CC_RASC,
  GX_CC_RASA,
  GX_CC_ONE,
  GX_CC_HALF,
  GX_CC_KONST,
  GX_CC_ZERO,
}

public enum GxCa : byte {
  GX_CA_APREV,
  GX_CA_A0,
  GX_CA_A1,
  GX_CA_A2,
  GX_CA_TEXA,
  GX_CA_RASA,
  GX_CA_KONST,
  GX_CA_ZERO,
}

public enum TevOp : byte {
  GX_TEV_ADD = 0,
  GX_TEV_SUB = 1,
  GX_TEV_COMP_R8_GT = 8,
  GX_TEV_COMP_R8_EQ = 9,
  GX_TEV_COMP_GR16_GT = 10,
  GX_TEV_COMP_GR16_EQ = 11,
  GX_TEV_COMP_BGR24_GT = 12,
  GX_TEV_COMP_BGR24_EQ = 13,
  GX_TEV_COMP_RGB8_GT = 14,
  GX_TEV_COMP_RGB8_EQ = 15
}

public enum TevBias : byte {
  GX_TB_ZERO,
  GX_TB_ADDHALF,
  GX_TB_SUBHALF
}

public enum TevScale : byte {
  GX_CS_SCALE_1,
  GX_CS_SCALE_2,
  GX_CS_SCALE_4,
  GX_CS_DIVIDE_2
}

public enum ColorRegister : byte {
  GX_TEVPREV,
  GX_TEVREG0,
  GX_TEVREG1,
  GX_TEVREG2,
}

public interface ITevStageProps {
  GxCc ColorA { get; }
  GxCc ColorB { get; }
  GxCc ColorC { get; }
  GxCc ColorD { get; }
  TevOp ColorOp { get; }
  TevBias ColorBias { get; }
  TevScale ColorScale { get; }
  bool ColorClamp { get; }
  ColorRegister ColorRegid { get; }

  GxCa AlphaA { get; }
  GxCa AlphaB { get; }
  GxCa AlphaC { get; }
  GxCa AlphaD { get; }
  TevOp AlphaOp { get; }
  TevBias AlphaBias { get; }
  TevScale AlphaScale { get; }
  bool AlphaClamp { get; }
  ColorRegister AlphaRegid { get; }
}

public sealed class TevStagePropsImpl : ITevStageProps {
  public GxCc ColorA { get; set; }
  public GxCc ColorB { get; set; }
  public GxCc ColorC { get; set; }
  public GxCc ColorD { get; set; }
  public TevOp ColorOp { get; set; }
  public TevBias ColorBias { get; set; }
  public TevScale ColorScale { get; set; }
  public bool ColorClamp { get; set; }
  public ColorRegister ColorRegid { get; set; }
  public GxCa AlphaA { get; set; }
  public GxCa AlphaB { get; set; }
  public GxCa AlphaC { get; set; }
  public GxCa AlphaD { get; set; }
  public TevOp AlphaOp { get; set; }
  public TevBias AlphaBias { get; set; }
  public TevScale AlphaScale { get; set; }
  public bool AlphaClamp { get; set; }
  public ColorRegister AlphaRegid { get; set; }
}