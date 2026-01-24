using fin.common;
using fin.io;

namespace uni.platforms.wii.tools;

public static class WiiToolsConstants {
  public static IReadOnlySystemDirectory WitDirectory { get; } =
    DirectoryConstants.toolsDirectory.AssertGetExistingSubdir("wit");

  public static IReadOnlySystemFile WitExe { get; } =
    WitDirectory.AssertGetExistingFile("wit.exe");
}