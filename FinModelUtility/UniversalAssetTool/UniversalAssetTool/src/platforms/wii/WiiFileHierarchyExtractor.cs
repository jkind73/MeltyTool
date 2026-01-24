using fin.common;
using fin.io;

using uni.platforms.wii.tools;

namespace uni.platforms.wii;

public sealed class WiiFileHierarchyExtractor {
  private readonly Wit wit_ = new();

  public bool TryToExtractFromGame(
      string gameName,
      out IFileHierarchy fileHierarchy) {
    if (!TryToFindRom_(gameName, out var romFile)) {
      fileHierarchy = null;
      return false;
    }

    fileHierarchy = this.ExtractFromRom(romFile);
    return true;
  }

  private static bool TryToFindRom_(string gameName, out ISystemFile romFile)
    => DirectoryConstants.romsDirectory
                         .TryToGetExistingFileWithFileType(
                             gameName,
                             out romFile,
                             ".ciso",
                             ".iso");


  public IFileHierarchy ExtractFromRom(ISystemFile romFile) {
    this.wit_.Run(romFile, out var fileHierarchy);
    return fileHierarchy;
  }
}