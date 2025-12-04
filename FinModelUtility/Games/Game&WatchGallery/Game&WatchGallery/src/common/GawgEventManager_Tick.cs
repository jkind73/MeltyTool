namespace gawg.common;

public sealed partial class GawgEventManager {
  private ulong currentTickId_;
  private DateTime currentTickStart_;
  private TimeSpan currentTickTimeSpan_;

  public IGawgTick CurrentTick { get; private set; }
  public float CurrentTickProgress { get; private set; }
  public float TickPeriod { get; set; } = 1;

  public IGawgTick GetTickAfter(IGawgTick other, ulong delta)
    => new GawgTick { Id = other.Id + delta };

  private sealed class GawgTick : IGawgTick {
    public required ulong Id { get; init; }

    static IGawgTick IGawgTick.operator+(IGawgTick lhs, ulong rhs) {
      if (rhs == 0) {
        return lhs;
      }
      
      return new GawgTick { Id = lhs.Id + rhs, };
    }
  }
}