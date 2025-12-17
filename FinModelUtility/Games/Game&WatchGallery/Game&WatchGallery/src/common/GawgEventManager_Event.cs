using fin.scene;

namespace gawg.common;

public sealed partial class GawgEventManager {
  private readonly LinkedList<GawgEvent> allIncompleteEvents_ = new();

  public void MarkAllEventsComplete() {
    foreach (var gawgEvent in this.allIncompleteEvents_) {
      gawgEvent.State = GawgEventState.COMPLETE;
      gawgEvent.ElapsedTicks = gawgEvent.DurationInTicks;
      gawgEvent.SteppedProgress = gawgEvent.Progress = 1;
    }

    this.allIncompleteEvents_.Clear();
  }

  public IGawgEvent AddEvent(long ticksFromCurrent, ulong durationInTicks) {
    var inclusiveStart = this.CurrentTick.GetRelativeTick(ticksFromCurrent);

    var newEvent = new GawgEvent(this, inclusiveStart, durationInTicks, false);

    this.allIncompleteEvents_.AddLast(newEvent);

    return newEvent;
  }

  public IGawgEvent AddEventRelativeToStartOf(
      IReadOnlyGawgEvent other,
      long offset,
      ulong durationInTicks) {
    var inclusiveStart = other.InclusiveStart.GetRelativeTick(offset);

    var newEvent = new GawgEvent(this, inclusiveStart, durationInTicks, false);

    this.allIncompleteEvents_.AddLast(newEvent);

    return newEvent;
  }

  public IGawgEvent AddEventRelativeToEndOf(
      IReadOnlyGawgEvent other,
      long offset,
      ulong durationInTicks) {
    var inclusiveStart = other.InclusiveStart.GetRelativeTick(
        (long) other.DurationInTicks + offset);

    var newEvent = new GawgEvent(this, inclusiveStart, durationInTicks, false);

    this.allIncompleteEvents_.AddLast(newEvent);

    return newEvent;
  }

  public bool TryToAddExclusiveEvent(
      long ticksFromCurrent,
      ulong durationInTicks,
      out IGawgEvent outEvent) {
    var inclusiveStart = this.CurrentTick.GetRelativeTick(ticksFromCurrent);
    var inclusiveEnd = inclusiveStart.GetRelativeTick((long) durationInTicks - 1);
    foreach (var gawgEvent in this.allIncompleteEvents_) {
      if (gawgEvent.IsExclusive &&
          gawgEvent.InclusiveStart.AtSameTimeAsOrBefore(inclusiveEnd) &&
          inclusiveStart.AtSameTimeAsOrBefore(gawgEvent.InclusiveEnd)) {
        outEvent = null!;
        return false;
      }
    }

    var newEvent = new GawgEvent(this, inclusiveStart, durationInTicks, true);
    this.allIncompleteEvents_.AddLast(newEvent);

    outEvent = newEvent;
    return true;
  }

  public IGawgEvent AddSoonestExclusiveEventAfter(
      long earliestTicksFromCurrent,
      ulong durationInTicks) {
    var inclusiveStart
        = this.CurrentTick.GetRelativeTick(earliestTicksFromCurrent);
    var inclusiveEnd
        = inclusiveStart.GetRelativeTick((long) durationInTicks - 1);

    TryAgain:
    foreach (var gawgEvent in this.allIncompleteEvents_) {
      if (gawgEvent.IsExclusive &&
          gawgEvent.InclusiveStart.AtSameTimeAsOrBefore(inclusiveEnd) &&
          inclusiveStart.AtSameTimeAsOrBefore(gawgEvent.InclusiveEnd)) {
        inclusiveStart = gawgEvent.InclusiveEnd.GetRelativeTick(1);
        inclusiveEnd
            = inclusiveStart.GetRelativeTick((long) durationInTicks - 1);

        goto TryAgain;
      }
    }

    var newEvent = new GawgEvent(this, inclusiveStart, durationInTicks, true);
    this.allIncompleteEvents_.AddLast(newEvent);
    return newEvent;
  }

  private sealed class GawgEvent(
      IGawgEventManager eventManager,
      IGawgTick inclusiveStart,
      ulong durationInTicks,
      bool isExclusive)
      : IGawgEvent, ITickable {
    public IGawgTick InclusiveStart => inclusiveStart;

    public IGawgTick InclusiveEnd { get; }
      = inclusiveStart.GetRelativeTick((long) durationInTicks);

    public ulong DurationInTicks => durationInTicks;
    public bool IsExclusive => isExclusive;

    public GawgEventState State { get; set; } = GawgEventState.WAITING;
    public ulong ElapsedTicks { get; set; }
    public float SteppedProgress { get; set; }
    public float Progress { get; set; }

    public void Tick() {
      if (this.State == GawgEventState.COMPLETE) {
        return;
      }

      // Updating current state
      {
        var currentState = this.State;

        GawgEventState newState;
        if (eventManager.CurrentTick.IsBefore(this.InclusiveStart)) {
          newState = GawgEventState.WAITING;
        } else if (eventManager.CurrentTick.GetDurationSince(
                       this.InclusiveStart) >=
                   this.DurationInTicks) {
          newState = GawgEventState.COMPLETE;
        } else {
          newState = GawgEventState.ACTIVE;
        }

        // If state jumps immediately from waiting to complete, multiple ticks
        // occurred in a single frame. Let's give the player a chance and have
        // this event active for a minimum of one frame.
        if (currentState == GawgEventState.WAITING &&
            newState == GawgEventState.COMPLETE) {
          newState = GawgEventState.ACTIVE;
        }

        this.State = newState;
      }

      // Updating progress
      {
        var state = this.State;
        if (state == GawgEventState.WAITING) {
          this.ElapsedTicks = 0;
          this.SteppedProgress = 0;
          this.Progress = 0;
        } else if (state == GawgEventState.COMPLETE) {
          this.ElapsedTicks = this.DurationInTicks;
          this.SteppedProgress = 1;
          this.Progress = 1;
        } else {
          var completedTicks
              = eventManager.CurrentTick.GetDurationSince(this.InclusiveStart);

          var singleTickSize = 1f / this.DurationInTicks;
          var progressInCurrentTick = eventManager.CurrentTickProgress;

          this.ElapsedTicks = completedTicks;
          this.SteppedProgress = singleTickSize * completedTicks;
          this.Progress = singleTickSize *
                          (completedTicks + progressInCurrentTick);
        }
      }
    }
  }
}