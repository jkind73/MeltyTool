using fin.common;
using fin.io;

namespace uni.platforms.gcn.tools;

public static class ThreeDsToolsConstants {
  public static ISystemDirectory CtrtoolDirectory { get; } =
    DirectoryConstants.toolsDirectory.AssertGetExistingSubdir("ctrtool");

  public static IReadOnlySystemFile ExtractCiaBat { get; } =
    CtrtoolDirectory.AssertGetExistingFile(
        "extract_cia.bat");

  public static IReadOnlySystemFile ExtractCciBat { get; } =
    CtrtoolDirectory.AssertGetExistingFile(
        "extract_cci.bat");


  public static IReadOnlySystemDirectory ThreedsXsfatoolDirectory { get; } =
    DirectoryConstants.toolsDirectory.AssertGetExistingSubdir(
        "3ds-xfsatool");

  public static IReadOnlySystemFile ThreedsXsfatoolExe { get; } =
    ThreedsXsfatoolDirectory.AssertGetExistingFile(
        "3ds-xfsatool.exe");
}