using System.Runtime.Versioning;
using System.Security.AccessControl;

namespace System.IO.Abstractions.TestingHelpers;

/// <inheritdoc />
#if FEATURE_SERIALIZABLE
[Serializable]
#endif
public class ImaginaryDirectoryInfo : DirectoryInfoBase, IFileSystemAclSupport {
  private readonly IImaginaryFileDataAccessor imaginaryFileDataAccessor_;
  private string directoryPath_;
  private string originalPath_;
  private ImaginaryFileData cachedImaginaryFileData_;
  private bool refreshOnNextRead_;

  /// <summary>
  /// Initializes a new instance of the <see cref="ImaginaryDirectoryInfo"/> class.
  /// </summary>
  /// <param name="imaginaryFileDataAccessor">The mock file data accessor.</param>
  /// <param name="directoryPath">The directory path.</param>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="imaginaryFileDataAccessor"/> or <paramref name="directoryPath"/> is <see langref="null"/>.</exception>
  public ImaginaryDirectoryInfo(
      IImaginaryFileDataAccessor imaginaryFileDataAccessor,
      string directoryPath) : base(
      imaginaryFileDataAccessor?.FileSystem) {
    this.imaginaryFileDataAccessor_ = imaginaryFileDataAccessor ??
                                      throw new ArgumentNullException(
                                          nameof(imaginaryFileDataAccessor));

    if (directoryPath == null) {
      throw new ArgumentNullException("path",
                                      StringResources.Manager.GetString(
                                          "VALUE_CANNOT_BE_NULL"));
    }

    if (directoryPath.Trim() == string.Empty) {
      throw CommonExceptions.PathIsNotOfALegalForm("path");
    }

    this.SetDirectoryPath_(directoryPath);
    Refresh();
  }

#if FEATURE_CREATE_SYMBOLIC_LINK
  /// <inheritdoc />
  public override void CreateAsSymbolicLink(string pathToTarget) {
    FileSystem.Directory.CreateSymbolicLink(FullName, pathToTarget);
  }
#endif

  /// <inheritdoc />
  public override void Delete() {
    this.imaginaryFileDataAccessor_.Directory.Delete(this.directoryPath_);
    this.refreshOnNextRead_ = true;
  }

  /// <inheritdoc />
  public override void Refresh() {
    var mockFileData = this.imaginaryFileDataAccessor_.GetFile(this.directoryPath_) ??
                       ImaginaryFileData.NULL_OBJECT_;
    this.cachedImaginaryFileData_ = mockFileData.Clone();
  }

#if FEATURE_CREATE_SYMBOLIC_LINK
  /// <inheritdoc />
  public override IFileSystemInfo ResolveLinkTarget(bool returnFinalTarget) {
    return FileSystem.Directory.ResolveLinkTarget(FullName, returnFinalTarget);
  }
#endif

  /// <inheritdoc />
  public override FileAttributes Attributes {
    get { return this.GetMockFileDataForRead_().Attributes; }
    set {
      this.GetMockFileDataForWrite_().Attributes = value | FileAttributes.Directory;
    }
  }

  /// <inheritdoc />
  public override DateTime CreationTime {
    get { return this.GetMockFileDataForRead_().CreationTime.LocalDateTime; }
    set { this.GetMockFileDataForWrite_().CreationTime = value; }
  }

  /// <inheritdoc />
  public override DateTime CreationTimeUtc {
    get { return this.GetMockFileDataForRead_().CreationTime.UtcDateTime; }
    set { this.GetMockFileDataForWrite_().CreationTime = value; }
  }

  /// <inheritdoc />
  public override bool Exists {
    get {
      var mockFileData = this.GetMockFileDataForRead_();
      return (int) mockFileData.Attributes != -1 && mockFileData.IsDirectory;
    }
  }

  /// <inheritdoc />
  public override string Extension {
    get {
      // System.IO.Path.GetExtension does only string manipulation,
      // so it's safe to delegate.
      return Path.GetExtension(this.directoryPath_);
    }
  }

