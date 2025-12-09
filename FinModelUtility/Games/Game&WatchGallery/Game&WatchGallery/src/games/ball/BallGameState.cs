using gawg.games.difficulty.ball;

namespace gawg.games.ball;

public sealed class BallGameState(
    uint ballCount,
    uint initialRightHandPosition) {
  public uint BallCount => ballCount;

  private readonly GawgTickPeriod dynamicTickPeriod_
      = new();

  public float TickPeriod
    => this.dynamicTickPeriod_.GetValue(this.CurrentScore);

  public uint CurrentScore { get; private set; }
  public void AddPoint() => this.CurrentScore += 10;

  public event Action<BallState> OnBallTicked = delegate { };

  public void TriggerBallTickedEvent(BallState ballState)
    => this.OnBallTicked(ballState);

  public event Action<BallState> OnBallDropped = delegate { };

  public void TriggerBallDroppedEvent(BallState ballState)
    => this.OnBallDropped(ballState);

  public event Action<BallState> OnBallJuggled = delegate { };

  public void TriggerBallJuggledEvent(BallState ballState)
    => this.OnBallJuggled(ballState);

  public uint RightHandPosition { get; private set; }
    = initialRightHandPosition;

  public uint LeftHandPosition => (ballCount - 1) - this.RightHandPosition;

  public void MoveLeft() => this.RightHandPosition
      = (uint) Math.Max(0, (int) this.RightHandPosition - 1);

  public void MoveRight() => this.RightHandPosition
      = Math.Min(this.RightHandPosition + 1, ballCount - 1);
}