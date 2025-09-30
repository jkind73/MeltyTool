using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl;

public partial class GlState {
  public int CurrentUboId { get; set; } = -1;
}

public static partial class GlUtil {
  public static void ResetUbo() => BindUbo(0);

  public static void BindUbo(int uboId) {
    if (currentState_.CurrentUboId == uboId) {
      return;
    }

    GL.BindBuffer(BufferTarget.UniformBuffer, uboId);
  }
}