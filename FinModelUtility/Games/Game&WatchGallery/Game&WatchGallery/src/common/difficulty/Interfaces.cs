namespace gawg.common.difficulty;

public interface IDynamicDifficultyValue<T> {
  T GetValue(uint currentScore);
}