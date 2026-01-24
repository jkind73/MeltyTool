using BenchmarkDotNet.Attributes;

namespace benchmarks;

public sealed class Finite {
  private const int N_ = 10000000;

  [Benchmark]
  public void IsFiniteMethod() {
    for (var i = 0; i < N_; i++) {
      var f = 1f * i;
      var a = float.IsFinite(f);
    }
  }

  [Benchmark]
  public void IsFiniteManual() {
    for (var i = 0; i < N_; i++) {
      var f = 1f * i;
      var a = float.IsNaN(f) && float.IsInfinity(f);
    }
  }
}