namespace fin.ui.rendering.gl;

/// <summary>
///   Util class for forcing OpenGL to use the GPU rather than integrated
///   graphics.
///
///   (This is fucking stupid. Why would I ever want to default to using
///   integrated graphics????)
/// </summary>
public static class GpuUtil {
  [System.Runtime.InteropServices.DllImport("nvapi64.dll", EntryPoint = "fake")]
  static extern int LoadNvApi64();

  [System.Runtime.InteropServices.DllImport("nvapi.dll", EntryPoint = "fake")]
  static extern int LoadNvApi32();

  public static void Initialize() {
    try {
      if (Environment.Is64BitProcess)
        LoadNvApi64();
      else
        LoadNvApi32();
    } catch { } // will always fail since 'fake' entry point doesn't exist
  }
}