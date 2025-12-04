namespace gawg.games.chef.difficulty;

/// <summary>
///   Calculates the maximum number of food items allowed in the air, ramping
///   up over time to adjust the difficulty.
///
///   These numbers were just kind of pulled out of my ass.
/// </summary>
public sealed class MeltyPlayerMaxJuggledFoodValues
    : IDynamicDifficultyValue<uint> {
  public uint GetValue(uint currentScore) {
    var currentHundred = currentScore / 100;
    var currentTen = (currentScore % 100) / 10;

    var baseCount = currentHundred switch {
        < 3 => 3,
        < 6 => 4,
        _   => 5,
    };

    var currentRoundCount = currentHundred switch {
        < 3 => currentTen switch {
            < 5 => 0,
            _   => 1
        },
        < 6 => currentTen switch {
            < 3 => -1,
            < 6 => 0,
            _   => 1
        },
        _ => currentTen switch {
            < 3 => -1,
            < 6 => 0,
            _   => 1
        },
    };

    return (uint) (baseCount + currentRoundCount);
  }
}