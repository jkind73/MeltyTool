using System.Runtime.Versioning;
using System.Security.AccessControl;

namespace System.IO.Abstractions.TestingHelpers;

/// <inheritdoc />
#if FEATURE_SERIALIZABLE
[Serializable]
#endif
public class ImaginaryFileStream : FileSystemStream, IFileSystemAclSupport {
  /// <summary>
  ///     Wrapper around a <see cref="Stream" /> with no backing store, which
  ///     is used as a replacement for a <see cref="FileSystemStream" />. As such
  ///     it implements the same properties and methods as a <see cref="FileSystemStream" />.
  /// </summary>
  public new static FileSystemStream Null { get; } = new NullFileSystemStream();

  private sealed class NullFileSystemStream : FileSystemStream {
    /// <summary>
    /// Initializes a new instance of <see cref="NullFileSystemStream" />.
    /// </summary>
    public NullFileSystemStream() : base(Null, ".", true) { }
  }

  private readonly IImaginaryFileDataAccessor imaginaryFileDataAccessor_;
  private readonly string path_;
  private readonly FileAccess access_ = FileAccess.ReadWrite;
  private readonly FileOptions options_;
  private readonly ImaginaryFileData fileData_;
  private bool disposed_;

  /// <inheritdoc />
  public ImaginaryFileStream(
      IImaginaryFileDataAccessor imaginaryFileDataAccessor,
      string path,
      FileMode mode,
      FileAccess access = FileAccess.ReadWrite,
      FileOptions options = FileOptions.None)
      : base(new MemoryStream(),
             path == null ? null : Path.GetFullPath(path),
             (options & FileOptions.Asynchronous) != 0) {
    ThrowIfInvalidModeAccess_(mode, access);

    this.imaginaryFileDataAccessor_ = imaginaryFileDataAccessor ??
                                      throw new ArgumentNullException(
                                          nameof(imaginaryFileDataAccessor));
    this.path_ = path;
    this.options_ = options;

    if (imaginaryFileDataAccessor.FileExists(path)) {
      if (mode.Equals(FileMode.CreateNew)) {
        throw CommonExceptions.FileAlreadyExists(path);
      }

      this.fileData_ = imaginaryFileDataAccessor.GetFile(path);
      this.fileData_.CheckFileAccess(path, access);

      var timeAdjustments
          = GetTimeAdjustmentsForFileStreamWhenFileExists_(mode, access);
      imaginaryFileDataAccessor.AdjustTimes(this.fileData_, timeAdjustments);
      var existingContents = this.fileData_.Contents;
      var keepExistingContents =
          existingContents?.Length > 0 &&
          mode != FileMode.Truncate &&
          mode != FileMode.Create;
      if (keepExistingContents) {
        base.Write(existingContents, 0, existingContents.Length);
        base.Seek(0,
                  mode == FileMode.Append
                      ? SeekOrigin.End
                      : SeekOrigin.Begin);
      }
    } else {
      var directoryPath = imaginaryFileDataAccessor.Path.GetDirectoryName(path);
      if (!string.IsNullOrEmpty(directoryPath) &&
          !imaginaryFileDataAccessor.Directory.Exists(directoryPath)) {
        throw CommonExceptions.CouldNotFindPartOfPath(path);
      }

      if (mode.Equals(FileMode.Open) || mode.Equals(FileMode.Truncate)) {
        throw CommonExceptions.FileNotFound(path);
      }

      this.fileData_ = new ImaginaryFileData(new byte[] { });
      imaginaryFileDataAccessor.AdjustTimes(this.fileData_,
                                            TimeAdjustments.CREATION_TIME |
                                            TimeAdjustments.LAST_ACCESS_TIME);
      imaginaryFileDataAccessor.AddFile(path, this.fileData_);
    }

    this.access_ = access;
  }

