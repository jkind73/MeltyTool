using fin.io;
using fin.util.asserts;

using Microsoft.VisualStudio.TestTools.UnitTesting;

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

      Assert.AreEqual(lhsAnimation.Duration,
                      rhsAnimation.Duration,
                      animationName);

      var lhsChannels = lhsAnimation.Channels;
      var rhsChannels = rhsAnimation.Channels;
      Assert.AreEqual(lhsChannels.Count, rhsChannels.Count, animationName);
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
}