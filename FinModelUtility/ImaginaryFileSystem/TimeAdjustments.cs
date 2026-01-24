namespace System.IO.Abstractions.TestingHelpers;

/// <summary>
/// Flags indicating which times to adjust for a <see cref="ImaginaryFileData"/>.
/// </summary>
[Flags]
public enum TimeAdjustments {
  /// <summary>
  /// Adjusts no times on the <see cref="ImaginaryFileData"/>
  /// </summary>
  NONE = 0,

  /// <summary>
  /// Adjusts the <see cref="ImaginaryFileData.CreationTime"/>
  /// </summary>
  CREATION_TIME = 1 << 0,

  /// <summary>
  /// Adjusts the <see cref="ImaginaryFileData.LastAccessTime"/>
  /// </summary>
  LAST_ACCESS_TIME = 1 << 1,

  /// <summary>
  /// Adjusts the <see cref="ImaginaryFileData.LastWriteTime"/>
  /// </summary>
  LAST_WRITE_TIME = 1 << 2,

  /// <summary>
  /// Adjusts all times on the <see cref="ImaginaryFileData"/>
  /// </summary>
  ALL = ~0
}