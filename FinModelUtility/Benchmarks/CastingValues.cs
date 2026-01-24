using BenchmarkDotNet.Attributes;

namespace benchmarks;

public sealed class CastingValues {
  private const int N_ = 100000000;

  [Benchmark]
  public unsafe void ViaPointer() {
    for (var i = 0; i < N_; i++) {
      ulong value = 123456;
      double castedValue = *(double*) (&value);
    }
  }

  [Benchmark]
  public void ViaBitConverter() {
    for (var i = 0; i < N_; i++) {
      ulong value = 123456;
      double castedValue = BitConverter.UInt64BitsToDouble(value);
    }
  }
}