using System;

namespace fin.util.time;

public sealed class DiscreteFpsStopwatch {
  private DateTime startOfSecond_;
  private int fps_;

  public int Fps { get; private set; }

  public void MarkStartOfFrame() {
    var startOfFrame = FrameTime.StartOfFrame;

    if (this.startOfSecond_ == null ||
        (startOfFrame - this.startOfSecond_).Seconds >= 1) {
      this.startOfSecond_ = startOfFrame;
      this.Fps = this.fps_;
      this.fps_ = 1;
      return;
    }

    ++this.fps_;
  }
}