  private static void
      ThrowIfInvalidModeAccess_(FileMode mode, FileAccess access) {
    if (mode == FileMode.Append) {
      if (access == FileAccess.Read) {
        throw CommonExceptions.InvalidAccessCombination(mode, access);
      }

      if (access != FileAccess.Write) {
        throw CommonExceptions.AppendAccessOnlyInWriteOnlyMode();
      }
    }

    if (!access.HasFlag(FileAccess.Write) &&
        (mode == FileMode.Truncate ||
         mode == FileMode.CreateNew ||
         mode == FileMode.Create ||
         mode == FileMode.Append)) {
      throw CommonExceptions.InvalidAccessCombination(mode, access);
    }
  }

  /// <inheritdoc />
  public override bool CanRead => this.access_.HasFlag(FileAccess.Read);

  /// <inheritdoc />
  public override bool CanWrite => this.access_.HasFlag(FileAccess.Write);

  /// <inheritdoc />
  public override int Read(byte[] buffer, int offset, int count) {
    this.imaginaryFileDataAccessor_.AdjustTimes(this.fileData_,
                                                TimeAdjustments.LAST_ACCESS_TIME);
    return base.Read(buffer, offset, count);
  }

  /// <inheritdoc />
  protected override void Dispose(bool disposing) {
    if (this.disposed_) {
      return;
    }

    this.InternalFlush_();
    base.Dispose(disposing);
    OnClose();
    this.disposed_ = true;
  }

  /// <inheritdoc cref="FileSystemStream.EndWrite(IAsyncResult)" />
  public override void EndWrite(IAsyncResult asyncResult) {
    if (!CanWrite) {
      throw new NotSupportedException("Stream does not support writing.");
    }

    base.EndWrite(asyncResult);
  }

  /// <inheritdoc />
  public override void SetLength(long value) {
    if (!CanWrite) {
      throw new NotSupportedException("Stream does not support writing.");
    }

    base.SetLength(value);
  }

  /// <inheritdoc cref="FileSystemStream.Write(byte[], int, int)" />
  public override void Write(byte[] buffer, int offset, int count) {
    if (!CanWrite) {
      throw new NotSupportedException("Stream does not support writing.");
    }

    this.imaginaryFileDataAccessor_.AdjustTimes(this.fileData_,
                                                TimeAdjustments.LAST_ACCESS_TIME |
                                                TimeAdjustments.LAST_WRITE_TIME);
    base.Write(buffer, offset, count);
  }

#if FEATURE_SPAN
        /// <inheritdoc />
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (!CanWrite)
            {
                throw new NotSupportedException("Stream does not support writing.");
            }
            imaginaryFileDataAccessor_.AdjustTimes(this.fileData_,
                TimeAdjustments.LAST_ACCESS_TIME | TimeAdjustments.LAST_WRITE_TIME);
            base.Write(buffer);
        }
#endif

  /// <inheritdoc cref="FileSystemStream.WriteAsync(byte[], int, int, CancellationToken)" />
  public override Task WriteAsync(byte[] buffer,
                                  int offset,
                                  int count,
                                  CancellationToken cancellationToken) {
    if (!CanWrite) {
      throw new NotSupportedException("Stream does not support writing.");
    }

    this.imaginaryFileDataAccessor_.AdjustTimes(this.fileData_,
                                                TimeAdjustments.LAST_ACCESS_TIME |
                                                TimeAdjustments.LAST_WRITE_TIME);
    return base.WriteAsync(buffer, offset, count, cancellationToken);
  }

#if FEATURE_SPAN
        /// <inheritdoc />
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer,
                                             CancellationToken cancellationToken
 = new())
        {
            if (!CanWrite)
            {
                throw new NotSupportedException("Stream does not support writing.");
            }
            imaginaryFileDataAccessor_.AdjustTimes(this.fileData_,
                                                   TimeAdjustments.LAST_ACCESS_TIME | TimeAdjustments.LAST_WRITE_TIME);
            return base.WriteAsync(buffer, cancellationToken);
        }
