using fin.io;
using fin.io.bundles;
using fin.model;

using gm.api;

namespace uni.games.meltyplayer;

public static class PokemonGold3dUtil {
  public static void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IFileHierarchyDirectory rootDir) {
    foreach (var omdFile in rootDir.GetFilesWithFileType(".omd", true)) {
      organizer.Add(new AnnotatedFileBundle<OmdModelFileBundle>(
                        new OmdModelFileBundle {
                            OmdFile = omdFile,
                            Mutator = TweakMaterials_,
                        },
                        omdFile));
    }
  }

  private static void TweakMaterials_(IModel model) {
    foreach (var texture in model.MaterialManager.Textures) {
      texture.MinFilter = TextureMinFilter.NEAR;
      texture.MagFilter = TextureMagFilter.NEAR;
    }
  }
}