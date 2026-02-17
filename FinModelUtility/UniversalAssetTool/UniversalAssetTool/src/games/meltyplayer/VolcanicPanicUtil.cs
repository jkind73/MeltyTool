using fin.io;
using fin.io.bundles;

using gm.api;


namespace uni.games.meltyplayer;

public static class VolcanicPanicUtil {
  public static void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IFileHierarchyDirectory rootDir) {
    var volcanicPanicModFiles
        = rootDir.GetFilesWithFileType(".mod").ToHashSet();
    var bacRockFile = rootDir.AssertGetExistingFile("bacRock.png");
    foreach (var modFile in volcanicPanicModFiles) {
      organizer.Add(new AnnotatedFileBundle<D3dModelFileBundle>(
                        new D3dModelFileBundle {
                            D3dFile = modFile,
                            TextureFile = bacRockFile,
                            FlipNormals = true,
                        },
                        modFile));
    }
  }
}