using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.soulcalibur_ii;

public sealed class SoulcaliburIiFileBundleGatherer : BGameCubeFileBundleGatherer {
  public override string Name => "soulcalibur_ii";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) { }
}