using fin.ui.rendering.gl;

using OpenTK.Windowing.Desktop;

namespace UniversalAssetTool.Ui.OpenTk;

internal class Program {
  static void Main(string[] args) {
    OpenGlVersionService.Init(false);

    var nativeWindowSettings = GlfwConstants.CreateNewNativeWindowSettings();
    nativeWindowSettings.StartVisible = true;

    using var window = new SceneViewerWindow(
        GameWindowSettings.Default,
        nativeWindowSettings);
    window.Run();
  }
}