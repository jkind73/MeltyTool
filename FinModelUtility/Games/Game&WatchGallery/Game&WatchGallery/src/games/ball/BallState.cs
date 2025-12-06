using fin.scene;

using gawg.common;

namespace gawg.games.ball;

public enum BallDirection {
  LEFT,
  RIGHT,
}

public sealed class BallState : ITickable {
  private readonly IGawgEventManager eventManager_;
  private readonly BallGameState gameState_;
  private readonly uint tickDurationInAir_;

  public uint Distance { get; }

  public IReadOnlyGawgEvent InAirEvent { get; private set; }
  private IGawgEvent catchEvent_;

  public BallDirection Direction { get; private set; }

  public BallState(
      IGawgEventManager eventManager,
      BallGameState gameState,
      uint distance,
      uint tickDurationInAir,
      BallDirection initialDirection) {
    this.eventManager_ = eventManager;
    this.gameState_ = gameState;
    this.Distance = distance;
    this.tickDurationInAir_ = tickDurationInAir;
    this.Direction = initialDirection;

    this.UpdateEvents_();
  }

  public void Tick() {
    switch (this.catchEvent_.State) {
      case GawgEventState.ACTIVE: {
        var caughtBall = this.Direction switch {
            BallDirection.LEFT
                => this.Distance == this.gameState_.LeftHandPosition,
            BallDirection.RIGHT
                => this.Distance == this.gameState_.RightHandPosition,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (caughtBall) {
          this.gameState_.AddPoint();
          this.UpdateEvents_();
        }

        break;
      }
      // Uh oh. If it was allowed to complete, that means the player dropped
      // the ball.
      case GawgEventState.COMPLETE: {
        this.gameState_.Fail(this);
        break;
      }
    }
  }

  private void UpdateEvents_() {
    this.InAirEvent = this.eventManager_.AddEvent(0, this.tickDurationInAir_);
    this.catchEvent_
        = this.eventManager_.AddEvent(this.tickDurationInAir_ - 1, 1);
  }
}