using fin.util.types;

namespace uni.services;

public record Announcement(string Message, Exception[]? Exceptions = null);

[IocCandiate]
public static class AnnouncementService {
  static AnnouncementService() {
    ModelService.OnModelSuccessfullyOpened
        += (_, _) => DisplayAnnouncement(null);
    SceneService.OnSceneSuccessfullyOpened
        += (_, _) => DisplayAnnouncement(null);

    ModelService.OnModelFailedToOpen
        += (fileBundle, exception) => DisplayAnnouncement(
            new Announcement(
                fileBundle != null
                    ? $"Failed to open model: {fileBundle.DisplayFullPath}"
                    : "Failed to open model.",
                [exception]));
    SceneService.OnSceneFailedToOpen
        += (fileBundle, exception) => DisplayAnnouncement(
            new Announcement(
                fileBundle != null
                    ? $"Failed to open scene: {fileBundle.DisplayFullPath}"
                    : "Failed to open scene.",
                [exception]));  }

  public static event Action<Announcement?> OnAnnouncement;

  public static void DisplayAnnouncement(Announcement? announcement)
    => OnAnnouncement?.Invoke(announcement);
}