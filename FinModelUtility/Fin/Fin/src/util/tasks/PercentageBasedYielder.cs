using System;
using System.Threading.Tasks;

namespace fin.util.tasks;

public sealed class PercentageBasedYielder(
    int total,
    float yieldFrequency = .03f) {
  private bool isComplete_;

  private int current_;

  public async Task IncrementAsync() {
    if (this.isComplete_) {
      return;
    }

    var current = ++this.current_;
    if (current >= total) {
      this.isComplete_ = true;
      this.OnComplete?.Invoke(this, EventArgs.Empty);
      return;
    }

    var previousProgress = this.Progress;

    this.Progress = 1f * current / total;

    var newProgress = this.Progress;

    if (Math.Floor(previousProgress / yieldFrequency) < Math.Floor(newProgress / yieldFrequency)) {
      this.OnProgressChanged?.Invoke(this, this.Progress);
      await Task.Yield();
    }
  }

  public float Progress { get; private set; }
  public event EventHandler<float>? OnProgressChanged;
  public event EventHandler? OnComplete;
}