using fin.io;
using fin.io.bundles;
using fin.util.progress;

using gm.api;


namespace uni.games.mario_kart_3d;

public sealed class MarioKart3dFileBundleGatherer : BPrereqsFileBundleGatherer {
  public override string Name => "mario_kart_3d";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    foreach (var smkFile in
             fileHierarchy.Root.FilesWithExtensionRecursive(".smk")) {
      var gifFile = smkFile.AssertGetParent()
                           .GetExistingFiles()
                           .WithFileType(".gif")
                           .SingleOrDefault();
      var pngFile = smkFile.AssertGetParent()
                           .GetExistingFiles()
                           .WithFileType(".png")
                           .SingleOrDefault();

      organizer.Add(
          new AnnotatedFileBundle<D3dModelFileBundle>(
              new D3dModelFileBundle {
                  ModFile = smkFile,
                  TextureFile = gifFile == null ? pngFile : null,
                  AnimatedTextureFile = pngFile == null && gifFile != null
                      ? (gifFile, 30)
                      : null,
              },
              smkFile));
    }
  }
}