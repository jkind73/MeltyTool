using System.Numerics;

namespace fin.model.skeleton;

public sealed class RootBoneTransformView : IBoneTransformView {
  private IReadOnlyBone? bone_;

  public void TargetBone(IReadOnlyBone bone) => this.bone_ = bone;

  public bool TryGetWorldTranslation(out Vector3 translation) {
    translation = default;
    return false;
  }

  public bool TryGetWorldRotation(out Quaternion rotation) {
    rotation = default;
    return false;
  }

  public bool TryGetWorldScale(out Vector3 scale) {
    scale = default;
    return false;
  }

  public bool TryGetLocalTranslation(out Vector3 translation) {
    if (this.bone_ == null) {
      translation = default;
      return false;
    }

    translation = this.bone_.LocalTransform.Translation;
    return true;
  }

  public bool TryGetLocalRotation(out Quaternion rotation) {
    if (this.bone_ == null) {
      rotation = default;
      return false;
    }

    rotation = this.bone_.LocalTransform.Rotation ?? Quaternion.Identity;
    return true;
  }

  public bool TryGetLocalScale(out Vector3 scale) {
    if (this.bone_ == null) {
      scale = default;
      return false;
    }

    scale = this.bone_.LocalTransform.Scale ?? Vector3.One;
    return true;
  }
}