  /// <inheritdoc />
  public override string FullName {
    get {
      var root
          = this.imaginaryFileDataAccessor_.Path.GetPathRoot(this.directoryPath_);

      if (this.imaginaryFileDataAccessor_.StringOperations.Equals(
              this.directoryPath_,
              root)) {
        // drives have the trailing slash
        return this.directoryPath_;
      }

      // directories do not have a trailing slash
      return this.directoryPath_.TrimEnd('\\').TrimEnd('/');
    }
  }

  /// <inheritdoc />
  public override DateTime LastAccessTime {
    get { return this.GetMockFileDataForRead_().LastAccessTime.LocalDateTime; }
    set { this.GetMockFileDataForWrite_().LastAccessTime = value; }
  }

  /// <inheritdoc />
  public override DateTime LastAccessTimeUtc {
    get { return this.GetMockFileDataForRead_().LastAccessTime.UtcDateTime; }
    set { this.GetMockFileDataForWrite_().LastAccessTime = value; }
  }

  /// <inheritdoc />
  public override DateTime LastWriteTime {
    get { return this.GetMockFileDataForRead_().LastWriteTime.LocalDateTime; }
    set { this.GetMockFileDataForWrite_().LastWriteTime = value; }
  }

  /// <inheritdoc />
  public override DateTime LastWriteTimeUtc {
    get { return this.GetMockFileDataForRead_().LastWriteTime.UtcDateTime; }
    set { this.GetMockFileDataForWrite_().LastWriteTime = value; }
  }

#if FEATURE_FILE_SYSTEM_INFO_LINK_TARGET
  /// <inheritdoc />
  public override string LinkTarget {
    get { return this.GetMockFileDataForRead_().LinkTarget; }
  }
#endif

  /// <inheritdoc />
  public override string Name {
    get {
      var mockPath = new ImaginaryPath(this.imaginaryFileDataAccessor_);
      return string.Equals(mockPath.GetPathRoot(this.directoryPath_), this.directoryPath_)
          ? this.directoryPath_
          : mockPath.GetFileName(this.directoryPath_.TrimEnd(
                                     this.imaginaryFileDataAccessor_.Path
                                         .DirectorySeparatorChar));
    }
  }

  /// <inheritdoc />
  public override void Create() {
    this.imaginaryFileDataAccessor_.Directory.CreateDirectory(FullName);
    this.refreshOnNextRead_ = true;
  }

  /// <inheritdoc />
  public override IDirectoryInfo CreateSubdirectory(string path) {
    return this.imaginaryFileDataAccessor_.Directory.CreateDirectory(
        Path.Combine(FullName, path));
  }

  /// <inheritdoc />
  public override void Delete(bool recursive) {
    this.imaginaryFileDataAccessor_.Directory.Delete(this.directoryPath_, recursive);
    this.refreshOnNextRead_ = true;
  }

  /// <inheritdoc />
  public override IEnumerable<IDirectoryInfo> EnumerateDirectories() {
    return GetDirectories();
  }

  /// <inheritdoc />
  public override IEnumerable<IDirectoryInfo> EnumerateDirectories(
      string searchPattern) {
    return GetDirectories(searchPattern);
  }

  /// <inheritdoc />
  public override IEnumerable<IDirectoryInfo> EnumerateDirectories(
      string searchPattern,
      SearchOption searchOption) {
    return GetDirectories(searchPattern, searchOption);
  }

#if FEATURE_ENUMERATION_OPTIONS
  /// <inheritdoc />
  public override IEnumerable<IDirectoryInfo> EnumerateDirectories(
      string searchPattern,
      EnumerationOptions enumerationOptions) {
    return GetDirectories(searchPattern, enumerationOptions);
  }
#endif

  /// <inheritdoc />
  public override IEnumerable<IFileInfo> EnumerateFiles() {
    return GetFiles();
  }

  /// <inheritdoc />
  public override IEnumerable<IFileInfo> EnumerateFiles(string searchPattern) {
    return GetFiles(searchPattern);
  }

