using fin.scene;

using gawg.common;

namespace gawg.games.ball;

public sealed class BallGameManager : ITickable {
  private readonly IGawgEventManager eventManager_;
  private readonly BallState[] ballStates_;

  public BallGameState GameState { get; }
  public IReadOnlyList<BallState> BallStates => this.ballStates_;

  public BallGameManager(uint ballCount, uint initialRightHandPosition) {
    this.GameState = new(ballCount, initialRightHandPosition);
    this.eventManager_ = new GawgEventManager {
        TickPeriod = this.GameState.TickPeriod
    };

    this.ballStates_ =
        Enumerable
            .Range(0, (int) ballCount)
            .Select(i => new BallState(this.eventManager_,
                                       this.GameState,
                                       (uint) i,
                                       (uint) (4 + i) * 2,
                                       (i % 2) == 0
                                           ? BallDirection.RIGHT
                                           : BallDirection.LEFT))
            .ToArray();
  }

  public void Tick() {
    this.eventManager_.TickPeriod = this.GameState.TickPeriod;
    this.eventManager_.Tick();

    foreach (var ballState in this.ballStates_) {
      ballState.Tick();
    }
  }
}