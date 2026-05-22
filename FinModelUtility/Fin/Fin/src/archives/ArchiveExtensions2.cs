using System.IO;
using System.Linq;

using fin.data.queues;
using fin.io;
using fin.io.archive;
using fin.log;

namespace fin.archives;

public static class ArchiveExtensions2 {
  private static readonly ILogger LOGGER_ = Logging.Create("Archive");

  public static ArchiveExtractionResult ExtractInto<TBundle>(
      this IArchiveImporter2<TBundle> importer,
      ISystemFile file,
      ISystemDirectory directory,
      bool cleanUp = false)
      where TBundle : ISimpleArchiveFileBundle<TBundle> {
    if (directory is { Exists: true, IsEmpty: false }) {
      return ArchiveExtractionResult.ALREADY_EXISTS;
    }

    var bundle = TBundle.FromFile(file);

    directory.Create();
    using (var archive = importer.Import(bundle)) {
      LOGGER_.LogInformation(
          $"Extracting archive: {bundle.MainFile.DisplayFullPath}");

      var directoryQueue
          = new FinTuple2Queue<IArchiveDirectory2, ISystemDirectory>(
              (archive.Root, directory));
      while (directoryQueue.TryDequeue(out var archiveDir, out var finDir)) {
        foreach (var archiveFile in archiveDir.Files) {
          var finFile = new FinFile(Path.Join(finDir.FullPath, archiveFile.Name));
          using var fw = finFile.OpenWrite();
          using var fr = archiveFile.OpenRead();
          fr.CopyTo(fw);
        }

        directoryQueue.Enqueue(
            archiveDir.Subdirs.Select(child => (
                                          child,
                                          finDir.GetOrCreateSubdir(child.Name))));
      }
    }

    if (cleanUp) {
      file.Delete();
    }

    return ArchiveExtractionResult.NEWLY_EXTRACTED;
  }
}