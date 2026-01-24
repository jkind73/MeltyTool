using BenchmarkDotNet.Attributes;

using fin.data.indexable;

namespace benchmarks;

public struct Indexable : IIndexable {
  public required int Index { get; init; }
}

public class IndexableDictionaries {
  private const int x = 10000;
  private const int n = 1000;

  private readonly IndexableDictionary<Indexable, int> indexableV1_ = new(x);
  private readonly IndexableDictionaryV2<Indexable, int> indexableV2_ = new(x);

  [GlobalSetup]
  public void SetUp() {
    for (var i = 0; i < x; i++) {
      this.indexableV1_[i] = i;
      this.indexableV2_[i] = i;
    }
  }

  [Benchmark]
  public void AddWithoutCapacityV1() {
    for (var iteration = 0; iteration < n; ++iteration) {
      var impl = new IndexableDictionary<Indexable, int>();
      for (var i = 0; i < x; i++) {
        impl[i] = i;
      }
    }
  }

  [Benchmark]
  public void AddWithoutCapacityV2() {
    for (var iteration = 0; iteration < n; ++iteration) {
      var impl = new IndexableDictionaryV2<Indexable, int>();
      for (var i = 0; i < x; i++) {
        impl[i] = i;
      }
    }
  }

  [Benchmark]
  public void AddWithCapacityV1() {
    for (var iteration = 0; iteration < n; ++iteration) {
      var impl = new IndexableDictionary<Indexable, int>(x);
      for (var i = 0; i < x; i++) {
        impl[i] = i;
      }
    }
  }

  [Benchmark]
  public void AddWithCapacityV2() {
    for (var iteration = 0; iteration < n; ++iteration) {
      var impl = new IndexableDictionaryV2<Indexable, int>(x);
      for (var i = 0; i < x; i++) {
        impl[i] = i;
      }
    }
  }

  [Benchmark]
  public void EnumerateV1() {
    for (var iteration = 0; iteration < n; ++iteration) {
      foreach (var value in this.indexableV1_) {
        var foo = value;
      }
    }
  }

  [Benchmark]
  public void EnumerateV2() {
    for (var iteration = 0; iteration < n; ++iteration) {
      foreach (var value in this.indexableV2_) {
        var foo = value;
      }
    }
  }

  [Benchmark]
  public void TryGetValueV1() {
    for (var iteration = 0; iteration < n; ++iteration) {
      for (var i = 0; i < x; i++) {
        if (this.indexableV1_.TryGetValue(i, out var value)) {
          var foo = value;
        }
      }
    }
  }

  [Benchmark]
  public void TryGetValueV2() {
    for (var iteration = 0; iteration < n; ++iteration) {
      for (var i = 0; i < x; i++) {
        if (this.indexableV2_.TryGetValue(i, out var value)) {
          var foo = value;
        }
      }
    }
  }
}