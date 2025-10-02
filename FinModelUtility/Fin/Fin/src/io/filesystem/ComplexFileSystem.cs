using System;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace fin.io.filesystem;

public sealed partial class ComplexFileSystem : IFileSystem {
  public const char IMAGINARY_DRIVE = '_';

  private readonly FileSystem real_ = new();
  private readonly MockFileSystem imaginary_ = new();

  private IFileSystem currentFileSystem_;

  public ComplexFileSystem() {
    this.Directory = new DirectoryImpl(this);
    this.File = new FileImpl(this);

    this.currentFileSystem_ = this.real_;
  }

  private IFileSystem GetFileSystemForPath_(string path) {
    if (path is [var driveChar, ':', ..]) {
      return driveChar == IMAGINARY_DRIVE ? this.imaginary_ : this.real_;
    }

    return this.currentFileSystem_;
  }

  public IDirectoryInfoFactory DirectoryInfo
    => throw new NotImplementedException();

  public IDriveInfoFactory DriveInfo
    => throw new NotImplementedException();

  public IFileInfoFactory FileInfo
    => throw new NotImplementedException();

  public IFileStreamFactory FileStream
    => throw new NotImplementedException();

  public IFileSystemWatcherFactory FileSystemWatcher
    => throw new NotImplementedException();

  public IFileVersionInfoFactory FileVersionInfo
    => throw new NotImplementedException();

  public IPath Path
    => throw new NotImplementedException();
}