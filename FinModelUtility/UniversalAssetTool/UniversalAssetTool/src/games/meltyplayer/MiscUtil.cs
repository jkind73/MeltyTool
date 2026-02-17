using fin.io;
using fin.io.bundles;
using fin.model;

using gm.api;


namespace uni.games.meltyplayer;

public static class MiscUtil {
  public static void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IFileHierarchyDirectory rootDir) {
    foreach (var modFile
             in rootDir.GetExistingFilesRecursive()
                       .Where(f => !f.FileType.Equals(
                                  ".png",
                                  StringComparison.OrdinalIgnoreCase))) {
      var textureFile = new FinFile($"{modFile.FullNameWithoutExtension}.png");
      if (!textureFile.Exists) {
        textureFile = null;
      }

      organizer.Add(
          new AnnotatedFileBundle<D3dModelFileBundle>(
              new D3dModelFileBundle {
                  ModFile = modFile,
                  TextureFile = textureFile,
                  TextureWrapMode = WrapMode.REPEAT,
              },
              (IFileHierarchyFile) modFile));
    }
  }
}