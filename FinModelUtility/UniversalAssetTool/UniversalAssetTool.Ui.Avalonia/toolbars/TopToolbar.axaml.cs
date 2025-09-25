using Avalonia.Controls;

using fin.io.bundles;
using fin.ui.avalonia;

using ReactiveUI;

namespace uni.ui.avalonia.toolbars;

public sealed class TopToolbarModelForDesigner : TopToolbarModel {
  public new IFileBundle? FileBundle => null;
}

public class TopToolbarModel : BViewModel {
  public IFileBundle? FileBundle {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class TopToolbar : UserControl {
  public TopToolbar() => this.InitializeComponent();
}