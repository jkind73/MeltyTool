using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.animal_crossing;

public sealed class AnimalCrossingFileBundleGatherer : BGameCubeFileBundleGatherer {
  public override string Name => "animal_crossing";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) { }
}