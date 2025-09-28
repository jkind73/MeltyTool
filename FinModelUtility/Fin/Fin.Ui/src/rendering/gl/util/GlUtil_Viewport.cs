using System.Drawing;

using OpenTK.Graphics.ES30;

namespace fin.ui.rendering.gl;

public partial class GlState {
  public Rectangle Viewport { get; set; } = Rectangle.Empty;
}

public static partial class GlUtil {
  public static void SetViewport(Rectangle viewport) {
    if (currentState_.Viewport == viewport) {
      return;
    }

    AssertNoErrorsWhenDebugging();
    GL.Viewport(viewport);
    AssertNoErrorsWhenDebugging();
  }
}