using gawg.common;

namespace gawg.games.chef.difficulty;

public interface IDynamicDifficultyValue<T> {
  T GetValue(uint currentScore);
}