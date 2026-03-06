using System.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

using fin.common;
using fin.io;
using fin.io.bundles;
using fin.model.io;
using fin.model.processing;
using fin.util.sets;
using fin.util.strings;
using fin.util.tasks;

using uni.api;
using uni.config;
using uni.games;
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