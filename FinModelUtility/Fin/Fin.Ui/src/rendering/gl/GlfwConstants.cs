using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;


namespace fin.ui.rendering.gl;

public static class GlfwConstants {
  public static NativeWindowSettings CreateNewNativeWindowSettings() {
    var nativeWindowSettings = new NativeWindowSettings {
        API = GlConstants.Es ? ContextAPI.OpenGLES : ContextAPI.OpenGL,
        APIVersion
            = new Version(GlConstants.MajorVersion, GlConstants.MinorVersion),
        Vsync = VSyncMode.On,
        StartVisible = false
    };

    if (GlConstants.Compatibility) {
      nativeWindowSettings.Profile = ContextProfile.Compatability;
    }

    if (GlConstants.Debug) {
      nativeWindowSettings.Flags = ContextFlags.Debug;
    }

    return nativeWindowSettings;
  }
}