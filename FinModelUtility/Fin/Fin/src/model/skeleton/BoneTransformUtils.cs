using System.Numerics;

using fin.math.matrix.four;

namespace fin.model.skeleton;

public static class BoneTransformUtils {
  public static Matrix4x4 CalculateBoneToWorldMatrix(
      in Matrix4x4 parent,
      IBoneTransformView boneTransformView) {
    boneTransformView.TryGetLocalTranslation(out var localTranslation);
    boneTransformView.TryGetLocalRotation(out var localRotation);
    boneTransformView.TryGetLocalScale(out var localScale);

    var boneToWorldMatrix = Matrix4x4.Multiply(
        SystemMatrix4x4Util.FromTrs(localTranslation,
                                    localRotation,
                                    localScale),
        parent);

    var hasWorldTranslation = boneTransformView.TryGetWorldTranslation(
        out var overrideWorldTranslation);
    var hasWorldRotation = boneTransformView.TryGetWorldRotation(
        out var overrideWorldRotation);
    var hasWorldScale = boneTransformView.TryGetWorldScale(
        out var overrideWorldScale);

    if (!hasWorldTranslation && !hasWorldRotation && !hasWorldScale) {
      return boneToWorldMatrix;
    }

    // TODO: In this case, this matrix needs to first invert the model matrix
    // to be able to properly set world-space transforms
    Matrix4x4.Decompose(boneToWorldMatrix,
                        out var worldScale,
                        out var worldRotation,
                        out var worldTranslation);

    if (hasWorldTranslation) {
      worldTranslation = overrideWorldTranslation;
    }

    if (hasWorldRotation) {
      worldRotation = overrideWorldRotation;
    }

    if (hasWorldScale) {
      worldScale = overrideWorldScale;
    }

    return SystemMatrix4x4Util.FromTrs(worldTranslation,
                                       worldRotation,
                                       worldScale);
  }
}