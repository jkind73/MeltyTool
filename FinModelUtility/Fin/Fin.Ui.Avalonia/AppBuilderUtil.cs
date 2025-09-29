using System;

using Avalonia;
using Avalonia.OpenGL;
using Avalonia.ReactiveUI;
using Avalonia.Win32;

using fin.ui.rendering.gl;

using OpenTK.Windowing.Common;


namespace fin.ui.avalonia;

public static class AppBuilderUtil {
  public static AppBuilder CreateFor<TApp>() where TApp : Application, new()
    => AppBuilder.Configure<TApp>()
                 .UsePlatformDetect()
                 .With(new AngleOptions {
                     GlProfiles = [
                         new GlVersion(
                             GlConstants.Api switch {
                                 ContextAPI.OpenGLES => GlProfileType.OpenGLES,
                                 ContextAPI.OpenGL => GlProfileType.OpenGL,
                                 _ => throw new ArgumentOutOfRangeException()
                             },
                             GlConstants.Version.Major,
                             GlConstants.Version.Minor,
                             GlConstants.Compatibility)
                     ],
                 })
                 .With(new Win32PlatformOptions {
                     RenderingMode = [Win32RenderingMode.AngleEgl],
                     CompositionMode = [Win32CompositionMode.LowLatencyDxgiSwapChain]
                 })
                 .With(new SkiaOptions {
                     // Use as much memory as available, similar to WPF. This
                     // massively improves performance.
                     MaxGpuResourceSizeBytes = long.MaxValue
                 })
                 .WithInterFont()
                 .UseReactiveUI();
}