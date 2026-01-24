using System.Numerics;
using System.Runtime.CompilerServices;

using fin.math.rotations;
using fin.model;

namespace fin.math.matrix.four;

public static class FinMatrix4X4Util {
  public static IReadOnlyFinMatrix4x4 Identity { get; } =
    FromIdentity();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix4X4 FromIdentity()
    => new FinMatrix4X4().SetIdentity();


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix4X4 FromTranslation(in Vector3 translation)
    => FromTranslation(
        translation.X,
        translation.Y,
        translation.Z);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix4X4 FromTranslation(float x, float y, float z)
    => new FinMatrix4X4(SystemMatrix4X4Util.FromTranslation(x, y, z));


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix4X4 FromRotation(IRotation rotation)
    => FromRotation(QuaternionUtil.Create(rotation));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix4X4 FromRotation(in Quaternion rotation)
    => new FinMatrix4X4(SystemMatrix4X4Util.FromRotation(rotation));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix4X4 FromScale(in Vector3 scale)
    => FromScale(scale.X, scale.Y, scale.Z);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix4X4 FromScale(float scale)
    => FromScale(scale, scale, scale);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix4X4 FromScale(float scaleX,
                                        float scaleY,
                                        float scaleZ)
    => new FinMatrix4X4(
        SystemMatrix4X4Util.FromScale(scaleX, scaleY, scaleZ));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix4X4 FromTrs(
      in Vector3? translation,
      in Quaternion? rotation,
      in Vector3? scale)
    => FromTrs(translation, rotation, scale, new FinMatrix4X4());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix4X4 FromTrs(
      in Vector3? translation,
      in Quaternion? rotation,
      in Vector3? scale,
      IFinMatrix4X4 dst) {
    dst.CopyFrom(SystemMatrix4X4Util.FromTrs(translation, rotation, scale));
    return dst;
  }
}