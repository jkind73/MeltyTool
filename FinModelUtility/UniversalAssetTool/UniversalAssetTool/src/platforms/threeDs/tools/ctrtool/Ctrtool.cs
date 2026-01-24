using System.Collections.Immutable;

using fin.io;

using uni.platforms.gcn.tools;

namespace uni.platforms.threeDs.tools.ctrtool;

public static partial class Ctrtool {
  private static readonly object CTRTOOL_LOCK_ = new();

  private static readonly ImmutableHashSet<string> EXPECTED_FILE_NAMES_ =
      new[] { "ctrtool.exe", "extract_cci.bat", "extract_cia.bat", }
          .ToImmutableHashSet();

  private static void RunInCtrDirectoryAndCleanUp_(Action handler) {
    lock (CTRTOOL_LOCK_) {
      Files.RunInDirectory(ThreeDsToolsConstants.CtrtoolDirectory, handler);

      foreach (var fileToCleanUp in ThreeDsToolsConstants
                                    .CtrtoolDirectory.GetExistingFiles()
                                    .Where(file => !EXPECTED_FILE_NAMES_
                                               .Contains(file.Name.ToString()))) {
        fileToCleanUp.Delete();
      }
    }
  }
}