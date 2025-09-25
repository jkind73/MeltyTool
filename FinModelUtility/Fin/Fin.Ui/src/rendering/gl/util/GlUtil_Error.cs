using System.Diagnostics;
using System.Runtime.CompilerServices;

using fin.util.asserts;

using OpenTK.Graphics.ES30;

namespace fin.ui.rendering.gl;

public static partial class GlUtil {
  [Conditional("DEBUG")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void AssertNoErrorsWhenDebugging() => AssertNoErrors();        

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void AssertNoErrors() {
    if (TryGetError(out var errorCode)) {
      Asserts.Fail($"Expected to not have any errors, but had: {errorCode}");
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TryGetError(out ErrorCode errorCode) {
    errorCode = GL.GetError();
    return errorCode != ErrorCode.NoError;
  }
}