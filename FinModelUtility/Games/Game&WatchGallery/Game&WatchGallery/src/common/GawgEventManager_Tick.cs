namespace gawg.common;

public sealed partial class GawgEventManager {
  private ulong currentTickId_;
  private DateTime currentTickStart_;
  private TimeSpan currentTickTimeSpan_;

  public IGawgTick CurrentTick { get; private set; }
  public float CurrentTickProgress { get; private set; }
  public float TickPeriod { get; set; } = 1;

  private sealed class GawgTick : IGawgTick {
    public required ulong Id { get; init; }

    public bool AtSameTimeAs(IGawgTick other) => this.Id == other.Id;

    public bool IsAfter(IGawgTick other)
      => this.GetDurationSince(other) < other.GetDurationSince(this);

    public bool AtSameTimeAsOrAfter(IGawgTick other)
      => this.AtSameTimeAs(other) || this.IsAfter(other);

    public bool IsBefore(IGawgTick other)
      => this.GetDurationSince(other) > other.GetDurationSince(this);

    public bool AtSameTimeAsOrBefore(IGawgTick other)
      => this.AtSameTimeAs(other) || this.IsBefore(other);

    public IGawgTick GetTickAfter(ulong delta) {
      if (delta == 0) {
        return this;
      }

      return new GawgTick { Id = this.Id + delta, };
    }

    public ulong GetDurationSince(IGawgTick other) {
      var lhsId = this.Id;
      var rhsId = other.Id;

      if (lhsId >= rhsId) {
        return lhsId - rhsId;
      }

      return lhsId + (ulong.MaxValue - rhsId);
    }
  }
}