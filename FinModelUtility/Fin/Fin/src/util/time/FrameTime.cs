using System;
using System.Linq;

using fin.ui;

namespace fin.util.time;

public static class FrameTime {
  private static readonly DateTime FIRST_FRAME_START_ = DateTime.Now;
  private static DateTime previousFrameStart_;

  private const int SMOOTH_COUNT_ = UiConstants.FPS;

  private static readonly float[] FRAME_TIMES_FOR_SMOOTHED_ACTUAL_FPS_
      = new float[SMOOTH_COUNT_];

  private static readonly float[] FRAME_TIMES_FOR_SMOOTHED_THEORETICAL_FPS_
      = new float[SMOOTH_COUNT_];

  public static void Initialize() => MarkStartOfFrame();

  public static void MarkStartOfFrame() {
    previousFrameStart_ = StartOfFrame;
    StartOfFrame = DateTime.Now;

    ElapsedTimeSinceApplicationOpened
        = StartOfFrame - FIRST_FRAME_START_;

    var elapsedSeconds =
        (float) (StartOfFrame - previousFrameStart_)
        .TotalSeconds;
    DeltaTime = elapsedSeconds;

    for (var i = FRAME_TIMES_FOR_SMOOTHED_ACTUAL_FPS_.Length - 1; i >= 1; --i) {
      FRAME_TIMES_FOR_SMOOTHED_ACTUAL_FPS_[i]
          = FRAME_TIMES_FOR_SMOOTHED_ACTUAL_FPS_[i - 1];
    }

    FRAME_TIMES_FOR_SMOOTHED_ACTUAL_FPS_[0] = elapsedSeconds;

    var smoothedFrameTime = FRAME_TIMES_FOR_SMOOTHED_ACTUAL_FPS_.Average();

    SmoothedActualFps = smoothedFrameTime == 0 ? 0 : 1 / smoothedFrameTime;
  }

  public static void MarkEndOfFrameForFpsDisplay() {
    var elapsedSeconds
        = (float) (DateTime.Now - StartOfFrame).TotalSeconds;

    for (var i = FRAME_TIMES_FOR_SMOOTHED_THEORETICAL_FPS_.Length - 1;
         i >= 1;
         --i) {
      FRAME_TIMES_FOR_SMOOTHED_THEORETICAL_FPS_[i]
          = FRAME_TIMES_FOR_SMOOTHED_THEORETICAL_FPS_[i - 1];
    }

    FRAME_TIMES_FOR_SMOOTHED_THEORETICAL_FPS_[0] = elapsedSeconds;

    var smoothedFrameTime = FRAME_TIMES_FOR_SMOOTHED_THEORETICAL_FPS_.Average();

    SmoothedTheoreticalFps
        = smoothedFrameTime == 0 ? 0 : 1 / smoothedFrameTime;
  }

  public static DateTime StartOfFrame { get; private set; }

  public static TimeSpan ElapsedTimeSinceApplicationOpened {
    get;
    private set;
  }

  public static float DeltaTime { get; private set; }

  public static float SmoothedActualFps { get; private set; }
  public static float SmoothedTheoreticalFps { get; private set; }

  public static string FpsString
    => $"Universal Asset Tool ({SmoothedActualFps:0.0} / {SmoothedTheoreticalFps:0.0} fps)";
}