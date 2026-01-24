using fin.common;
using fin.io;

namespace gx.tools;

public static class GcnToolsConstants {
  public static IReadOnlySystemDirectory SzstoolsDirectory { get; } =
    DirectoryConstants.toolsDirectory.AssertGetExistingSubdir("szstools");

  public static IReadOnlySystemFile RarcdumpExe { get; } =
    SzstoolsDirectory.AssertGetExistingFile("rarcdump.exe");
}