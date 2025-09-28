using System.Runtime.CompilerServices;

using fin.util.asserts;

using OpenTK.Graphics.ES30;

namespace fin.ui.rendering.gl;

public partial class GlState {
  public int CurrentShader { get; set; } = -1;
}

public static partial class GlUtil {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void UseProgram(int shader) {
    if (currentState_.CurrentShader == shader) {
      return;
    }

    AssertNoErrorsWhenDebugging();
    currentState_.CurrentShader = shader;
    GL.UseProgram(shader);
    AssertNoErrorsWhenDebugging();
  }

  public static void ValidateCurrentProgram() {
    var programId = currentState_.CurrentShader;

    AssertNoErrorsWhenDebugging();
    GL.ValidateProgram(programId);
    GL.GetProgram(programId,
                  GetProgramParameterName.ValidateStatus,
                  out var validateStatus);

    if (validateStatus == 0) {
      var bufferSize = 10000;
      GL.GetProgramInfoLog(
          programId,
          bufferSize,
          out var shaderErrorLength,
          out var shaderError);

      if (shaderError?.Length > 0) {
        Asserts.Fail(shaderError);
      }
    }
    AssertNoErrorsWhenDebugging();
  }
}