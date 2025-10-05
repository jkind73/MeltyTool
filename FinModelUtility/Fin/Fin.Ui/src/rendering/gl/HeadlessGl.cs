using OpenTK.Windowing.Desktop;


namespace fin.ui.rendering.gl;

public static class HeadlessGl {
  private static GameWindow? headlessWindow_;

  private static void InitIfNull_() {
    if (headlessWindow_ != null) {
      return;
    }

    GLFWProvider.CheckForMainThread = false;
    headlessWindow_ = new GameWindow(
        GameWindowSettings.Default,
        new NativeWindowSettings {
            StartVisible = false,
        });
  }

  public static void MakeCurrent() {
    InitIfNull_();

    GlUtil.SwitchContext(headlessWindow_);
    headlessWindow_.MakeCurrent();
    GlUtil.InitGl();
  }
}