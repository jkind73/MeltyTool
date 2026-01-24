using System.Numerics;

using BenchmarkDotNet.Attributes;

using fin.math.rotations;

using MathNet.Numerics;

namespace benchmarks;

public sealed class QuaternionCreation {
  private const int N_ = 1000;

  private const float X_RADIANS_ = 0;
  private const float Y_RADIANS_ = 0;
  private const float Z_RADIANS_ = 0;





  [GlobalSetup]
  public void Setup() {
    FinTrig.Cos(0);
  }


  [Benchmark]
  public void CheckViaIfs() {
    for (var i = 0; i < N_; i++) {
      var q = Quaternion.Identity;

      if (!X_RADIANS_.AlmostEqual(0, .001)) {
        q *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, X_RADIANS_);
      }

      if (!Y_RADIANS_.AlmostEqual(0, .001)) {
        q *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, Y_RADIANS_);
      }

      if (!Z_RADIANS_.AlmostEqual(0, .001)) {
        q *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, Z_RADIANS_);
      }

      q = Quaternion.Normalize(q);
    }
  }

  [Benchmark]
  public void NoChecks() {
    for (var i = 0; i < N_; i++) {
      var q = Quaternion.Identity;

      q *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, X_RADIANS_);
      q *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, Y_RADIANS_);
      q *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, Z_RADIANS_);

      q = Quaternion.Normalize(q);
    }
  }

  [Benchmark]
  public void NoTemp() {
    for (var i = 0; i < N_; i++) {
      var q = Quaternion.Normalize(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, X_RADIANS_) *
                                   Quaternion.CreateFromAxisAngle(Vector3.UnitY, Y_RADIANS_) *
                                   Quaternion.CreateFromAxisAngle(Vector3.UnitX, Z_RADIANS_));
    }
  }

  [Benchmark]
  public void Manually() {
    for (var i = 0; i < N_; i++) {
      var cr = FinTrig.Cos(X_RADIANS_ * 0.5f);
      var sr = FinTrig.Sin(X_RADIANS_ * 0.5f);
      var cp = FinTrig.Cos(Y_RADIANS_ * 0.5f);
      var sp = FinTrig.Sin(Y_RADIANS_ * 0.5f);
      var cy = FinTrig.Cos(Z_RADIANS_ * 0.5f);
      var sy = FinTrig.Sin(Z_RADIANS_ * 0.5f);

      var q = new Quaternion(
          sr * cp * cy - cr * sp * sy,
          cr * sp * cy + sr * cp * sy,
          cr * cp * sy - sr * sp * cy,
          cr * cp * cy + sr * sp * sy);
    }
  }
}