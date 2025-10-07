using System.Numerics;

namespace fin.model.skeleton;

public interface IBoneTransformView {
  bool TryGetWorldTranslation(IReadOnlyBone bone, out Vector3 position);
  bool TryGetWorldRotation(IReadOnlyBone bone, out Quaternion rotation);
  bool TryGetWorldScale(IReadOnlyBone bone, out Vector3 scale);

  bool TryGetLocalTranslation(IReadOnlyBone bone, out Vector3 position);
  bool TryGetLocalRotation(IReadOnlyBone bone, out Quaternion rotation);
  bool TryGetLocalScale(IReadOnlyBone bone, out Vector3 scale);
}