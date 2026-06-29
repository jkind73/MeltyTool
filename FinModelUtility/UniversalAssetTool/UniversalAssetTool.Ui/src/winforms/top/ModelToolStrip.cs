using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using fin.io.bundles;
using fin.model;
using fin.model.io;
using fin.util.progress;

using uni.config;
using uni.services;
using uni.ui.winforms.common.fileTreeView;

using DialogResult = System.Windows.Forms.DialogResult;

namespace uni.ui.winforms.top;

public partial class ModelToolStrip : UserControl {
  private IFileTreeParentNode? directoryNode_;
  private (IFileTreeLeafNode, IReadOnlyModel)? fileNodeAndModel_;

  private bool hasModelsInDirectory_;
  private bool isModelSelected_;

  public ModelToolStrip() {
    this.InitializeComponent();

    var config = Config.Instance;
    var viewerSettings = config.Viewer;

    var showBonesButton = this.showBonesButton_;
    showBonesButton.Checked = viewerSettings.ShowSkeleton;
    showBonesButton.CheckedChanged += (_, e) => {
      viewerSettings.ShowSkeleton = showBonesButton.Checked;
    };

    var showGridButton = this.showGridButton_;
    showGridButton.Checked = viewerSettings.ShowGrid;
    showGridButton.CheckedChanged += (_, e) => {
      viewerSettings.ShowGrid = showGridButton.Checked;
    };

    var automaticallyPlayMusicButton = this.automaticallyPlayMusicButton_;
    automaticallyPlayMusicButton.Checked =
        viewerSettings.AutomaticallyPlayGameAudioForModel;
    automaticallyPlayMusicButton.CheckedChanged += (_, e) => {
      viewerSettings.AutomaticallyPlayGameAudioForModel =
          showGridButton.Checked;
    };

    this.Progress.ProgressChanged += (_, e) => {
      this.AttemptToUpdateExportSelectedModelButtonEnabledState_();
      this
          .AttemptToUpdateExportAllModelsInSelectedDirectoryButtonEnabledState_();
    };
  }

  public MemoryProgress<(float, IModelFileBundle?)> Progress
    => ExportService.Progress;

  public CancellationTokenSource? CancellationToken
    => ExportService.CancellationToken;

  public bool IsStarted => ExportService.IsStarted;
  public bool IsInProgress => ExportService.IsInProgress;

  public IFileTreeParentNode? DirectoryNode {
    set {
      var hasDirectory = value != null;
      this.directoryNode_ = value;

      var tooltipText = "Export all models in selected directory";
      var modelCount = 0;
      if (hasDirectory) {
        modelCount = value!.GetFilesOfType<IModelFileBundle>(true)
                           .Count();

        var totalText = this.GetTotalNodeText_(value!);
        tooltipText = modelCount == 1
            ? $"Export {modelCount} model in '{totalText}'"
            : $"Export all {modelCount} models in '{totalText}'";
      }

      this.hasModelsInDirectory_ = modelCount > 0;
      this
          .AttemptToUpdateExportAllModelsInSelectedDirectoryButtonEnabledState_();
      this.exportAllModelsInSelectedDirectoryButton_.ToolTipText =
          tooltipText;
    }
  }

  public (IFileTreeLeafNode?, IReadOnlyModel?) FileNodeAndModel {
    set {
      var (fileNode, model) = value;

      this.isModelSelected_ =
          fileNode?.File.IsOfType<IModelFileBundle>(out _) ?? false;

      if (this.isModelSelected_) {
        this.fileNodeAndModel_ = (fileNode, model!);
      } else {
        this.fileNodeAndModel_ = null;
      }

      var tooltipText = "Export selected model";
      if (this.isModelSelected_) {
        var totalText = this.GetTotalNodeText_(fileNode!);
        tooltipText = $"Export '{totalText}'";
      }

      this.AttemptToUpdateExportSelectedModelButtonEnabledState_();
      this.exportSelectedModelButton_.ToolTipText = tooltipText;
    }
  }

  public void AttemptToUpdateExportSelectedModelButtonEnabledState_() {
    this.exportSelectedModelButton_.Enabled =
        !this.IsInProgress && this.isModelSelected_;
  }

  public void
      AttemptToUpdateExportAllModelsInSelectedDirectoryButtonEnabledState_() {
    this.exportAllModelsInSelectedDirectoryButton_.Enabled =
        !this.IsInProgress && this.hasModelsInDirectory_;
  }

  private void exportAllModelsInSelectedDirectoryButton__Click(
      object sender,
      EventArgs e)
    => ExportService.ExportAllModelsInDirectory(
        this.directoryNode_!,
        ShowMessageBox_);

  private void exportSelectedModelButton__Click(object sender, EventArgs e) {
    if (this.fileNodeAndModel_ == null) {
      return;
    }

    var (fileNode, _) = this.fileNodeAndModel_.Value;
    if (fileNode.File.IsOfType<IModelFileBundle>(out var modelFileBundle)) {
      ExportService.ExportSelectedFile(
          modelFileBundle,
          ShowMessageBox_);
    }
  }

  private string GetTotalNodeText_(IFileTreeNode node) {
    var totalText = "";
    var directory = node;
    while (true) {
      if (totalText.Length > 0) {
        totalText = "/" + totalText;
      }

      totalText = directory.Text + totalText;

      directory = directory.Parent;
      if (directory?.Parent == null) {
        break;
      }
    }

    return totalText;
  }

  private static async Task<services.DialogResult> ShowMessageBox_(
      string title,
      string message,
      bool includeCancel)
    => MessageBox.Show(
        message,
        title,
        includeCancel ? MessageBoxButtons.YesNoCancel : MessageBoxButtons.YesNo,
        MessageBoxIcon.Warning,
        MessageBoxDefaultButton.Button1) switch {
        DialogResult.Cancel => services.DialogResult.CANCEL,
        DialogResult.Yes    => services.DialogResult.YES,
        DialogResult.No     => services.DialogResult.NO,
        _                   => throw new ArgumentOutOfRangeException()
    };
}