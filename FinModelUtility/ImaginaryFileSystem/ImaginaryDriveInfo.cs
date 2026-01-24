namespace System.IO.Abstractions.TestingHelpers;

/// <inheritdoc />
#if FEATURE_SERIALIZABLE
[Serializable]
#endif
public class ImaginaryDriveInfo : DriveInfoBase {
  private readonly IImaginaryFileDataAccessor imaginaryFileDataAccessor_;
  private readonly string name_;

  /// <inheritdoc />
  public ImaginaryDriveInfo(IImaginaryFileDataAccessor imaginaryFileDataAccessor,
                       string name) : base(
      imaginaryFileDataAccessor?.FileSystem) {
    this.imaginaryFileDataAccessor_ = imaginaryFileDataAccessor ??
                                      throw new ArgumentNullException(
                                          nameof(imaginaryFileDataAccessor));
    this.name_ = imaginaryFileDataAccessor.ImaginaryPathVerifier.NormalizeDriveName(name);
  }

  /// <inheritdoc />
  public override long AvailableFreeSpace {
    get {
      var mockDriveData = this.GetMockDriveData_();
      return mockDriveData.AvailableFreeSpace;
    }
  }

  /// <inheritdoc />
  public override string DriveFormat {
    get {
      var mockDriveData = this.GetMockDriveData_();
      return mockDriveData.DriveFormat;
    }
  }

  /// <inheritdoc />
  public override DriveType DriveType {
    get {
      var mockDriveData = this.GetMockDriveData_();
      return mockDriveData.DriveType;
    }
  }

  /// <inheritdoc />
  public override bool IsReady {
    get {
      var mockDriveData = this.GetMockDriveData_();
      return mockDriveData.IsReady;
    }
  }

  /// <inheritdoc />
  public override string Name {
    get { return this.name_; }
  }

  /// <inheritdoc />
  public override IDirectoryInfo RootDirectory {
    get { return this.imaginaryFileDataAccessor_.DirectoryInfo.New(Name); }
  }

  /// <inheritdoc />
  public override long TotalFreeSpace {
    get {
      var mockDriveData = this.GetMockDriveData_();
      return mockDriveData.TotalFreeSpace;
    }
  }

  /// <inheritdoc />
  public override long TotalSize {
    get {
      var mockDriveData = this.GetMockDriveData_();
      return mockDriveData.TotalSize;
    }
  }

  /// <inheritdoc />
  public override string VolumeLabel {
    get {
      var mockDriveData = this.GetMockDriveData_();
      return mockDriveData.VolumeLabel;
    }
    set {
      var mockDriveData = this.GetMockDriveData_();
      mockDriveData.VolumeLabel = value;
    }
  }

  /// <inheritdoc />
  public override string ToString() {
    return Name;
  }

  private ImaginaryDriveData GetMockDriveData_() {
    return this.imaginaryFileDataAccessor_.GetDrive(this.name_) ??
           throw CommonExceptions.FileNotFound(this.name_);
  }
}