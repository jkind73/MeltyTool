using fin.math;

namespace gawg.games.ball;

public static class BallTimeUtil {
  public static uint GetAdjustedTickDuration(uint rawTickDuration,
                                             uint ballCount)
    => rawTickDuration * ballCount;

  public static float GetAdjustedSteppedProgress(
      ulong elapsedTicks,
      ulong adjustedTickDuration,
      uint ballCount)
    => (1f * elapsedTicks).FloorToNearest(ballCount) /
       adjustedTickDuration;
}