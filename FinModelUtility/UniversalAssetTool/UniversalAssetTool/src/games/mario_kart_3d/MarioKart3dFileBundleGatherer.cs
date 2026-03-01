using fin.io;
using fin.io.bundles;
using fin.util.progress;

using mk3d.api;


namespace uni.games.mario_kart_3d;

public sealed class MarioKart3dFileBundleGatherer : BPrereqsFileBundleGatherer {
  public override string Name => "mario_kart_3d";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var rootDir = fileHierarchy.Root;

    foreach (var smkFile in rootDir.FilesWithExtensionRecursive(".smk")) {
      organizer.Add(
          Mk3dModelFileBundleUtil
              .FromSmkFile(smkFile)
              .Annotate(smkFile));
    }

    var track1PlaceholderFile
        = rootDir.AssertGetExistingFile("track1/track1.placeholder");
    organizer.Add(
        new Mk3dTrackSceneFileBundle(track1PlaceholderFile)
            .Annotate(track1PlaceholderFile));
  }
}