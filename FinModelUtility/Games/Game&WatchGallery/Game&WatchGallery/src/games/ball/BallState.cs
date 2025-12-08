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
  private readonly uint adjustedTickDurationInAir_;

  public uint Index { get; }

  private float previousAirSteppedProgress_;

  public float AirSteppedProgress => BallTimeUtil.GetAdjustedSteppedProgress(
      this.inAirEvent_.ElapsedTicks,
      this.tickDurationInAir_,
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
    this.adjustedTickDurationInAir_ = BallTimeUtil.GetAdjustedTickDuration(
        tickDurationInAir,
        gameState.BallCount);
    this.Direction = initialDirection;

    this.UpdateEvents_();
  }

  public void Tick() {
    var currentAirSteppedProgress = this.AirSteppedProgress;
    if (currentAirSteppedProgress > this.previousAirSteppedProgress_) {
      this.gameState_.TriggerBallTickedEvent(this);
    }

    this.previousAirSteppedProgress_ = currentAirSteppedProgress;

    switch (this.catchEvent_.State) {
      case GawgEventState.ACTIVE: {
        var juggledBall = this.Direction switch {
            BallDirection.LEFT
                => this.Index == this.gameState_.LeftHandPosition,
            BallDirection.RIGHT
                => this.Index == this.gameState_.RightHandPosition,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (juggledBall) {
          this.gameState_.AddPoint();
          this.Direction = this.Direction switch {
              BallDirection.LEFT  => BallDirection.RIGHT,
              BallDirection.RIGHT => BallDirection.LEFT,
          };
          this.UpdateEvents_();
          this.gameState_.TriggerBallJuggledEvent(this);
        }

        break;
      }
      // Uh oh. If it was allowed to complete, that means the player dropped
      // the ball.
      case GawgEventState.COMPLETE: {
        this.gameState_.TriggerBallDroppedEvent(this);
        break;
      }
    }
  }

  private void UpdateEvents_() {
    var ballCount = this.gameState_.BallCount;

    if (this.inAirEvent_ == null) {
      this.inAirEvent_ = this.eventManager_.AddEvent(
          this.Index,
          this.adjustedTickDurationInAir_);
    } else {
      this.inAirEvent_ = this.eventManager_.AddEventAtSameTimeAs(
          this.catchEvent_,
          this.adjustedTickDurationInAir_);
    }

    this.catchEvent_ = this.eventManager_.AddEventRelativeToEndOf(
        this.inAirEvent_,
        -ballCount,
        ballCount);

    this.previousAirSteppedProgress_ = this.AirSteppedProgress;
  }
}