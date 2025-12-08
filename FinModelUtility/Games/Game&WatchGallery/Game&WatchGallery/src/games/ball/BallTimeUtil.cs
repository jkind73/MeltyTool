using fin.math;

namespace gawg.games.ball;

public static class BallTimeUtil {
  public static uint GetAdjustedTickDuration(uint rawTickDuration,
                                             uint ballCount)
    => rawTickDuration * ballCount;

  public static ulong GetAdjustedStep(
      ulong elapsedTicks,
      uint ballCount)
    => elapsedTicks / ballCount;

  public static float GetAdjustedSteppedProgress(
      ulong elapsedTicks,
      ulong tickDuration,
      uint ballCount)
    => (GetAdjustedStep(elapsedTicks, ballCount) /
        (tickDuration - 1f))
        .Clamp(0, 1);
}