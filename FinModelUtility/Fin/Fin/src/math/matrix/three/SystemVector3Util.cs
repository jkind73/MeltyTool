using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

using fin.math.floats;
using fin.math.rotations;
using fin.util.hash;


namespace fin.math.matrix.three;

public static class SystemVector3Util {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe bool IsRoughly(this in Vector3 lhs, in Vector3 rhs) {
    fixed (float* lhsPtr = &lhs.X) {
      fixed (float* rhsPtr = &rhs.X) {
        for (var i = 0; i < 3; ++i) {
          if (!lhsPtr[i].IsRoughly(rhsPtr[i])) {
            return false;
          }
        }
      }
    }

    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int GetRoughHashCode(this in Vector3 v)
    => FluentHash.Start()
                 .With(v.X.GetRoughHashCode())
                 .With(v.Y.GetRoughHashCode())
                 .With(v.Z.GetRoughHashCode());

  public static Vector3 FromPitchYawRadians(float pitchRadians,
                                            float yawRadians) {
    FinTrig.FromPitchYawRadians(pitchRadians,
                                yawRadians,
                                out var x,
                                out var y,
                                out var z);
    return new Vector3(x, y, z);
  }

  public static Vector3 Average<T>(this IEnumerable<T> src,
                                   Func<T, Vector3> selector)
    => src.Select(selector).Average();

  public static Vector3 Average(this IEnumerable<Vector3> src) {
    var count = 0;
    Vector3 total = default;
    foreach (var vec3 in src) {
      count++;
      total += vec3;
    }

    if (count != 0) {
      total /= count;
    }

    return total;
  }
}