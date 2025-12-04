using fin.scene;

using readOnly;

namespace gawg.common;

/// <summary>
///   Class for managing a Game & Watch style scheduling system.
/// </summary>
[GenerateReadOnly]
public partial interface IGawgEventManager : ITickable {
  // Tick logic
  IGawgTick CurrentTick { get; }
  float CurrentTickProgress { get; }

  float TickPeriod { get; set; }

  // Event logic
  void MarkAllEventsComplete();

  IGawgEvent AddEvent(ulong ticksFromCurrent, ulong durationInTicks);

  bool TryToAddExclusiveEvent(
      ulong ticksFromCurrent,
      ulong durationInTicks,
      out IGawgEvent outEvent);

  IGawgEvent AddSoonestExclusiveEventAfter(
      ulong earliestTicksFromCurrent,
      ulong duration);
}

public enum GawgEventState {
  UNDEFINED,
  WAITING,
  ACTIVE,
  COMPLETE,
}

[GenerateReadOnly]
public partial interface IGawgTick {
  ulong Id { get; }

  public static virtual bool operator==(IGawgTick lhs, IGawgTick rhs)
    => lhs.Id == rhs.Id;

  public static virtual bool operator!=(IGawgTick lhs, IGawgTick rhs)
    => !(lhs == rhs);

  public static virtual bool operator>(IGawgTick lhs, IGawgTick rhs)
    => (lhs - rhs) < (rhs - lhs);

  public static virtual bool operator>=(IGawgTick lhs, IGawgTick rhs)
    => lhs == rhs || lhs > rhs;

  public static virtual bool operator<=(IGawgTick lhs, IGawgTick rhs)
    => !(lhs > rhs);

  public static virtual bool operator<(IGawgTick lhs, IGawgTick rhs)
    => !(lhs >= rhs);

  public static abstract IGawgTick operator+(IGawgTick lhs, ulong rhs);

  public static ulong operator-(IGawgTick lhs, IGawgTick rhs) {
    var lhsId = lhs.Id;
    var rhsId = rhs.Id;

    if (lhsId > rhsId) {
      return lhsId - rhsId;
    }

    return lhsId + (ulong.MaxValue - rhsId);
  }
}

[GenerateReadOnly]
public partial interface IGawgEvent {
  GawgEventState State { get; }

  float Progress { get; }
  bool IsExclusive { get; }
  IGawgTick InclusiveStart { get; }
  IGawgTick InclusiveEnd { get; }
}