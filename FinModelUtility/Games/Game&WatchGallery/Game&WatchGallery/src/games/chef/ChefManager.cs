using fin.scene;

using gawg.common;
using gawg.common.difficulty;
using gawg.games.chef.difficulty;

namespace gawg.games.chef;

public sealed class ChefManager : ITickable {
  private readonly IGawgEventManager eventManager_ = new GawgEventManager();

  private uint currentScore_;

  private readonly IDynamicDifficultyValue<uint> maxJuggledFoodItems_
      = new GawgJuggledFoodCount();

  public void Tick() {
    this.eventManager_.Tick();

    var maxJuggledFoodItems
        = this.maxJuggledFoodItems_.GetValue(this.currentScore_);
  }
}