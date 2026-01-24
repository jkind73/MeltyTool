using BenchmarkDotNet.Attributes;

namespace benchmarks;

public sealed class TrimmingStrings {
  private const string TEXT_ =
      "sample foobar text, another line\nhere's some more text\0and there's more content after this\0";
  private readonly int n_ = 100000;



  [Benchmark]
  public void UsingTrimEnd() {
    for (var i = 0; i < this.n_; ++i) {
      var substring = TEXT_.TrimEnd('\0');
    }
  }

  [Benchmark]
  public void UsingIndexOfAndSubstring() {
    for (var i = 0; i < this.n_; ++i) {
      var substring =
          TEXT_[TEXT_.IndexOf('\0')..];
    }
  }
}