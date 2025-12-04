using fin.scene;

namespace gawg.common;

public sealed partial class GawgEventManager {
  private readonly LinkedList<GawgEvent> allIncompleteEvents_ = new();

  public void MarkAllEventsComplete() {
    foreach (var gawgEvent in this.allIncompleteEvents_) {
      gawgEvent.State = GawgEventState.COMPLETE;
      gawgEvent.Progress = 1;
    }

    this.allIncompleteEvents_.Clear();
  }

  public IGawgEvent AddEvent(ulong ticksFromCurrent, ulong durationInTicks) {
    var inclusiveStart = this.CurrentTick + ticksFromCurrent;

    var newEvent = new GawgEvent(this) {
        InclusiveStart = inclusiveStart,
        InclusiveEnd = inclusiveStart + durationInTicks,
        IsExclusive = false,
    };

    this.allIncompleteEvents_.AddLast(newEvent);

    return newEvent;
  }

  public bool TryToAddExclusiveEvent(
      ulong ticksFromCurrent,
      ulong durationInTicks,
      out IGawgEvent outEvent) {
    var inclusiveStart = this.CurrentTick + ticksFromCurrent;
    var inclusiveEnd = inclusiveStart + durationInTicks;
    foreach (var gawgEvent in this.allIncompleteEvents_) {
      if (gawgEvent.IsExclusive &&
          gawgEvent.InclusiveStart <= inclusiveEnd &&
          inclusiveStart <= gawgEvent.InclusiveEnd) {
        outEvent = null!;
        return false;
      }
    }

    var newEvent = new GawgEvent(this) {
        InclusiveStart = inclusiveStart,
        InclusiveEnd = inclusiveEnd,
        IsExclusive = true,
    };
    this.allIncompleteEvents_.AddLast(newEvent);

    outEvent = newEvent;
    return true;
  }

  public IGawgEvent AddSoonestExclusiveEventAfter(
      ulong earliestTicksFromCurrent,
      ulong durationInTicks) {
    var inclusiveStart = this.CurrentTick + earliestTicksFromCurrent;
    var inclusiveEnd = inclusiveStart + durationInTicks;

    TryAgain:
    foreach (var gawgEvent in this.allIncompleteEvents_) {
      if (gawgEvent.IsExclusive &&
          gawgEvent.InclusiveStart <= inclusiveEnd &&
          inclusiveStart <= gawgEvent.InclusiveEnd) {
        inclusiveStart = gawgEvent.InclusiveEnd + 1;
        inclusiveEnd = inclusiveStart + durationInTicks;

        goto TryAgain;
      }
    }

    var newEvent = new GawgEvent(this) {
        InclusiveStart = inclusiveStart,
        InclusiveEnd = inclusiveEnd,
        IsExclusive = true,
    };
    this.allIncompleteEvents_.AddLast(newEvent);
    return newEvent;
  }

  private sealed class GawgEvent(IGawgEventManager eventManager)
      : IGawgEvent, ITickable {
    public GawgEventState State { get; set; } = GawgEventState.WAITING;
    public float Progress { get; set; }

    public required bool IsExclusive { get; init; }
    public required IGawgTick InclusiveStart { get; init; }
    public required IGawgTick InclusiveEnd { get; init; }

    public void Tick() {
      if (this.State == GawgEventState.COMPLETE) {
        return;
      }

      // Updating current state
      {
        var currentState = this.State;

        GawgEventState newState;
        if (eventManager.CurrentTick < this.InclusiveStart) {
          newState = GawgEventState.WAITING;
        } else if (this.InclusiveEnd < eventManager.CurrentTick) {
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
          this.Progress = 0;
        } else if (state == GawgEventState.COMPLETE) {
          this.Progress = 1;
        } else {
          var durationInTicks = 1 + (this.InclusiveEnd - this.InclusiveStart);
          var completedTicks = eventManager.CurrentTick - this.InclusiveStart;

          var singleTickSize = 1f / durationInTicks;
          var progressInCurrentTick = eventManager.CurrentTickProgress;

          this.Progress = singleTickSize *
                          (completedTicks + progressInCurrentTick);
        }
      }
    }
  }
}