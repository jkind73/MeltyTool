using System.Text.RegularExpressions;

namespace System.IO.Abstractions.TestingHelpers;

/// <summary>
/// Provides helper methods for handling paths in a portable way.
/// </summary>
public static class ImaginaryUnixSupport {
  private static readonly Regex PATH_TRANSFORM_
      = new Regex(@"^[a-zA-Z]:(?<path>.*)$");

  /// <summary>
  /// Normalizes the given path so that it works on all platfoms.
  /// </summary>
  public static string Path(string path) => path != null && IsUnixPlatform()
      ? PATH_TRANSFORM_.Replace(path, "${path}").Replace(@"\", "/")
      : path;

  /// <summary>
  /// Determines whether the current runtime platform is Unix.
  /// </summary>
  public static bool IsUnixPlatform() => IO.Path.DirectorySeparatorChar == '/';

  /// <summary>
  /// Determines whether the current runtime platform is Windows.
  /// </summary>
  public static bool IsWindowsPlatform()
    => IO.Path.DirectorySeparatorChar == '\\';
}