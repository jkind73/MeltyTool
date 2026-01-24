using System.Numerics;
using System.Runtime.CompilerServices;

namespace fin.math.matrix.three;

public static class FinMatrix3X2Util {
  public static IReadOnlyFinMatrix3x2 Identity { get; } =
    FromIdentity();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix3X2 FromIdentity()
    => new FinMatrix3X2().SetIdentity();


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix3X2 FromTranslation(Vector2 translation)
    => new FinMatrix3X2(
        SystemMatrix3X2Util.FromTranslation(translation.X, translation.Y));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix3X2 FromTranslation(float x, float y)
    => new FinMatrix3X2(SystemMatrix3X2Util.FromTranslation(x, y));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix3X2 FromRotation(float radians)
    => new FinMatrix3X2(SystemMatrix3X2Util.FromRotation(radians));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix3X2 FromScale(float scale)
    => FromScale(scale, scale);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix3X2 FromScale(Vector2 scale)
    => new FinMatrix3X2(SystemMatrix3X2Util.FromScale(scale.X, scale.Y));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix3X2 FromScale(float scaleX, float scaleY)
    => new FinMatrix3X2(SystemMatrix3X2Util.FromScale(scaleX, scaleY));


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix3X2 FromSkewXRadians(float skewXRadians)
    => new FinMatrix3X2(SystemMatrix3X2Util.FromSkewXRadians(skewXRadians));


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix3X2 FromTrss(Vector2? translation,
                                       float? rotationRadians,
                                       Vector2? scale,
                                       float? skewXRadians)
    => FromTrss(translation, rotationRadians, scale, skewXRadians, new FinMatrix3X2());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IFinMatrix3X2 FromTrss(Vector2? translation,
                                       float? rotationRadians,
                                       Vector2? scale,
                                       float? skewXRadians,
                                       IFinMatrix3X2 dst) {
    dst.CopyFrom(
        SystemMatrix3X2Util.FromTrss(translation,
                                     rotationRadians,
                                     scale,
                                     skewXRadians));
    return dst;
  }
}