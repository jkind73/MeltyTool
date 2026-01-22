using System;
using System.Globalization;
using System.Text;

using Avalonia;

using fin.services;
using fin.ui.avalonia;
using fin.ui.rendering.gl;
using fin.util.time;

using uni.cli;

namespace uni.ui.avalonia.desktop;

class Program {
  // Initialization code. Don't use any Avalonia, third-party APIs or any
  // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
  // yet and stuff might break.
  [STAThread]
  public static void Main(string[] args) {
    CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    FrameTime.Initialize();
    GpuUtil.Initialize();

    Cli.Run(args,
            () => {
              try {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
              } catch (Exception e) {
                ExceptionService.HandleException(e, null);
              }
            });
  }

  // Avalonia configuration, don't remove; also used by visual designer.
  public static AppBuilder BuildAvaloniaApp()
    => AppBuilderUtil.CreateFor<App>();
}