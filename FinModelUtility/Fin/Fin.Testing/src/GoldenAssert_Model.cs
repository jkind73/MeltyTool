using fin.io;
using fin.util.asserts;
using fin.util.exceptions;

using SharpGLTF.Schema2;


namespace fin.testing;

public static partial class GoldenAssert {
  private static void AssertModelFilesAreIdentical_(
      IReadOnlyTreeFile lhs,
      IReadOnlyTreeFile rhs) {
    var lhsModel = ParseModelFromBytes_(lhs);
    var rhsModel = ParseModelFromBytes_(rhs);

    AssertAnimationsIdentical_(
        lhsModel.LogicalAnimations,
        rhsModel.LogicalAnimations);

    AssertMaterialsIdentical_(
        lhsModel.LogicalMaterials,
        rhsModel.LogicalMaterials);

    AssertSkinsIdentical_(
        lhsModel.LogicalSkins,
        rhsModel.LogicalSkins);

    AssertMeshesIdentical_(
        lhsModel.LogicalMeshes,
        rhsModel.LogicalMeshes);

    AssertNodesIdentical_(
        lhsModel.LogicalNodes,
        rhsModel.LogicalNodes);

    // TODO: The rest
  }

  private static ModelRoot ParseModelFromBytes_(IReadOnlyTreeFile file) {
    var directory = file.AssertGetParent();
    return ReadContext.Create(f => directory
                                   .AssertGetExistingFile(f)
                                   .ReadAllBytes())
                      .ReadBinarySchema2(file.OpenRead());
  }

  private static void AssertAnimationsIdentical_(
      IReadOnlyList<Animation> lhsAnimations,
      IReadOnlyList<Animation> rhsAnimations) {
    Assert.AreEqual(lhsAnimations.Count, rhsAnimations.Count);
    foreach (var (lhsAnimation, rhsAnimation) in lhsAnimations.Zip(
                 rhsAnimations)) {
      Assert.AreEqual(lhsAnimation.Name, rhsAnimation.Name);
      var animationName = lhsAnimation.Name;

      AnnotatedException.Space(
          $"Found a change in animation {animationName}:\n",
          () => {
            Assert.AreEqual(lhsAnimation.Duration,
                            rhsAnimation.Duration,
                            animationName);

            var lhsChannels = lhsAnimation.Channels;
            var rhsChannels = rhsAnimation.Channels;
            Assert.AreEqual(lhsChannels.Count,
                            rhsChannels.Count,
                            animationName);
            foreach (var (lhsChannel, rhsChannel) in lhsChannels.Zip(
                         rhsChannels)) {
              Assert.AreEqual(lhsChannel.TargetNodePath,
                              rhsChannel.TargetNodePath,
                              animationName);

              switch (lhsChannel.TargetNodePath) {
                case PropertyPath.translation: {
                  AssertChannelsIdentical_(lhsChannel.GetTranslationSampler(),
                                           rhsChannel.GetTranslationSampler(),
                                           animationName);
                  break;
                }
                case PropertyPath.rotation: {
                  AssertChannelsIdentical_(lhsChannel.GetRotationSampler(),
                                           rhsChannel.GetRotationSampler(),
                                           animationName);
                  break;
                }
                case PropertyPath.scale: {
                  AssertChannelsIdentical_(lhsChannel.GetScaleSampler(),
                                           rhsChannel.GetScaleSampler(),
                                           animationName);
                  break;
                }
                default:
                  throw new NotImplementedException(
                      $"{nameof(lhsChannel.TargetNodePath)}: {lhsChannel.TargetNodePath}");
              }
            }
          });
    }
  }

  private static void AssertChannelsIdentical_<T>(
      IAnimationSampler<T> lhs,
      IAnimationSampler<T> rhs,
      string animationName) {
    Assert.AreEqual(lhs.InterpolationMode,
                    rhs.InterpolationMode,
                    animationName);

    switch (lhs.InterpolationMode) {
      case AnimationInterpolationMode.STEP:
      case AnimationInterpolationMode.LINEAR: {
        Asserts.SequenceEqual(lhs.GetLinearKeys(), rhs.GetLinearKeys());
        break;
      }
      case AnimationInterpolationMode.CUBICSPLINE: {
        Asserts.SequenceEqual(lhs.GetCubicKeys(), rhs.GetCubicKeys());
        break;
      }
      default: throw new NotImplementedException(nameof(lhs.InterpolationMode));
    }
  }

  private static void AssertMaterialsIdentical_(
      IReadOnlyList<Material> lhsMaterials,
      IReadOnlyList<Material> rhsMaterials) {
    Assert.AreEqual(lhsMaterials.Count, rhsMaterials.Count);

    foreach (var (lhsMaterial, rhsMaterial) in lhsMaterials.Zip(
                 rhsMaterials)) {
      Assert.AreEqual(lhsMaterial.Name, rhsMaterial.Name);
      var materialName = lhsMaterial.Name;

      // TODO: The rest
    }
  }

