using System.IO;
using System.Linq;

using fin.data.queues;
using fin.io;
using fin.io.archive;

namespace fin.archives;

public static class ArchiveExtensions2 {
  public static ArchiveExtractionResult ExtractInto<TBundle>(
      this IArchiveImporter2<TBundle> importer,
      TBundle bundle,
      ISystemDirectory directory,
      bool cleanUp = false) where TBundle : IArchiveFileBundle2 {
    if (directory is { Exists: true, IsEmpty: false }) {
      return ArchiveExtractionResult.ALREADY_EXISTS;
    }

    using var archive = importer.Import(bundle);

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

    return ArchiveExtractionResult.NEWLY_EXTRACTED;
  }
}