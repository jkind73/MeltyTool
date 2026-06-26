using Avalonia;

using ReactiveUI.Avalonia;

namespace fin.ui.avalonia;

public static class AppBuilderUtil {
  public static AppBuilder CreateFor<TApp>() where TApp : Application, new()
    => AppBuilder.Configure<TApp>()
                 .UsePlatformDetect()
                 .With(new Win32PlatformOptions {
                     RenderingMode = [Win32RenderingMode.AngleEgl],
                     CompositionMode = [
                         Win32CompositionMode.LowLatencyDxgiSwapChain,
                         Win32CompositionMode.WinUIComposition,
                         Win32CompositionMode.DirectComposition
                     ],
                 })
                 .With(new SkiaOptions {
                     // Use as much memory as available, similar to WPF. This
                     // massively improves performance.
                     MaxGpuResourceSizeBytes = long.MaxValue
                 })
                 .WithInterFont()
#if DEBUG
                 .WithDeveloperTools()
#endif
                 .UseReactiveUI(reactiveUiBuilder => { });
}