#endif

  /// <inheritdoc cref="FileSystemStream.WriteByte(byte)" />
  public override void WriteByte(byte value) {
    if (!CanWrite) {
      throw new NotSupportedException("Stream does not support writing.");
    }

    this.imaginaryFileDataAccessor_.AdjustTimes(this.fileData_,
                                                TimeAdjustments.LAST_ACCESS_TIME |
                                                TimeAdjustments.LAST_WRITE_TIME);
    base.WriteByte(value);
  }

  /// <inheritdoc />
  public override void Flush() {
    this.InternalFlush_();
  }

  /// <inheritdoc />
  public override void Flush(bool flushToDisk)
    => this.InternalFlush_();

  /// <inheritdoc />
  public override Task FlushAsync(CancellationToken cancellationToken) {
    this.InternalFlush_();
    return Task.CompletedTask;
  }

  /// <inheritdoc cref="IFileSystemAclSupport.GetAccessControl()" />
  [SupportedOSPlatform("windows")]
  public object GetAccessControl() {
    return this.GetMockFileData_().AccessControl;
  }

  /// <inheritdoc cref="IFileSystemAclSupport.GetAccessControl(IFileSystemAclSupport.AccessControlSections)" />
  [SupportedOSPlatform("windows")]
  public object GetAccessControl(
      IFileSystemAclSupport.AccessControlSections includeSections) {
    return this.GetMockFileData_().AccessControl;
  }

  /// <inheritdoc cref="IFileSystemAclSupport.SetAccessControl(object)" />
  [SupportedOSPlatform("windows")]
  public void SetAccessControl(object value) {
    this.GetMockFileData_().AccessControl = value as FileSecurity;
  }

  private ImaginaryFileData GetMockFileData_() {
    return this.imaginaryFileDataAccessor_.GetFile(this.path_) ??
           throw CommonExceptions.FileNotFound(this.path_);
  }

  private void InternalFlush_() {
    if (this.imaginaryFileDataAccessor_.FileExists(this.path_)) {
      var mockFileData = this.imaginaryFileDataAccessor_.GetFile(this.path_);
      /* reset back to the beginning .. */
      var position = Position;
      Seek(0, SeekOrigin.Begin);
      /* .. read everything out */
      var data = new byte[Length];
      _ = Read(data, 0, (int) Length);
      /* restore to original position */
      Seek(position, SeekOrigin.Begin);
      /* .. put it in the mock system */
      mockFileData.Contents = data;
    }
  }

  private void OnClose() {
    if (this.options_.HasFlag(FileOptions.DeleteOnClose) &&
        this.imaginaryFileDataAccessor_.FileExists(this.path_)) {
      this.imaginaryFileDataAccessor_.RemoveFile(this.path_);
    }

    if (this.options_.HasFlag(FileOptions.Encrypted) &&
        this.imaginaryFileDataAccessor_.FileExists(this.path_)) {
#pragma warning disable CA1416 // Ignore SupportedOSPlatform for testing helper encryption
      this.imaginaryFileDataAccessor_.FileInfo.New(this.path_).Encrypt();
#pragma warning restore CA1416
    }
  }

  private static TimeAdjustments GetTimeAdjustmentsForFileStreamWhenFileExists_(
      FileMode mode,
      FileAccess access) {
    switch (mode) {
      case FileMode.Append:
      case FileMode.CreateNew:
        if (access.HasFlag(FileAccess.Read)) {
          return TimeAdjustments.LAST_ACCESS_TIME;
        }

        return TimeAdjustments.NONE;
      case FileMode.Create:
      case FileMode.Truncate:
        if (access.HasFlag(FileAccess.Write)) {
          return TimeAdjustments.LAST_ACCESS_TIME | TimeAdjustments.LAST_WRITE_TIME;
        }

        return TimeAdjustments.LAST_ACCESS_TIME;
      case FileMode.Open:
      case FileMode.OpenOrCreate:
      default:
        return TimeAdjustments.NONE;
    }
  }
}