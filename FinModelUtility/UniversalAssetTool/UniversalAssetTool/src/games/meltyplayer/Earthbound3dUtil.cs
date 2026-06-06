using System.Reflection;

using fin.io;
using fin.io.bundles;
using fin.math.matrix.three;
using fin.math.rotations;
using fin.model;
using fin.model.io.importers.gltf;
using fin.model.util;

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
    foreach (var material in model.MaterialManager.All) {
      material.IgnoreLights = true;

      if (material.Name == "water") {
        material.DepthCompareType = DepthCompareType.Less;
      }
    }

    foreach (var texture in model.MaterialManager.Textures) {
      texture.MinFilter = TextureMinFilter.NEAR;
      texture.MagFilter = TextureMagFilter.NEAR;
      texture.WrapModeU = texture.WrapModeV = WrapMode.REPEAT;
    }

    foreach (var mesh in model.Skin.Meshes) {
      var isFlower
          = mesh.Primitives.All(p => p.Material?.Name?.StartsWith(
                                         "decoration_flower") ??
                                     false);

      if (isFlower) {
        var flowers = mesh.Primitives.SelectMany(p => p.Vertices).Chunk(6);
        foreach (var flower in flowers) {
          var vertices = flower.Distinct().Cast<IVertex>().ToArray();

          var bone = model.Skeleton.Root.AddChild(
              vertices.Average(v => v.LocalPosition));
          bone.AlwaysFaceTowardsCamera(
              FaceTowardsCameraType.YAW_ONLY,
              QuaternionUtil.CreateZyxRadians(
                  -MathF.PI / 2,
                  0,
                  0));
          var boneWeights
              = model.Skin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_WORLD,
                                                  bone);

          foreach (var vertex in vertices) {
            vertex.SetBoneWeights(boneWeights);
          }
        }
      }

      foreach (var primitive in mesh.Primitives) {
        if (primitive.Material?.Name is "water_shore") {
          primitive.SetInversePriority(3);
        }
      }
    }
  }
}