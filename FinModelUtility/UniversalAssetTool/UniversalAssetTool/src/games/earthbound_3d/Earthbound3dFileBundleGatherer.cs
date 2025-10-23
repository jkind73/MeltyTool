using System.Reflection;

using fin.io;
using fin.io.bundles;
using fin.model.io.importers.gltf;
using fin.util.asserts;
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

    var defaultSampler = constructor.Invoke([
        TextureMipMapFilter.NEAREST,
        TextureInterpolationFilter.NEAREST,
        TextureWrapMode.REPEAT,
        TextureWrapMode.REPEAT
    ]).AssertAsA<TextureSampler>();

    foreach (var glbFile in root.FilesWithExtensionRecursive(".glb")) {
      organizer.Add(new GltfModelFileBundle(glbFile) {
          DefaultSampler = defaultSampler,
      }.Annotate(glbFile));
    }
  }
}