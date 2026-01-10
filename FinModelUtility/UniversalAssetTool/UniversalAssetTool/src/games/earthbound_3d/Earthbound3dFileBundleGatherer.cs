using System.Reflection;

using fin.io;
using fin.io.bundles;
using fin.model;
using fin.model.io.importers.gltf;
using fin.util.progress;

using SharpGLTF.Schema2;

namespace uni.games.earthbound_3d;

public sealed class Earthbound3dFileBundleGatherer
    : BPrereqsFileBundleGatherer {
  public override string Name => "earthbound_3d";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var root = fileHierarchy.Root;

    var samplerType = typeof(TextureSampler);
    var constructor = samplerType.GetConstructor(
        BindingFlags.NonPublic | BindingFlags.Instance,
        [
            typeof(TextureMipMapFilter),
            typeof(TextureInterpolationFilter),
            typeof(TextureWrapMode),
            typeof(TextureWrapMode)
        ]);

    foreach (var glbFile in root.FilesWithExtensionRecursive(".glb")) {
      organizer.Add(new FlatGltfModelFileBundle(glbFile) {
          AdditionalProcessing = ProcessModel_,
      }.Annotate(glbFile));
    }
  }

  private static void ProcessModel_(IModel model) {
    foreach (var texture in model.MaterialManager.Textures) {
      texture.MinFilter = TextureMinFilter.NEAR;
      texture.MagFilter = TextureMagFilter.NEAR;
      texture.WrapModeU = texture.WrapModeV = WrapMode.REPEAT;
    }

    foreach (var waterShoreMaterial in
             model.MaterialManager.All.Where(m => m.Name is "water_shore")) {
      waterShoreMaterial.DepthCompareType = DepthCompareType.LEqual;
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