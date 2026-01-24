using Avalonia;
using Avalonia.OpenGL;
using Avalonia.Win32;
using ReactiveUI.Avalonia;

using fin.ui.rendering.gl;


namespace fin.ui.avalonia;

public static class AppBuilderUtil {
  public static AppBuilder CreateFor<TApp>() where TApp : Application, new()
    => AppBuilder.Configure<TApp>()
                 .UsePlatformDetect()
                 .With(new AngleOptions {
                     GlProfiles = [
                         // This needs to be OpenGL ES to start up, but we'll
                         // use a different version of OpenGL downstream.
                         new GlVersion(
                             GlProfileType.OpenGLES,
                             3,
                             1,
                             GlConstants.COMPATIBILITY)
                     ],
                 })
                 .With(new Win32PlatformOptions {
                     RenderingMode = [Win32RenderingMode.AngleEgl],
                     CompositionMode = [
                         Win32CompositionMode.LowLatencyDxgiSwapChain,
                         Win32CompositionMode.WinUIComposition,
                         Win32CompositionMode.DirectComposition
                     ]
                 })
                 .With(new SkiaOptions {
                     // Use as much memory as available, similar to WPF. This
                     // massively improves performance.
                     MaxGpuResourceSizeBytes = long.MaxValue
                 })
                 .WithInterFont()
                 .UseReactiveUI();
}