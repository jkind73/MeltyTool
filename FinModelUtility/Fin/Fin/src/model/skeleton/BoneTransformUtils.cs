using System.Numerics;

using fin.math.matrix.four;

namespace fin.model.skeleton;

public static class BoneTransformUtils {
  public static Matrix4x4 CalculateBoneToWorldMatrix(
      IBoneTransformView boneTransformView,
      in Matrix4x4 parent,
      in Matrix4x4 modelMatrix) {
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

    // TODO: Does this stuff make sense, using the model matrix here???
    var filteredModelMatrix =
        modelMatrix.FilterTrs(hasWorldTranslation,
                              hasWorldRotation,
                              hasWorldScale);

    var boneToWorldMatrixWithWorldMatrix
        = boneToWorldMatrix * filteredModelMatrix;
    Matrix4x4.Decompose(boneToWorldMatrixWithWorldMatrix,
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

    Matrix4x4.Invert(filteredModelMatrix, out var invertedModelMatrix);

    return SystemMatrix4x4Util.FromTrs(
               worldTranslation,
               worldRotation,
               worldScale) *
           invertedModelMatrix;
  }
}