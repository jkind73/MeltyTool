using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

using Avalonia.Controls;
using Avalonia.OpenGL.Controls;

namespace fin.ui.avalonia.gl;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/AvaloniaUI/Avalonia/issues/17865#issuecomment-2568602395
/// </summary>
public sealed class FpsTimer {
  private readonly OpenGlControlBase render_;
  private TopLevel top_;
  private readonly Timer timer_;
  private bool pause_ = true;
  private bool last_;

  public bool LowFps { get; set; }
  public Action<int>? FpsTick { private get; init; }

  public bool Pause {
    get { return this.pause_; }
    set {
      //不改变
      if (this.pause_ == value) {
        return;
      }

      //暂停 -> 继续
      if (this.pause_ && value == false) {
        this.top_ ??= TopLevel.GetTopLevel(this.render_) ?? throw new Exception();
        this.pause_ = false;
        this.timer_.Start();
        this.Go_();
      } else //暂停
      {
        this.pause_ = true;
        this.timer_.Stop();
      }
    }
  }

  public int NowFps { get; private set; }

  public FpsTimer(OpenGlControlBase render) {
    this.render_ = render;
    this.timer_ = new(TimeSpan.FromSeconds(1));
    this.timer_.BeginInit();
    this.timer_.AutoReset = true;
    this.timer_.Elapsed += this.Timer_Elapsed;
    this.timer_.EndInit();
  }

  private void Go_() {
    if (!this.pause_) {
      this.top_.RequestAnimationFrame((t) => {
        if (this.LowFps) {
          this.last_ = !this.last_;
          if (this.last_) {
            this.render_.RequestNextFrameRendering();
            this.NowFps++;
          }
        } else {
          this.render_.RequestNextFrameRendering();
          this.NowFps++;
        }

        this.Go_();
      });
    }
  }

  private void Timer_Elapsed(object? sender, ElapsedEventArgs e) {
    if (!this.Pause) {
      this.FpsTick?.Invoke(this.NowFps);
    }

    this.NowFps = 0;
  }

  public void Close() {
    this.pause_ = true;
    this.timer_.Stop();
    this.timer_.Close();
    this.timer_.Dispose();
  }
}