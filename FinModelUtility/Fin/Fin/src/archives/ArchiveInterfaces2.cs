using System;
using System.Collections.Generic;
using System.IO;

using fin.importers;
using fin.io.bundles;

namespace fin.archives;

public interface IArchiveImporter2<in TArchiveFileBundle>
    : IImporter<IArchive2, TArchiveFileBundle>
    where TArchiveFileBundle : IArchiveFileBundle2;

public interface IArchiveFileBundle2 : IFileBundle {
  FileBundleType IFileBundle.Type => FileBundleType.ARCHIVE;
}

public interface IArchive2 : IResource, IDisposable {
  IArchiveDirectory2 Root { get; }
}

public interface IArchiveDirectory2 {
  string Name { get; }

  IEnumerable<IArchiveDirectory2> Subdirs { get; }
  IEnumerable<IArchiveFile2> Files { get; }
}

public interface IArchiveFile2 {
  string Name { get; }
  Stream OpenRead();
}