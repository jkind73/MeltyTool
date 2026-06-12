using bar.api;

using fin.archives;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.beetle_adventure_racing;

public sealed class BeetleAdventureRacingFileBundleGatherer
    : BN64FileBundleGatherer {
  public override string Name => "beetle_adventure_racing";

  protected override void ExtractFilesFromRom(
      IReadOnlyTreeFile romFile,
      ISystemDirectory extractedDir) {
    new BarFileTableImporter().ExtractInto(romFile, extractedDir);
  }

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress
          mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var rootDirectoryImpl = fileHierarchy.Root.Impl;

    foreach (var uvmdFile in fileHierarchy.Root.FilesWithExtensionRecursive(".uvmd")) {
      organizer.Add(new UvmdModelFileBundle(uvmdFile.Impl, rootDirectoryImpl));
    }

    foreach (var uvctFile in fileHierarchy.Root.FilesWithExtensionRecursive(".uvct")) {
      var uvctFileImpl = uvctFile.Impl;

      organizer.Add(new UvctModelFileBundle(uvctFileImpl, rootDirectoryImpl));
      organizer.Add(new UvctSceneFileBundle(uvctFileImpl, rootDirectoryImpl));
    }

    foreach (var uvtrFile in fileHierarchy.Root.FilesWithExtensionRecursive(".uvtr")) {
      organizer.Add(new UvtrSceneFileBundle(uvtrFile.Impl, rootDirectoryImpl));
    }
  }
}