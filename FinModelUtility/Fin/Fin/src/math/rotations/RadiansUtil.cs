using System;
using System.Runtime.CompilerServices;

using fin.math.floats;

namespace fin.math.rotations;

public static class RadiansUtil {
  private const float PI_ = MathF.PI;
  private const float PI2_ = 2 * PI_;
  private const float PI3_ = 3 * PI_;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float CalculateRadiansTowards(float from, float to) {
    var difference = to - from;
    if (Math.Abs(difference).IsRoughly(PI_)) {
      return difference;
    }

    return (((difference % PI2_) + PI3_) % PI2_) - PI_;
  }
}