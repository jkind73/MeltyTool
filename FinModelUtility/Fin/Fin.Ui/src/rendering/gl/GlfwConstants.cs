using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;


namespace fin.ui.rendering.gl;

public static class GlfwConstants {
  public static NativeWindowSettings CreateNewNativeWindowSettings() {
    var nativeWindowSettings = new NativeWindowSettings {
        API = OpenGlVersionService.Es ? ContextAPI.OpenGLES : ContextAPI.OpenGL,
        APIVersion = new Version(OpenGlVersionService.MajorVersion,
                                 OpenGlVersionService.MinorVersion),
        Vsync = VSyncMode.On,
        StartVisible = false,
        RedBits = 8,
        BlueBits = 8,
        GreenBits = 8,
        AlphaBits = 8,
        DepthBits = 32,
    };

    if (GlConstants.COMPATIBILITY) {
      nativeWindowSettings.Profile = ContextProfile.Compatability;
    }

    if (GlConstants.DEBUG) {
      nativeWindowSettings.Flags = ContextFlags.Debug;
    }

    return nativeWindowSettings;
  }
}