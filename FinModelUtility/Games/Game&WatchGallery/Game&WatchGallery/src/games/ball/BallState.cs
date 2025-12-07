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

  public uint Index { get; }

  public float AirSteppedProgress => BallTimeUtil.GetAdjustedSteppedProgress(
      this.inAirEvent_.ElapsedTicks,
      this.inAirEvent_.DurationInTicks - 1,
      this.gameState_.BallCount);

  public BallDirection Direction { get; private set; }

  private IReadOnlyGawgEvent inAirEvent_;
  private IGawgEvent catchEvent_;

  public BallState(
      IGawgEventManager eventManager,
      BallGameState gameState,
      uint index,
      uint tickDurationInAir,
      BallDirection initialDirection) {
    this.eventManager_ = eventManager;
    this.gameState_ = gameState;
    this.Index = index;
    this.tickDurationInAir_ = tickDurationInAir;
    this.Direction = initialDirection;

    this.UpdateEvents_(true);
  }

  public void Tick() {
    switch (this.catchEvent_.State) {
      case GawgEventState.ACTIVE: {
        var caughtBall = this.Direction switch {
            BallDirection.LEFT
                => this.Index == this.gameState_.LeftHandPosition,
            BallDirection.RIGHT
                => this.Index == this.gameState_.RightHandPosition,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (caughtBall) {
          this.gameState_.AddPoint();
          this.Direction = this.Direction switch {
              BallDirection.LEFT  => BallDirection.RIGHT,
              BallDirection.RIGHT => BallDirection.LEFT,
          };
          this.UpdateEvents_(false);
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

  private void UpdateEvents_(bool factorInIndexIntoOffset) {
    var ballCount = this.gameState_.BallCount;

    var startOffset = factorInIndexIntoOffset
        ? this.Index
        : this.inAirEvent_.InclusiveEnd.GetDurationSince(
            this.eventManager_.CurrentTick);
    var adjustedTickDurationInAir
        = BallTimeUtil.GetAdjustedTickDuration(this.tickDurationInAir_,
                                               ballCount);

    this.inAirEvent_
        = this.eventManager_.AddEvent(startOffset, adjustedTickDurationInAir);
    this.catchEvent_
        = this.eventManager_.AddEvent(
            startOffset + adjustedTickDurationInAir - ballCount,
            ballCount);
  }
}