  /// <inheritdoc />
  public override IEnumerable<IFileInfo> EnumerateFiles(
      string searchPattern,
      SearchOption searchOption) {
    return GetFiles(searchPattern, searchOption);
  }

#if FEATURE_ENUMERATION_OPTIONS
  /// <inheritdoc />
  public override IEnumerable<IFileInfo> EnumerateFiles(
      string searchPattern,
      EnumerationOptions enumerationOptions) {
    return GetFiles(searchPattern, enumerationOptions);
  }
#endif

  /// <inheritdoc />
  public override IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos() {
    return GetFileSystemInfos();
  }

  /// <inheritdoc />
  public override IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(
      string searchPattern) {
    return GetFileSystemInfos(searchPattern);
  }

  /// <inheritdoc />
  public override IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(
      string searchPattern,
      SearchOption searchOption) {
    return GetFileSystemInfos(searchPattern, searchOption);
  }

#if FEATURE_ENUMERATION_OPTIONS
  /// <inheritdoc />
  public override IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(
      string searchPattern,
      EnumerationOptions enumerationOptions) {
    return GetFileSystemInfos(searchPattern, enumerationOptions);
  }
#endif

  /// <inheritdoc />
  public override IDirectoryInfo[] GetDirectories() {
    return this.ConvertStringsToDirectories_(
        this.imaginaryFileDataAccessor_.Directory
            .GetDirectories(this.directoryPath_));
  }

  /// <inheritdoc />
  public override IDirectoryInfo[] GetDirectories(string searchPattern) {
    return this.ConvertStringsToDirectories_(
        this.imaginaryFileDataAccessor_.Directory.GetDirectories(
            this.directoryPath_,
            searchPattern));
  }

  /// <inheritdoc />
  public override IDirectoryInfo[] GetDirectories(
      string searchPattern,
      SearchOption searchOption) {
    return this.ConvertStringsToDirectories_(
        this.imaginaryFileDataAccessor_.Directory.GetDirectories(
            this.directoryPath_,
            searchPattern,
            searchOption));
  }

#if FEATURE_ENUMERATION_OPTIONS
  /// <inheritdoc />
  public override IDirectoryInfo[] GetDirectories(
      string searchPattern,
      EnumerationOptions enumerationOptions) {
    return this.ConvertStringsToDirectories_(
        imaginaryFileDataAccessor_.Directory.GetDirectories(
            this.directoryPath_,
            searchPattern,
            enumerationOptions));
  }
#endif

  private DirectoryInfoBase[] ConvertStringsToDirectories_(
      IEnumerable<string> paths) {
    return paths
           .Select(path => new ImaginaryDirectoryInfo(
                       this.imaginaryFileDataAccessor_,
                       path))
           .Cast<DirectoryInfoBase>()
           .ToArray();
  }

  /// <inheritdoc />
  public override IFileInfo[] GetFiles() {
    return this.ConvertStringsToFiles_(
        this.imaginaryFileDataAccessor_.Directory.GetFiles(FullName));
  }

  /// <inheritdoc />
  public override IFileInfo[] GetFiles(string searchPattern) {
    return this.ConvertStringsToFiles_(
        this.imaginaryFileDataAccessor_.Directory.GetFiles(
            FullName,
            searchPattern));
  }

  /// <inheritdoc />
  public override IFileInfo[] GetFiles(string searchPattern,
                                       SearchOption searchOption) {
    return this.ConvertStringsToFiles_(
        this.imaginaryFileDataAccessor_.Directory.GetFiles(
            FullName,
            searchPattern,
            searchOption));
  }

#if FEATURE_ENUMERATION_OPTIONS
  /// <inheritdoc />
  public override IFileInfo[] GetFiles(string searchPattern,
                                       EnumerationOptions enumerationOptions) {
    return this.ConvertStringsToFiles_(
        imaginaryFileDataAccessor_.Directory.GetFiles(
            FullName,
            searchPattern,
            enumerationOptions));
  }
#endif

  IFileInfo[] ConvertStringsToFiles_(IEnumerable<string> paths) {
    return paths
           .Select(this.imaginaryFileDataAccessor_.FileInfo.New)
           .ToArray();
  }

