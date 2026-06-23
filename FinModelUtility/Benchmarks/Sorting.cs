using BenchmarkDotNet.Attributes;

namespace benchmarks;

public class Sorting {
  private const int VALUE_COUNT_ = 100000;

  public class A : IComparable<A> {
    public int Value { get; set; }
    public int CompareTo(A? other) => this.Value.CompareTo(other.Value);
  }

  public class AComparer : IComparer<A> {
    public int Value { get; set; }
    public int Compare(A? x, A? y) => x.Value.CompareTo(y.Value);
  }

  private static readonly A[] VALUES_
      = Enumerable.Range(0, VALUE_COUNT_)
                  .Select(_ => new A { Value = Random.Shared.Next() })
                  .ToArray();

  private static readonly int[] KEYS_ = VALUES_.Select(v => v.Value).ToArray();

  private readonly AComparer comparer_ = new();

  private readonly A[] iterationValues_ = new A[VALUE_COUNT_];
  private readonly int[] iterationKeys_ = new int[VALUE_COUNT_];

  private readonly A[] sortedIterationValues_ = new A[VALUE_COUNT_];
  private readonly int[] sortedIterationKeys_ = new int[VALUE_COUNT_];


  [IterationSetup]
  public void CopyIterationValues() {
    VALUES_.CopyTo(this.iterationValues_);
    KEYS_.CopyTo(this.iterationKeys_);

    VALUES_.CopyTo(this.sortedIterationValues_);
    this.sortedIterationValues_.Sort();
    KEYS_.CopyTo(this.sortedIterationKeys_);
    this.sortedIterationKeys_.Sort();
  }

  [Benchmark]
  public void SimplestSort() => this.iterationValues_.Sort();

  [Benchmark]
  public void SimplestSortWithComparer()
    => this.iterationValues_.Sort(this.comparer_);

  [Benchmark]
  public void ArraySort()
    => Array.Sort(this.iterationValues_, this.comparer_);

  [Benchmark]
  public void SortByKeys()
    => Array.Sort(this.iterationKeys_, this.iterationValues_);


  [Benchmark]
  public void AlreadySorted_SimplestSort() => this.sortedIterationValues_.Sort();

  [Benchmark]
  public void AlreadySorted_SimplestSortWithComparer()
    => this.sortedIterationValues_.Sort(this.comparer_);

  [Benchmark]
  public void AlreadySorted_ArraySort()
    => Array.Sort(this.sortedIterationValues_, this.comparer_);

  [Benchmark]
  public void AlreadySorted_SortByKeys()
    => Array.Sort(this.sortedIterationKeys_, this.sortedIterationValues_);

  [Benchmark]
  public void CheckingIfAlreadySorted_Sorted_Array_Index() {
    for (var i = 1; i < this.sortedIterationValues_.Length; ++i) {
      if (this.comparer_.Compare(this.sortedIterationValues_[i - 1],
                                 this.sortedIterationValues_[i]) > 0) {
        return;
      }
    }
  }

  [Benchmark]
  public void CheckingIfAlreadySorted_Sorted_Span_Index() {
    var span = this.sortedIterationValues_.AsSpan();
    for (var i = 1; i < span.Length; ++i) {
      if (this.comparer_.Compare(span[i - 1], span[i]) > 0) {
        return;
      }
    }
  }

  [Benchmark]
  public unsafe void CheckingIfAlreadySorted_Sorted_Pointer_Index() {
    fixed (A* ptr = &this.sortedIterationValues_[0]) {
      for (var i = 1; i < this.sortedIterationValues_.Length; ++i) {
        if (this.comparer_.Compare(ptr[i - 1], ptr[i]) > 0) {
          return;
        }
      }
    }
  }
}