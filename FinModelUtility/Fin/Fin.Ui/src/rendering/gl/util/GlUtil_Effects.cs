using System.Drawing;

using fin.model;


namespace fin.ui.rendering.gl;

public static partial class GlUtil {
  public static void RenderHighlight(
      Action render,
      Color? highlightColor = null) {
    SetBlendColor(highlightColor ?? Color.FromArgb(180, 255, 255, 255));
    SetBlending(BlendEquation.ADD,
                       BlendFactor.CONST_COLOR,
                       BlendFactor.CONST_ALPHA);
    SetDepth(DepthMode.READ_ONLY);
    DisableChangingBlending = true;
    DisableChangingDepth = true;

    render();

    DisableChangingBlending = false;
    DisableChangingDepth = false;
    ResetBlending();
    ResetDepth();
  }

  public static void RenderAsShadow(
      Action render,
      float opacity = .6f) {
    SetBlendColor(Color.FromArgb((byte) (255 * opacity), 0, 0, 0));
    SetBlending(BlendEquation.ADD,
                BlendFactor.CONST_COLOR,
                BlendFactor.ONE_MINUS_CONST_ALPHA);
    DisableChangingBlending = true;

    render();

    DisableChangingBlending = false;
    ResetBlending();
  }
}