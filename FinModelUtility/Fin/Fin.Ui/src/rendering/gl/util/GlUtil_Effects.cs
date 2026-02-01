using System.Drawing;

using fin.model;

using OpenTK.Graphics.OpenGL4;


namespace fin.ui.rendering.gl;

public static partial class GlUtil {
  public static void RenderHighlight(
      Action render,
      Color? highlightColor = null) {
    SetBlendColor(highlightColor ?? Color.FromArgb(180, 255, 255, 255));
    SetBlending(BlendEquation.ADD,
                       BlendFactor.CONST_COLOR,
                       BlendFactor.CONST_ALPHA);
    SetDepth(DepthMode.READ_ONLY, DepthCompareType.Equal);
    DisableChangingBlending = true;
    DisableChangingDepth = true;

    render();

    DisableChangingBlending = false;
    DisableChangingDepth = false;
    ResetBlending();
    ResetDepth();
  }

  public static void RenderOutline(
      Action render,
      Color? outlineColor = null,
      float lineWidth = 8) {
    if (OpenGlVersionService.Es) {
      return;
    }

    SetBlendColor(outlineColor ?? Color.Black);
    SetBlending(BlendEquation.ADD,
                BlendFactor.ZERO,
                BlendFactor.CONST_COLOR);
    SetDepth(DepthMode.READ_ONLY);
    GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
    GL.LineWidth(lineWidth);
    DisableChangingBlending = true;
    DisableChangingDepth = true;

    render();

    DisableChangingBlending = false;
    DisableChangingDepth = false;
    SetBlendColor(Color.White);
    ResetBlending();
    ResetDepth();
    GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
    GL.LineWidth(1);
  }

  public static void RenderAsShadow(
      Action render,
      float opacity = .6f) {
    SetBlendColor(Color.FromArgb((byte) (255 * opacity), 0, 0, 0));
    SetBlending(BlendEquation.ADD,
                BlendFactor.ZERO,
                BlendFactor.ONE_MINUS_CONST_ALPHA);
    SetDepth(DepthMode.READ_ONLY);
    GL.DepthRange(1, 1);
    DisableChangingBlending = true;
    DisableChangingDepth = true;

    render();

    DisableChangingBlending = false;
    DisableChangingDepth = false;
    ResetBlending();
    ResetDepth();
    GL.DepthRange(0, 1);
  }

  public static void RenderWithColor(
      Action render,
      Color color) {
    SetBlendColor(color);
    SetBlending(BlendEquation.ADD,
                BlendFactor.ZERO,
                BlendFactor.ONE_MINUS_CONST_ALPHA);
    DisableChangingBlending = true;

    render();

    DisableChangingBlending = false;
    ResetBlending();
  }
}