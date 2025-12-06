using fin.util.time;

namespace gawg.common;

public sealed partial class GawgEventManager : IGawgEventManager {
  public GawgEventManager(float tickPeriod = 1) {
    this.currentTickStart_ = FrameTime.StartOfFrame;
    this.currentTickTimeSpan_
        = TimeSpan.FromSeconds(this.TickPeriod = tickPeriod);
    this.CurrentTick = new GawgTick {Id = this.currentTickId_};
  }

  public void Tick() {
    // Update current tick
    {
      var now = FrameTime.StartOfFrame;
    
      // Should not be possible for multiple ticks to happen in a single frame,
      // but just in case let's support this case.
      var elapsed = now - this.currentTickStart_;
      while (elapsed > this.currentTickTimeSpan_) {
        elapsed -= this.currentTickTimeSpan_;

        this.CurrentTick = new GawgTick { Id = ++this.currentTickId_ };
        this.currentTickStart_ = now - elapsed;
        this.currentTickTimeSpan_ = TimeSpan.FromSeconds(this.TickPeriod);
      }

      this.CurrentTickProgress = (float) (elapsed / this.currentTickTimeSpan_);
    }

    // Update events
    {
      var currentEventNode = this.allIncompleteEvents_.First;
      while (currentEventNode != null) {
        var nextEventNode = currentEventNode.Next;

        var gawgEvent = currentEventNode.Value;
        gawgEvent.Tick();
        if (gawgEvent.State == GawgEventState.COMPLETE) {
          this.allIncompleteEvents_.Remove(currentEventNode);
        }

        currentEventNode = nextEventNode;
      }
    }
  }
}