using BenchmarkDotNet.Attributes;

using fin.math.rotations;

namespace benchmarks;

public sealed class Radians {
  private const int N_ = 100000;

  public const float LHS = 1;
  public const float RHS = 2;

  public const float PI2 = MathF.PI * 2;
  public const float PI3 = MathF.PI * 3;


  [Benchmark]
  public void Inline() {
    for (var i = 0; i < N_; i++) {
      var radians = ((((LHS - RHS) % PI2) + PI3) % PI2) - MathF.PI;
    }
  }

  [Benchmark]
  public void StaticCall() {
    for (var i = 0; i < N_; i++) {
      var radians =
          RadiansUtil.CalculateRadiansTowards(LHS, RHS);
    }
  }

  private readonly ReadonlyRadiansFloat lhsStruct_ = new(1);
  private readonly ReadonlyRadiansFloat rhsStruct_ = new(2);

  [Benchmark]
  public void ReadonlyStruct() {
    for (var i = 0; i < N_; i++) {
      var radians = this.lhsStruct_ - this.rhsStruct_;
    }
  }


  [Benchmark]
  public void ReadonlyStructEach() {
    for (var i = 0; i < N_; i++) {
      var lhs = new ReadonlyRadiansFloat(1);
      var rhs = new ReadonlyRadiansFloat(2);

      var radians = lhs - rhs;
    }
  }

  [Benchmark]
  public void StructEach() {
    for (var i = 0; i < N_; i++) {
      var lhs = new RadiansFloat(1);
      var rhs = new RadiansFloat(2);

      var radians = lhs - rhs;
    }
  }


  public readonly struct ReadonlyRadiansFloat(float radians) {
    private const float PI2_ = MathF.PI * 2;
    private const float PI3_ = MathF.PI * 3;

    private readonly float radians_ = (((radians % PI2_) + PI3_) % PI2_) - MathF.PI;

    public ReadonlyRadiansFloat Add(ReadonlyRadiansFloat other)
      => new(this.radians_ + other.radians_);

    public ReadonlyRadiansFloat Subtract(ReadonlyRadiansFloat other)
      => new(this.radians_ - other.radians_);

    public static ReadonlyRadiansFloat operator -(ReadonlyRadiansFloat value)
      => new(-value.radians_);

    public static ReadonlyRadiansFloat operator +(ReadonlyRadiansFloat lhs, ReadonlyRadiansFloat rhs)
      => lhs.Add(rhs);

    public static ReadonlyRadiansFloat operator -(ReadonlyRadiansFloat lhs, ReadonlyRadiansFloat rhs)
      => lhs.Subtract(rhs);
  }

  public struct RadiansFloat(float radians) {
    private const float PI2_ = MathF.PI * 2;
    private const float PI3_ = MathF.PI * 3;

    private readonly float radians_ = (((radians % PI2_) + PI3_) % PI2_) - MathF.PI;

    public RadiansFloat Add(RadiansFloat other)
      => new(this.radians_ + other.radians_);

    public RadiansFloat Subtract(RadiansFloat other)
      => new(this.radians_ - other.radians_);

    public static RadiansFloat operator -(RadiansFloat value)
      => new(-value.radians_);

    public static RadiansFloat operator +(RadiansFloat lhs, RadiansFloat rhs)
      => lhs.Add(rhs);

    public static RadiansFloat operator -(RadiansFloat lhs, RadiansFloat rhs)
      => lhs.Subtract(rhs);
  }
}