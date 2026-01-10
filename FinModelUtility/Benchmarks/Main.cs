using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;

namespace benchmarks {
  public sealed class Program {
    public static void Main(string[] args) {
      var summary = BenchmarkRunner.Run<IndexableDictionaries>(
          ManualConfig.Create(DefaultConfig.Instance)
                      .AddDiagnoser(MemoryDiagnoser.Default)
                      .WithOptions(ConfigOptions
                                       .DisableOptimizationsValidator));
    }
  }
}