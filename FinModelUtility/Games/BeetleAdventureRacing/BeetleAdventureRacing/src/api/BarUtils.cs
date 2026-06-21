using System.Numerics;

namespace bar.api;

public static class BarUtils {
  public static readonly Matrix4x4 ROOT_MATRIX = Matrix4x4.CreateFromQuaternion(
      Quaternion.CreateFromYawPitchRoll(0, -MathF.PI / 2, 0));
}