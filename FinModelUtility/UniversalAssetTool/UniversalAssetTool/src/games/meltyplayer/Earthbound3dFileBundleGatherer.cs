using System.Reflection;

using fin.io;
using fin.io.bundles;
using fin.model;
using fin.model.io.importers.gltf;

using SharpGLTF.Schema2;

namespace uni.games.meltyplayer;

public sealed class Earthbound3dUtil {
  public static void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IFileHierarchyDirectory rootDir) {
    var samplerType = typeof(TextureSampler);
    var constructor = samplerType.GetConstructor(
        BindingFlags.NonPublic | BindingFlags.Instance,
        [
            typeof(TextureMipMapFilter),
            typeof(TextureInterpolationFilter),
            typeof(TextureWrapMode),
            typeof(TextureWrapMode)
        ]);

    foreach (var glbFile in rootDir.FilesWithExtensionRecursive(".glb")) {
      organizer.Add(new GltfModelFileBundle(glbFile) {
          AdditionalProcessing = ProcessModel_,
      });
    }
  }

  private static void ProcessModel_(IModel model) {
    foreach (var texture in model.MaterialManager.Textures) {
      texture.MinFilter = TextureMinFilter.NEAR;
      texture.MagFilter = TextureMagFilter.NEAR;
      texture.WrapModeU = texture.WrapModeV = WrapMode.REPEAT;
    }

    foreach (var mesh in model.Skin.Meshes) {
      foreach (var primitive in mesh.Primitives) {
        if (primitive.Material?.Name is "water_shore") {
          primitive.SetInversePriority(3);
        }
      }
    }
  }
}