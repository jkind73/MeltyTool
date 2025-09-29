using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;


namespace fin.ui.rendering.gl;

public static class GlConstants {
  public static ContextAPI Api => ContextAPI.OpenGLES;
  public static Version Version { get; } = new(3, 1);
  public static bool Compatibility => true;

  public static NativeWindowSettings NativeWindowSettings { get; } =
    CreateNewNativeWindowSettings();

  public static NativeWindowSettings CreateNewNativeWindowSettings() {
    var nativeWindowSettings = new NativeWindowSettings {
        API = Api,
        APIVersion = Version,
        Vsync = VSyncMode.On
    };

    if (Compatibility) {
      nativeWindowSettings.Profile = ContextProfile.Compatability;
    }

    return nativeWindowSettings;
  }
}