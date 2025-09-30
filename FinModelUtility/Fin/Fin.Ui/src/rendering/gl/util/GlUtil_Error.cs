using System.Diagnostics;
using System.Runtime.CompilerServices;

using fin.util.asserts;

using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl;

public static partial class GlUtil {
  public const bool ASSERT_NO_ERRORS = true;

  [Conditional("DEBUG")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void AssertNoErrorsWhenDebugging() => AssertNoErrors();        

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void AssertNoErrors() {
    if (ASSERT_NO_ERRORS && TryGetError(out var errorCode)) {
      Asserts.Fail($"Expected to not have any errors, but had: {errorCode}");
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TryGetError(out ErrorCode errorCode) {
    errorCode = GL.GetError();
    return errorCode != ErrorCode.NoError;
  }
}