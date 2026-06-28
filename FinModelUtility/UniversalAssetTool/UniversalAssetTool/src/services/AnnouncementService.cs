using fin.io.web;
using fin.util.types;

namespace uni.services;

public enum AnnouncementType {
  VERBOSE,
  INFO,
  ERROR,
}

public record Announcement(
    AnnouncementType Type,
    string Message,
    (Exception, IExceptionContext?)[]? ExceptionsAndContexts = null);

[IocCandiate]
public static class AnnouncementService {
  static AnnouncementService() {
    ModelService.OnModelSuccessfullyOpened
        += (node, _) => DisplayAnnouncement(
            new Announcement(
                AnnouncementType.VERBOSE,
                node != null
                    ? $"Successfully opened model: {node.File.DisplayFullPath}"
                    : "Successfully opened model."));
    SceneService.OnSceneSuccessfullyOpened
        += (node, _) => DisplayAnnouncement(
            new Announcement(
                AnnouncementType.VERBOSE,
                node != null
                    ? $"Successfully opened scene: {node.File.DisplayFullPath}"
                    : "Successfully opened scene."));

    ModelService.OnModelFailedToOpen
        += (fileBundle, exception) => DisplayAnnouncement(
            new Announcement(
                AnnouncementType.ERROR,
                fileBundle != null
                    ? $"Failed to open model: {fileBundle.DisplayFullPath}"
                    : "Failed to open model.",
                [(exception, fileBundle != null ? new LoadFileBundleExceptionContext(fileBundle) : null)]));
    SceneService.OnSceneFailedToOpen
        += (fileBundle, exception) => DisplayAnnouncement(
            new Announcement(
                AnnouncementType.ERROR,
                fileBundle != null
                    ? $"Failed to open scene: {fileBundle.DisplayFullPath}"
                    : "Failed to open scene.",
                [(exception, fileBundle != null ? new LoadFileBundleExceptionContext(fileBundle) : null)]));  }

  public static event Action<Announcement?> OnAnnouncement;

  public static void DisplayAnnouncement(Announcement? announcement)
    => OnAnnouncement?.Invoke(announcement);
}