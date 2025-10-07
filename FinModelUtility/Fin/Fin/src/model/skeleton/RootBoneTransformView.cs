using System.Numerics;

namespace fin.model.skeleton;

public class RootBoneTransformView : IBoneTransformView {
  public bool TryGetWorldTranslation(IReadOnlyBone bone, out Vector3 position) {
    position = default;
    return false;
  }

  public bool TryGetWorldRotation(IReadOnlyBone bone, out Quaternion rotation) {
    rotation = default;
    return false;
  }

  public bool TryGetWorldScale(IReadOnlyBone bone, out Vector3 scale) {
    scale = default;
    return false;
  }

  public bool TryGetLocalTranslation(IReadOnlyBone bone, out Vector3 position) {
    position = bone.LocalTransform.Translation;
    return true;
  }

  public bool TryGetLocalRotation(IReadOnlyBone bone, out Quaternion rotation) {
    rotation = bone.LocalTransform.Rotation ?? Quaternion.Identity;
    return true;
  }

  public bool TryGetLocalScale(IReadOnlyBone bone, out Vector3 scale) {
    scale = bone.LocalTransform.Scale ?? Vector3.One;
    return true;
  }
}