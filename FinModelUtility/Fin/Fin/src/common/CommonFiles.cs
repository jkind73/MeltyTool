using fin.io;

namespace fin.common;

public static class CommonFiles {
  public static IReadOnlySystemDirectory CommonDirectory { get; } =
    DirectoryConstants.BaseDirectory.AssertGetExistingSubdir("common");

  public static IReadOnlySystemFile WindowsSoundfontFile { get; } =
    CommonDirectory.AssertGetExistingFile("windows.sf2");
}