using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Threading;

using fin.config.avalonia.services;
using fin.services;
using fin.ui.avalonia.dialogs;

using marioartisttool.services;

namespace marioartisttool.Views;

public partial class MainWindow : Window {
  public MainWindow() {
    InitializeComponent();

    ExceptionService.OnException += (e, c) => {
      Dispatcher.UIThread.Invoke(() => {
        var dialog = new ExceptionDialog {
            DataContext = new ExceptionDialogViewModel
                { Exception = e, Context = c },
            CanResize = false,
        };

        dialog.ShowDialog(this);
      });
    };

    TopLevelService.Init(this);
    Task.Run(MfsFileSystemService.LoadFromConfigOrPromptUserForDiskFile);
  }
}