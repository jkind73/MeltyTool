using fin.io;
using fin.io.bundles;
using fin.util.progress;

using gdl.api;

namespace uni.games.gauntlet_dark_legacy;

public sealed class GauntletDarkLegacyFileBundleGatherer : BGameCubeFileBundleGatherer {
  public override string Name => "gauntlet_dark_legacy";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    foreach (var animFile in fileHierarchy.Root.GetExistingFilesRecursive()
                                          .ByName("anim.ps2")
                                          .Cast<IFileHierarchyFile>()) {
      organizer.Add(new AnimModelFileBundle(animFile).Annotate(animFile));
    }
  }
}