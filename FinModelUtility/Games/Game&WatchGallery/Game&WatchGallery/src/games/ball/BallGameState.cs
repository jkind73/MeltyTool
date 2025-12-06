namespace gawg.games.ball;

public sealed class BallGameState(uint ballCount, uint initialRightHandPosition) {
  // TODO: Vary this based on the score
  public int TickPeriod => 2;

  public int CurrentScore { get; private set; }
  public int AddPoint() => ++this.CurrentScore;

  public void Fail(BallState ballState) => this.OnFail(ballState);

  public event Action<BallState> OnFail = delegate { };

  public uint RightHandPosition { get; private set; } = initialRightHandPosition;
  public uint LeftHandPosition => (ballCount - 1) - this.RightHandPosition;

  public void MoveLeft() {
    this.RightHandPosition = (uint) Math.Max(0, (int) this.RightHandPosition - 1);
  }

  public void MoveRight() {
    this.RightHandPosition
        = Math.Min(this.RightHandPosition + 1, ballCount - 1);
  }
}