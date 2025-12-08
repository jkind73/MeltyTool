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

  IGawgEvent AddEvent(long ticksFromCurrent, ulong durationInTicks);
  IGawgEvent AddEventRelativeToStartOf(IReadOnlyGawgEvent other, long offset, ulong durationInTicks);
  IGawgEvent AddEventRelativeToEndOf(IReadOnlyGawgEvent other, long offset, ulong durationInTicks);

  bool TryToAddExclusiveEvent(
      long ticksFromCurrent,
      ulong durationInTicks,
      out IGawgEvent outEvent);

  IGawgEvent AddSoonestExclusiveEventAfter(
      long earliestTicksFromCurrent,
      ulong duration);
}

public enum GawgEventState {
  UNDEFINED,
  WAITING,
  ACTIVE,
  COMPLETE,
}

public partial interface IGawgTick {
  ulong Id { get; }

  bool AtSameTimeAs(IGawgTick other);
  bool IsAfter(IGawgTick other);
  bool AtSameTimeAsOrAfter(IGawgTick other);
  bool IsBefore(IGawgTick other);
  bool AtSameTimeAsOrBefore(IGawgTick other);
  IGawgTick GetRelativeTick(long delta);
  ulong GetDurationSince(IGawgTick other);
}

[GenerateReadOnly]
public partial interface IGawgEvent {
  GawgEventState State { get; }

  ulong DurationInTicks { get; }
  ulong ElapsedTicks { get; }

  float SteppedProgress { get; }
  float Progress { get; }

  bool IsExclusive { get; }
  IGawgTick InclusiveStart { get; }
  IGawgTick InclusiveEnd { get; }
}