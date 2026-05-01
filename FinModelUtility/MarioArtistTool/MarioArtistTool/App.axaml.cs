using System.Globalization;
using System.Threading;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using fin.ui.avalonia.styles;

using marioartisttool.ViewModels;
using marioartisttool.Views;

namespace marioartisttool;

public partial class App : Application {
  public override void Initialize() {
    AvaloniaXamlLoader.Load(this);
    this.Styles.AddRange(new HeaderStyles());

    Dispatcher.UIThread.Invoke(() => {
      Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
    });
  }

  public override void OnFrameworkInitializationCompleted() {
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime
        desktop) {
      desktop.MainWindow = new MainWindow {
          DataContext = new MainViewModel()
      };
    } else if (ApplicationLifetime is ISingleViewApplicationLifetime
               singleViewPlatform) {
      singleViewPlatform.MainView = new MainView {
          DataContext = new MainViewModel()
      };
    }

    base.OnFrameworkInitializationCompleted();
  }
}