  private static void AssertSkinsIdentical_(
      IReadOnlyList<Skin> lhsSkins,
      IReadOnlyList<Skin> rhsSkins) {
    Assert.AreEqual(lhsSkins.Count, rhsSkins.Count);

    foreach (var (lhsSkin, rhsSkin) in lhsSkins.Zip(rhsSkins)) {
      Assert.AreEqual(lhsSkin.Name, rhsSkin.Name);
      var skinName = lhsSkin.Name;

      AnnotatedException.Space(
          $"Found a change in skin {skinName}:\n",
          () => {
            Assert.AreEqual(lhsSkin.InverseBindMatrices.Count,
                            rhsSkin.InverseBindMatrices.Count);
            foreach (var (lhsInverseBindMatrix, rhsInverseBindMatrix) in
                     lhsSkin.InverseBindMatrices.Zip(
                         rhsSkin.InverseBindMatrices)) {
              Asserts.Equal(lhsInverseBindMatrix,
                            rhsInverseBindMatrix,
                            skinName);
            }

            Assert.AreEqual(lhsSkin.Joints.Count, rhsSkin.Joints.Count);
            foreach (var (lhsJoint, rhsJoint) in lhsSkin.Joints.Zip(
                         rhsSkin.Joints)) {
              Asserts.Equal(lhsJoint.Name, rhsJoint.Name, skinName);
              var jointName = lhsJoint.Name;

              Asserts.Equal(lhsJoint.LocalMatrix,
                            rhsJoint.LocalMatrix,
                            jointName);
            }
          });
    }
  }

  private static void AssertMeshesIdentical_(
      IReadOnlyList<Mesh> lhsMeshes,
      IReadOnlyList<Mesh> rhsMeshes) {
    Assert.AreEqual(lhsMeshes.Count, rhsMeshes.Count);

    foreach (var (lhsMesh, rhsMesh) in lhsMeshes.Zip(rhsMeshes)) {
      Assert.AreEqual(lhsMesh.Name, rhsMesh.Name);

      AnnotatedException.Space(
          $"Found a change in mesh {lhsMesh.Name}:\n",
          () => {
            Assert.AreEqual(lhsMesh.Primitives.Count, rhsMesh.Primitives.Count);
            foreach (var (lhsPrimitive, rhsPrimitive) in lhsMesh.Primitives.Zip(
                         rhsMesh.Primitives)) {
              var lhsMaterial = lhsPrimitive.Material;
              var rhsMaterial = rhsPrimitive.Material;

              if (!lhsMaterial.Equals(rhsMaterial)) {
                Assert.AreEqual(lhsMaterial.Name, rhsMaterial.Name);
                Assert.AreEqual(lhsMaterial.Unlit, rhsMaterial.Unlit);
                Assert.AreEqual(lhsMaterial.Alpha, rhsMaterial.Alpha);
                Assert.AreEqual(lhsMaterial.AlphaCutoff, rhsMaterial.AlphaCutoff);
                Assert.AreEqual(lhsMaterial.DoubleSided, rhsMaterial.DoubleSided);

                var lhsDiffuseTexture = lhsMaterial.GetDiffuseTexture();
                var rhsDiffuseTexture = rhsMaterial.GetDiffuseTexture();

                var lhsSampler = lhsDiffuseTexture.Sampler;
                var rhsSampler = rhsDiffuseTexture.Sampler;

                Assert.AreEqual(lhsSampler.Name, rhsSampler.Name);
                Assert.AreEqual(lhsSampler.MagFilter, rhsSampler.MagFilter);
                Assert.AreEqual(lhsSampler.MinFilter, rhsSampler.MinFilter);
                Assert.AreEqual(lhsSampler.WrapS, rhsSampler.WrapS);
                Assert.AreEqual(lhsSampler.WrapT, rhsSampler.WrapT);
                Assert.AreEqual(lhsSampler.Extras, rhsSampler.Extras);
                foreach (var (lhsExtension, rhsExtension) in lhsSampler
                             .Extensions.Zip(rhsSampler.Extensions)) {
                  Assert.AreEqual(lhsExtension, rhsExtension);
                }
                Assert.AreEqual(lhsSampler, rhsSampler);
                Assert.AreEqual(lhsDiffuseTexture.PrimaryImage, rhsDiffuseTexture.PrimaryImage);
                Assert.AreEqual(lhsDiffuseTexture, rhsDiffuseTexture);

                Assert.AreEqual(lhsMaterial.Channels, rhsMaterial.Channels);

                Assert.Fail(
                    $"Found a change in material from {lhsMaterial} to {rhsMaterial}, but not sure where.");
              }

              Asserts.SequenceEqual(lhsPrimitive.IndexAccessor.AsIndicesArray(),
                                    rhsPrimitive.IndexAccessor
                                                .AsIndicesArray());
            }

            // TODO: The rest
          });
    }
  }

  private static void AssertNodesIdentical_(
      IReadOnlyList<Node> lhsNodes,
      IReadOnlyList<Node> rhsNodes) {
    Assert.AreEqual(lhsNodes.Count, rhsNodes.Count);

    foreach (var (lhsNode, rhsNode) in lhsNodes.Zip(rhsNodes)) {
      Assert.AreEqual(lhsNode.Name, rhsNode.Name);
      var nodeName = lhsNode.Name;

      Asserts.Equal(lhsNode.LocalMatrix, rhsNode.LocalMatrix, nodeName);
    }
  }
}