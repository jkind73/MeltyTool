using fin.io.bundles;
using fin.math.floats;
using fin.model.io;
using fin.util.progress;
using fin.util.tasks;
using fin.util.types;

using uni.api;
using uni.config;
using uni.games;
using uni.ui.winforms.common.fileTreeView;

namespace uni.services;

public enum DialogResult {
  CANCEL,
  YES,
  NO,
}

[IocCandiate]
public static class ExportService {
  public static event Action OnExportStart;
  public static event Action OnExportComplete;

  public delegate DialogResult ShowMessageBox(
      string title,
      string message,
      bool includeCancel);

  public static MemoryProgress<(float, IModelFileBundle?)> Progress { get; } =
    new((0, null));

  public static CancellationTokenSource? CancellationToken { get; private set; }

  public static bool IsStarted => CancellationToken != null;

  public static bool IsInProgress
    => IsStarted && !Progress.Current.Item1.IsRoughly1();

  public static void ExportAllModelsInDirectory(
      IFileTreeParentNode directoryNode,
      ShowMessageBox showMessageBox) {
    var models = directoryNode.GetFilesOfType<IModelFileBundle>(true)
                              .ToArray();
    StartExportingModelsInBackground_(models, showMessageBox);
  }

  public static void ExportSelectedFile(
      IFileTreeLeafNode fileNode,
      ShowMessageBox showMessageBox) {
    if (fileNode.File.IsOfType<IModelFileBundle>(out var modelFileBundle)) {
      StartExportingModelsInBackground_([modelFileBundle], showMessageBox);
    }
  }

  private static void StartExportingModelsInBackground_(
      IReadOnlyList<IModelFileBundle> modelFileBundles,
      ShowMessageBox showMessageBox) {
    var extractorPromptChoice =
        PromptIfModelFileBundlesAlreadyExported_(
            modelFileBundles,
            showMessageBox,
            Config.Instance.Exporter.General.ExportedFormats);
    if (extractorPromptChoice != ExporterUtil.ExporterPromptChoice.CANCEL) {
      CancellationToken = new CancellationTokenSource();

      FinTask.Run(() => {
        OnExportStart?.Invoke();

        ExporterUtil.ExportAll(
            modelFileBundles,
            new GlobalModelImporter(),
            Progress,
            CancellationToken,
            Config.Instance.Exporter.General
                  .ExportedFormats,
            extractorPromptChoice ==
            ExporterUtil.ExporterPromptChoice.OVERWRITE_EXISTING);

        OnExportComplete?.Invoke();
      });
    }
  }

  private static ExporterUtil.ExporterPromptChoice
      PromptIfModelFileBundlesAlreadyExported_(
          IReadOnlyList<IFileBundle> modelFileBundles,
          ShowMessageBox showMessageBox,
          IReadOnlySet<ExportedFormat> formats) {
    if (ExporterUtil.CheckIfModelFileBundlesAlreadyExported(
            modelFileBundles,
            formats,
            out var existingOutputFiles)) {
      var totalCount = modelFileBundles.Count;
      if (totalCount == 1) {
        var result = showMessageBox(
            "Model has already been exported!",
            $"Model defined in \"{existingOutputFiles.First().DisplayFullPath}\" has already been exported. Would you like to overwrite it?",
            false);
        return result switch {
            DialogResult.YES => ExporterUtil.ExporterPromptChoice
                                            .OVERWRITE_EXISTING,
            DialogResult.NO => ExporterUtil.ExporterPromptChoice.CANCEL,
        };
      } else {
        var existingCount = existingOutputFiles.Count();
        var result = showMessageBox(
            $"{existingCount}/{totalCount} models have already been exported!",
            $"{existingCount} model{(existingCount != 1 ? "s have" : " has")} already been exported. Select 'Yes' to overwrite them, 'No' to skip them, or 'Cancel' to abort this operation.",
            true);
        return result switch {
            DialogResult.YES => ExporterUtil.ExporterPromptChoice
                                            .OVERWRITE_EXISTING,
            DialogResult.NO => ExporterUtil.ExporterPromptChoice.SKIP_EXISTING,
            DialogResult.CANCEL => ExporterUtil.ExporterPromptChoice.CANCEL,
        };
      }
    }

    return ExporterUtil.ExporterPromptChoice.SKIP_EXISTING;
  }
}