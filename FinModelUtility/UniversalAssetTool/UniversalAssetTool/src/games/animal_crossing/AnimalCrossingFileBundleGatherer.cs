using fin.config;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms.gcn;
using uni.platforms.gcn.tools;

namespace uni.games.animal_crossing;

public sealed class AnimalCrossingFileBundleGatherer
    : BGameCubeFileBundleGatherer {
  public override string Name => "animal_crossing";

  public override GcnFileHierarchyExtractor.Options Options
    => GcnFileHierarchyExtractor
       .Options
       .Empty()
       .UseRarcDumpForExtensions(".arc");

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var root = fileHierarchy.Root;

    var forestaDir = root.Impl.GetOrCreateSubdir("foresta");
    if (forestaDir.IsEmpty &&
        root.TryToGetExistingFile("foresta.rarc", out var forestaRarc) &&
        root.TryToGetExistingFile("foresta.map", out var forestaMap)) {
      var relDump = new RelDump();
      relDump.Run(forestaRarc,
                  forestaMap,
                  FinConfig.CleanUpArchives);
      fileHierarchy.RefreshRootAndUpdateCache();
    }
  }
}