  /// <inheritdoc />
  public override IFileSystemInfo[] GetFileSystemInfos() {
    return GetFileSystemInfos("*");
  }

  /// <inheritdoc />
  public override IFileSystemInfo[] GetFileSystemInfos(string searchPattern) {
    return GetFileSystemInfos(searchPattern, SearchOption.TopDirectoryOnly);
  }

  /// <inheritdoc />
  public override IFileSystemInfo[] GetFileSystemInfos(
      string searchPattern,
      SearchOption searchOption) {
    return GetDirectories(searchPattern, searchOption)
           .OfType<IFileSystemInfo>()
           .Concat(GetFiles(searchPattern, searchOption))
           .ToArray();
  }

#if FEATURE_ENUMERATION_OPTIONS
  /// <inheritdoc />
  public override IFileSystemInfo[] GetFileSystemInfos(
      string searchPattern,
      EnumerationOptions enumerationOptions) {
    return GetDirectories(searchPattern, enumerationOptions)
           .OfType<IFileSystemInfo>()
           .Concat(GetFiles(searchPattern, enumerationOptions))
           .ToArray();
  }
#endif

  /// <inheritdoc />
  public override void MoveTo(string destDirName) {
    this.imaginaryFileDataAccessor_.Directory.Move(this.directoryPath_, destDirName);
    this.SetDirectoryPath_(destDirName);
  }

  /// <inheritdoc />
  public override IDirectoryInfo Parent {
    get {
      return this.imaginaryFileDataAccessor_.Directory.GetParent(this.directoryPath_);
    }
  }

  /// <inheritdoc />
  public override IDirectoryInfo Root {
    get {
      return new ImaginaryDirectoryInfo(this.imaginaryFileDataAccessor_,
                                        this.imaginaryFileDataAccessor_
                                            .Directory
                                            .GetDirectoryRoot(FullName));
    }
  }

  private ImaginaryFileData GetMockFileDataForRead_() {
    if (this.refreshOnNextRead_) {
      Refresh();
      this.refreshOnNextRead_ = false;
    }

    return this.cachedImaginaryFileData_;
  }

  private ImaginaryFileData GetMockFileDataForWrite_() {
    this.refreshOnNextRead_ = true;
    return this.imaginaryFileDataAccessor_.GetFile(this.directoryPath_) ??
           throw CommonExceptions.CouldNotFindPartOfPath(this.directoryPath_);
  }

  /// <inheritdoc />
  public override string ToString() {
    return this.originalPath_;
  }

  /// <inheritdoc cref="IFileSystemAclSupport.GetAccessControl()" />
  [SupportedOSPlatform("windows")]
  public object GetAccessControl() {
    return this.GetMockDirectoryData_().AccessControl;
  }

  /// <inheritdoc cref="IFileSystemAclSupport.GetAccessControl(IFileSystemAclSupport.AccessControlSections)" />
  [SupportedOSPlatform("windows")]
  public object GetAccessControl(
      IFileSystemAclSupport.AccessControlSections includeSections) {
    return this.GetMockDirectoryData_().AccessControl;
  }

  /// <inheritdoc cref="IFileSystemAclSupport.SetAccessControl(object)" />
  [SupportedOSPlatform("windows")]
  public void SetAccessControl(object value) {
    this.GetMockDirectoryData_().AccessControl = value as DirectorySecurity;
  }

  private void SetDirectoryPath_(string path) {
    this.originalPath_ = path;
    path = this.imaginaryFileDataAccessor_.Path.GetFullPath(path);

    path = path.TrimSlashes();
    if (ImaginaryUnixSupport.IsWindowsPlatform()) {
      path = path.TrimEnd(' ');
    }

    this.directoryPath_ = path;
  }

  private ImaginaryDirectoryData GetMockDirectoryData_() {
    return this.imaginaryFileDataAccessor_.GetFile(this.directoryPath_) as
               ImaginaryDirectoryData ??
           throw CommonExceptions.CouldNotFindPartOfPath(this.directoryPath_);
  }
}