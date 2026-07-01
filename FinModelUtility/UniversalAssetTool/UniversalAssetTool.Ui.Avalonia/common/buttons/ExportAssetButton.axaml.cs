using System;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;

using fin.io.bundles;
using fin.model.io;

using uni.services;
using uni.ui.avalonia.services;

namespace uni.ui.avalonia.common.buttons;

public partial class ExportAssetButton : UserControl {
  public IObservable<IFileBundle?> ΔFileBundle
    => ContextService.ΔFileBundle;

  public IObservable<bool> ΔIsEnabled
    => this.ΔFileBundle.Select(f => f != null);

  public ExportAssetButton() => this.InitializeComponent();

  protected void Button_OnClick(object? sender, RoutedEventArgs e) {
    this.ΔFileBundle.Take(1).Subscribe(fb => {
      if (fb is IModelFileBundle modelFileBundle) {
        FileBundleExportService.ExportModelFileBundleTo(
            modelFileBundle,
            TopLevel.GetTopLevel(this));
      }
    });
  }
}