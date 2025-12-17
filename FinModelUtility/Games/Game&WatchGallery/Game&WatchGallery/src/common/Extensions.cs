namespace gawg.common;

public static class GawgEventManagerExtensions {
  public static IGawgEvent AddEventAfter(
      this IGawgEventManager eventManager,
      IReadOnlyGawgEvent other,
      ulong durationInTicks)
    => eventManager.AddEventRelativeToEndOf(other, 0, durationInTicks);

  public static IGawgEvent AddEventAtSameTimeAs(
      this IGawgEventManager eventManager,
      IReadOnlyGawgEvent other,
      ulong durationInTicks)
    => eventManager.AddEventRelativeToStartOf(other, 0, durationInTicks);
}