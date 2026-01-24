using fin.io;
using fin.util.enumerables;
using fin.util.linq;

using Gameloop.Vdf;
using Gameloop.Vdf.Linq;

namespace uni.platforms.desktop;

internal static class SteamUtils {
  private static string? InstallPath { get; } =
    RegistryExtensions.GetSoftwareValueEither32Or64Bit(
        @"Valve\Steam",
        "InstallPath") as string;

  private static IReadOnlySystemDirectory? InstallDirectory { get; } =
    InstallPath != null
        ? new FinDirectory(InstallPath)
        : null;

  private static IReadOnlySystemFile? LibraryFoldersVdf { get; } =
    (InstallDirectory?.TryToGetExistingFile(
         "config/libraryfolders.vdf",
         out var libraryFoldersVdf) ??
     false)
        ? libraryFoldersVdf
        : null;

  private static ISystemDirectory[] Libraries { get; } =
    InitializeLibraries_();

  private static ISystemDirectory[] InitializeLibraries_() {
    if (!(LibraryFoldersVdf?.Exists ?? false)) {
      return [];
    }

    using var ls = LibraryFoldersVdf.OpenReadAsText();
    var root = VdfConvert.Deserialize(ls);

    if (root.Key != "libraryfolders") {
      return [];
    }

    return root.Value.Children()
               .Select(section => {
                 try {
                   return section.Value<VProperty>()
                                 .Value["path"]
                                 ?.ToString();
                 } catch {
                   return null;
                 }
               })
               .Nonnull()
               .Select(path => new FinDirectory(path))
               .CastTo<FinDirectory, ISystemDirectory>()
               // A steam library directory may not exist if it lives on an
               // external hard drive
               .Where(steamDirectory => steamDirectory.Exists)
               .ToArray();
  }

  private static ISystemDirectory[] CommonDirectories { get; } =
    Libraries
        .Select(
            dir => dir.TryToGetExistingSubdir("steamapps", out var steamappsDir)
                ? steamappsDir
                : dir)
        .Select(
            dir => dir.TryToGetExistingSubdir("common", out var commonDir)
                ? commonDir
                : dir)
        .ToArray();

  public static ISystemDirectory[] GameDirectories { get; }
    = CommonDirectories
      .SelectMany(common => common.GetExistingSubdirs())
      .ToArray();

  public static bool TryGetGameDirectory(string name,
                                         out ISystemDirectory directory)
    => GameDirectories.ByName(name).TryGetFirst(out directory);
}