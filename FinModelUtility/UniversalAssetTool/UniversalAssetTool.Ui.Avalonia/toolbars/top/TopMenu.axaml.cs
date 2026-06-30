using System;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Interactivity;

using fin.io.bundles;
using fin.io.web;
using fin.model.io;
using fin.ui;
using fin.util.io;
using fin.util.tasks;

using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

using ReactiveUI;

using uni.services;
using uni.ui.avalonia.common.buttons;
using uni.ui.avalonia.io;
using uni.ui.avalonia.services;
using uni.ui.avalonia.settings;
using uni.ui.avalonia.util;
using uni.ui.winforms.common.fileTreeView;

namespace uni.ui.avalonia.toolbars.top;

public sealed class TopMenuModelForDesigner : TopMenuModel {
  public new IFileBundle? FileBundle => null;
  public new IFileTreeParentNode? SelectedDirectory => null;
}

public class TopMenuModel : BViewModel {
  public TopMenuModel() {
    this.SelectedDirectory = null;
    FileBundleService.OnFileBundleOpened
        += (_, fileBundle) => this.FileBundle = fileBundle;
    SelectedFileTreeDirectoryService.OnFileTreeDirectorySelected
        += directory => this.SelectedDirectory = directory;
  }

  public IFileBundle? FileBundle {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);
      this.HasExportableFileBundle = value is IModelFileBundle;
    }
  }

  public IFileTreeParentNode? SelectedDirectory {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);

      var fileBundleCount = value?.GetFiles(true)
                                 .Count(fb => fb is IModelFileBundle) ?? 0;
      this.ExportInDirectoryButtonEnabled = fileBundleCount > 0;
      this.ExportInDirectoryText
          = $"Export _all {fileBundleCount} asset{(fileBundleCount == 1 ? "" : "s")} in {value?.GetLocalPath() ?? "selected directory"} to out/";
    }
  }

  public bool HasExportableFileBundle {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public bool ExportInDirectoryButtonEnabled {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public string ExportInDirectoryText {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class TopMenu : UserControl {
  public TopMenu() {
    InitializeComponent();
    this.DataContext = new TopMenuModel();
  }

  private void OpenFileWindowAndTryToImportAsset_(
      object? sender,
      RoutedEventArgs e)
    => FinTask.Run(() => ImportAssetButton
                       .OpenFileWindowAndTryToImportAsset(this));

  private void OpenSettingsWindow_(object? sender, RoutedEventArgs e)
    => this.ShowNewWindow(() => new SettingsWindow());

  private void OpenGithubInBrowser_(object? sender, RoutedEventArgs e)
    => WebBrowserUtil.OpenUrl(GitHubUtil.GITHUB_CHOOSE_NEW_ISSUE_URL);

  private void OpenExtractionProgressWindow_(object? sender,
                                             RoutedEventArgs e)
    => FileBundleGatherersService.ShowExtractorProgressWindow(this);

  private void ExportTo_(object? sender, RoutedEventArgs e) {
    var fileBundle = (this.DataContext as TopMenuModel)?.FileBundle;
    if (fileBundle is IModelFileBundle modelFileBundle) {
      FileBundleExportService.ExportModelFileBundleTo(
          modelFileBundle,
          TopLevel.GetTopLevel(this));
    }
  }

  private void ExportSelectedAsset_(object? sender, RoutedEventArgs e) {
    var fileBundle = (this.DataContext as TopMenuModel)?.FileBundle;
    if (fileBundle is IModelFileBundle modelFileBundle) {
      ExportService.ExportSelectedFile(modelFileBundle, ShowMessageBox_);
    }
  }

  private void ExportSelectedDirectory_(object? sender, RoutedEventArgs e) {
    var selectedDirectory = (this.DataContext as TopMenuModel)?.SelectedDirectory;
    if (selectedDirectory != null) {
      ExportService.ExportAllModelsInDirectory(
          selectedDirectory,
          ShowMessageBox_);
    }
  }

  private static async Task<DialogResult> ShowMessageBox_(
      string title,
      string message,
      bool includeCancel)
    => (await MessageBoxManager.GetMessageBoxStandard(
                                   title,
                                   message,
                                   includeCancel
                                       ? ButtonEnum.YesNoCancel
                                       : ButtonEnum.YesNo,
                                   Icon.Warning)
                               .ShowAsync()) switch {
        ButtonResult.Yes    => DialogResult.YES,
        ButtonResult.No     => DialogResult.NO,
        ButtonResult.Cancel => DialogResult.CANCEL,
        _                   => throw new ArgumentOutOfRangeException()
    };
}