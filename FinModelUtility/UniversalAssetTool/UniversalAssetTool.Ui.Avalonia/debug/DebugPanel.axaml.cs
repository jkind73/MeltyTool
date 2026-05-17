using Avalonia.Controls;

using fin.ui.rendering;

namespace uni.ui.avalonia.debug;

public partial class DebugPanel : UserControl {
  public DebugPanel() {
    this.DataContext = DebugService.ViewModel;

    InitializeComponent();
  }
}