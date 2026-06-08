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
    // TODO: Gather these
  }
}