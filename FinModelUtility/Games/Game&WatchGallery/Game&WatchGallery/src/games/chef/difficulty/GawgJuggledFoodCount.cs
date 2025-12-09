using gawg.common.difficulty;

namespace gawg.games.chef.difficulty;

public sealed class GawgJuggledFoodCount : IDynamicDifficultyValue<uint> {
  public uint GetValue(uint currentScore) {
    var currentHundred = currentScore / 100;
    var currentTen = (currentScore % 100) / 10;

    return currentHundred switch {
        < 2 => currentTen switch {
            < 5 => 2,
            _   => 3
        },
        _ => currentTen switch {
            < 5 => 3,
            _   => 4
        }
    };
  }
}