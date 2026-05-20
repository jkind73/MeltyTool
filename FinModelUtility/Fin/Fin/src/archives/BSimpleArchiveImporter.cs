using System.Collections.Generic;
using System.IO;
using System.Linq;

using fin.io;
using fin.io.bundles;

using SubstreamSharp;

namespace fin.archives;

public interface ISimpleArchiveDirectory : IArchiveDirectory2 {
  ISimpleArchiveDirectory AddSubdir(string name);
  void AddFile(string name, long position, long length);
}

public abstract class BSimpleArchiveImporter<TBundle>
    : IArchiveImporter2<TBundle>
    where TBundle : IArchiveFileBundle2 {
  protected abstract Stream BuildHierarchyAndGetFileStream(
      TBundle bundle,
      ISet<IReadOnlyGenericFile> fileSet,
      ISimpleArchiveDirectory builderRoot);

  public IArchive2 Import(TBundle fileBundle) {
    var fileSet = fileBundle.Files.ToHashSet();

    var archive = new SimpleArchive {
        FileBundle = fileBundle,
        Files = fileSet,
    };

    var root = new SimpleArchiveDirectory(archive, "");
    archive.Root = root;
    archive.Stream
        = this.BuildHierarchyAndGetFileStream(fileBundle, fileSet, root);

    return archive;
  }

  private sealed class SimpleArchive : IArchive2 {
    public required IFileBundle FileBundle { get; init; }
    public required IReadOnlySet<IReadOnlyGenericFile> Files { get; init; }

    public IArchiveDirectory2 Root { get; set; }
    public Stream Stream { get; set; }

    public void Dispose() => this.Stream.Dispose();
  }

  private sealed class SimpleArchiveDirectory(
      SimpleArchive archive,
      string name)
      : ISimpleArchiveDirectory {
    private readonly List<IArchiveDirectory2> subdirsImpl_ = new();
    private readonly List<IArchiveFile2> filesImpl_ = new();

    public string Name => name;
    public IReadOnlyList<IArchiveDirectory2> Subdirs => this.subdirsImpl_;
    public IReadOnlyList<IArchiveFile2> Files => this.filesImpl_;

    public ISimpleArchiveDirectory AddSubdir(string name) {
      var subdir = new SimpleArchiveDirectory(archive, name);
      this.subdirsImpl_.Add(subdir);
      return subdir;
    }

    public void AddFile(string name, long position, long length)
      => this.filesImpl_.Add(
          new SimpleArchiveFile(archive, name, position, length));
  }

  private sealed class SimpleArchiveFile(
      SimpleArchive archive,
      string name,
      long position,
      long length) : IArchiveFile2 {
    public string Name => name;

    public Stream OpenRead() => archive.Stream.Substream(position, length);
  }
}