using fin.model;
using fin.util.enums;

using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl;

public partial class GlState {
  public (DepthMode, DepthCompareType) DepthModeAndCompareType { get; set; }
    = (DepthMode.READ | DepthMode.WRITE, DepthCompareType.L_EQUAL);
}

public static partial class GlUtil {
  public static bool DisableChangingDepth { get; set; }

  public static void ResetDepth()
    => SetDepth((DepthMode.READ | DepthMode.WRITE), DepthCompareType.L_EQUAL);

  public static bool SetDepth(DepthMode depthMode)
    => SetDepth(depthMode, depthMode == DepthMode.NONE ? DepthCompareType.ALWAYS : DepthCompareType.L_EQUAL);

  public static bool SetDepth(
      DepthMode depthMode,
      DepthCompareType depthCompareType) {
    if (DisableChangingDepth) {
      return false;
    }

    if (currentState_.DepthModeAndCompareType ==
        (depthMode, depthCompareType)) {
      return false;
    }

    currentState_.DepthModeAndCompareType
        = (depthMode, depthCompareType);

    if (depthMode.CheckFlag(DepthMode.READ) &&
        depthCompareType != DepthCompareType.ALWAYS) {
      GL.Enable(EnableCap.DepthTest);
      GL.DepthFunc(ConvertFinDepthCompareTypeToGl_(depthCompareType));
    } else {
      GL.Disable(EnableCap.DepthTest);
    }

    GL.DepthMask(depthMode.CheckFlag(DepthMode.WRITE));

    return true;
  }

  private static DepthFunction ConvertFinDepthCompareTypeToGl_(
      DepthCompareType finDepthCompareType)
    => finDepthCompareType switch {
        DepthCompareType.L_EQUAL  => DepthFunction.Lequal,
        DepthCompareType.LESS    => DepthFunction.Less,
        DepthCompareType.EQUAL   => DepthFunction.Equal,
        DepthCompareType.GREATER => DepthFunction.Greater,
        DepthCompareType.N_EQUAL  => DepthFunction.Notequal,
        DepthCompareType.G_EQUAL  => DepthFunction.Gequal,
        DepthCompareType.ALWAYS  => DepthFunction.Always,
        DepthCompareType.NEVER   => DepthFunction.Never,
        _ => throw new ArgumentOutOfRangeException(
            nameof(finDepthCompareType),
            finDepthCompareType,
            null)
    };
}