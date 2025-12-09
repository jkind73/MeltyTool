using gawg.common.difficulty;

namespace gawg.games.difficulty.ball;

public sealed class GawgTickPeriod
    : IDynamicDifficultyValue<float> {
  public float GetValue(uint currentScore) {
    var currentHundred = (currentScore % 1000) / 100;
    var currentTen = (currentScore % 100) / 10;

    var bpmFloor = GetBpmForHundred_(currentHundred);
    var bpmCeiling = GetBpmForHundred_(currentHundred + 1);

    var bpm = float.Lerp(bpmFloor, bpmCeiling, currentTen / 10f);
    var bps = bpm / 60;

    return 1 / bps;
  }

  private static uint GetBpmForHundred_(uint currentHundred)
    => 230 + 10 * currentHundred;
}