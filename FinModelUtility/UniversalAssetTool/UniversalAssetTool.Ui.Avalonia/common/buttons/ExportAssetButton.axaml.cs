using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

using fin.io.bundles;
using fin.model.io;

using uni.ui.avalonia.services;

namespace uni.ui.avalonia.common.buttons;

public partial class ExportAssetButton : UserControl {
  public static readonly StyledProperty<IFileBundle?> FileBundleProperty =
      AvaloniaProperty.Register<ExportAssetButton, IFileBundle?>(
          nameof(FileBundle));

  public IFileBundle? FileBundle {
    get => this.GetValue(FileBundleProperty);
    set => this.SetValue(FileBundleProperty, value);
  }

  public ExportAssetButton() => this.InitializeComponent();


  protected void Button_OnClick(object? sender, RoutedEventArgs e) {
    if (this.FileBundle is IModelFileBundle modelFileBundle) {
      FileBundleExportService.ExportModelFileBundleTo(
          modelFileBundle,
          TopLevel.GetTopLevel(this));
    }
  }
}