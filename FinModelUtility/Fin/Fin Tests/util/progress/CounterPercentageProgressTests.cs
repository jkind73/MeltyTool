using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.util.progress;

public sealed class CounterPercentageProgressTests {
  private const int TOTAL_ = 104;

  [Test]
  public void TestDefaultProgress() {
    var progress = new CounterPercentageProgress(TOTAL_);
    Assert.AreEqual(0, progress.Progress);
  }

  [Test]
  public void TestIncrementalProgress() {
    var progress = new CounterPercentageProgress(TOTAL_);

    var actualProgressChanges = new List<float>();
    progress.OnProgressChanged += (sender, f) => actualProgressChanges.Add(f);

    progress.Increment();
    CollectionAssert.AreEqual(new [] {1f / TOTAL_}, actualProgressChanges);
    Assert.AreEqual(1f / TOTAL_, progress.Progress);
    
    progress.Increment();
    CollectionAssert.AreEqual(new [] {1f / TOTAL_, 2f / TOTAL_}, actualProgressChanges);
    Assert.AreEqual(2f / TOTAL_, progress.Progress);
  }

  [Test]
  public void TestDoesNotUpdateAfterComplete() {
    var progress = new CounterPercentageProgress(TOTAL_);

    var progressChangedCallCount = 0;
    var completeCallCount = 0;
    progress.OnProgressChanged += (sender, f) => ++progressChangedCallCount;
    progress.OnComplete += (sender, f) => ++completeCallCount;

    for (var i = 0; i < TOTAL_ - 1; ++i) {
      progress.Increment();
    }

    Assert.AreEqual(TOTAL_ - 1, progressChangedCallCount);
    Assert.AreEqual(0, completeCallCount);
    
    progress.Increment();
    Assert.AreEqual(TOTAL_ - 1, progressChangedCallCount);
    Assert.AreEqual(1, completeCallCount);

    progress.Increment();
    Assert.AreEqual(TOTAL_ - 1, progressChangedCallCount);
    Assert.AreEqual(1, completeCallCount);
  }
}