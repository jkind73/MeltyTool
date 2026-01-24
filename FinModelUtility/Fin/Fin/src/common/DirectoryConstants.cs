using System;
using System.IO;
using System.Linq;

using fin.io;

namespace fin.common;

public static class DirectoryConstants {
  public static ISystemDirectory BaseDirectory { get; } =
    GetBaseDirectory_();

  private static ISystemDirectory GetBaseDirectory_() {
    // Launched externally
    var exeDirectory = new FinDirectory(AppContext.BaseDirectory);
    if (exeDirectory.Name is "universal_asset_tool") {
      return exeDirectory.AssertGetParent()  // tools
                         .AssertGetParent(); // cli
    }

    foreach (var ancestor in Files.GetCwd().GetAncestry()) {
      var subdirByName = ancestor
                         .GetExistingSubdirs()
                         .ToDictionary(d => d.Name.ToString(), d => d);

      // Launched via Visual Studio
      if (subdirByName.ContainsKey("cli") &&
          subdirByName.ContainsKey("FinModelUtility")) {
        return subdirByName["cli"];
      }

      if (subdirByName.ContainsKey("roms") &&
          subdirByName.ContainsKey("tools")) {
        return ancestor;
      }
    }

    throw new DirectoryNotFoundException("Failed to find the base directory.");
  }

  public static ISystemDirectory GameConfigDirectory { get; } =
    BaseDirectory.AssertGetExistingSubdir("config");

  public static ISystemFile ConfigFile { get; } =
    BaseDirectory.AssertGetExistingFile("config.json");


  public static ISystemDirectory romsDirectory =
      BaseDirectory.AssertGetExistingSubdir("roms");

  public static ISystemDirectory toolsDirectory =
      BaseDirectory.AssertGetExistingSubdir("tools");

  public static ISystemDirectory dllDirectory =
      toolsDirectory.AssertGetExistingSubdir("dll");

  public static ISystemDirectory outDirectory =
      BaseDirectory.AssertGetExistingSubdir("out");
}