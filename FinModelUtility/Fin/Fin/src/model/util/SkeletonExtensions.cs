using System;
using System.Numerics;

using fin.math.matrix.four;
using fin.math.rotations;
using fin.math.xyz;
using fin.ui;
using fin.util.asserts;

using Microsoft.CodeAnalysis;

namespace fin.model.util;

public static class SkeletonExtensions {
  public static IBone AddChild(this IBone parent, Vector3 position)
    => parent.AddChild(position.X, position.Y, position.Z);

  public static IBone AddChild(this IBone parent, IReadOnlyXyz position)
    => parent.AddChild(position.X, position.Y, position.Z);

  public static IBone AddChild(this IBone parent,
                               IReadOnlyFinMatrix4x4 matrix)
    => parent.AddChild(matrix.Impl);

  public static IBone AddChild(this IBone parent, Matrix4x4 matrix) {
    matrix.AssertDecompose(out var translation,
                           out var rotation,
                           out var scale);

    var child = parent.AddChild(translation);
    child.LocalTransform.Rotation = rotation;
    child.LocalTransform.Scale = scale;

    return child;
  }

  public static Matrix4x4 GetWorldMatrix(this IBone bone) {
    var currentMatrix = Matrix4x4.Identity;
    while (bone != null) {
      currentMatrix = bone.LocalTransform.Matrix * currentMatrix;
      bone = bone.Parent;
    }

    return currentMatrix;
  }

  public static bool TryGetFaceCameraQuaternion(
      this IReadOnlyBone? bone,
      out Quaternion rotation) {
    var faceTowardsCameraType
        = bone?.FaceTowardsCameraType ?? FaceTowardsCameraType.NONE;
    if (faceTowardsCameraType != FaceTowardsCameraType.NONE) {
      var camera = Camera.Instance;
      rotation = Quaternion.CreateFromAxisAngle(
              Vector3.UnitZ,
              camera.YawDegrees * FinTrig.DEG_2_RAD - MathF.PI / 2);
      if (faceTowardsCameraType is FaceTowardsCameraType.YAW_AND_PITCH) {
        rotation *= Quaternion.CreateFromAxisAngle(
            Vector3.UnitX,
            camera.PitchDegrees * FinTrig.DEG_2_RAD + MathF.PI / 2);
      } else {
        rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI / 2);
      }
      return true;
    }

    rotation = default;
    return false;
  }
}