using fin.scene;

using gawg.common;
using gawg.games.chef.difficulty;

namespace gawg.games.chef;

public sealed class ChefManager : ITickable {
  private readonly IGawgEventManager eventManager_ = new GawgEventManager();

  private uint currentScore_;

  private readonly IDynamicDifficultyValue<uint> maxJuggledFoodItems_
      = new MeltyPlayerMaxJuggledFoodValues();

  public void Tick() {
    this.eventManager_.Tick();

    var maxJuggledFoodItems
        = this.maxJuggledFoodItems_.GetValue(this.currentScore_);
  }
}