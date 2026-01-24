using System;
using System.IO;

using KSoft.Phoenix.Resource;

namespace HaloWarsTools;

public sealed class HwContext(string gameInstallDirectory, string scratchDirectory) {
  public string gameInstallDirectory = gameInstallDirectory;
  public string scratchDirectory = scratchDirectory;

  /*public Dictionary<string, HWObjectDefinition> ObjectDefinitions => ValueCache.Get(LoadObjectDefinitions);

  private Dictionary<string, HWObjectDefinition> LoadObjectDefinitions() {
    var manifest = new Dictionary<string, HWObjectDefinition>();

    var source = "data\\objects.xml";

    var objects = XElement.Load(GetAbsoluteScratchPath(source)).Descendants("Object");
    foreach (var obj in objects) {
      if (obj.Attribute("name") != null) {
        var def = new HWObjectDefinition {
            Name = obj.Attribute("name").Value
        };
        var vis = obj.Descendants().FirstOrDefault(xmlElement => xmlElement.Name == "Visual");
        if (vis != null) {
          def.Visual = HWVisResource.FromFile(this, Path.Combine("art", vis.Value));
        }
        manifest.Add(def.Name, def);
      }
    }

    return manifest;
  }*/

  public string GetAbsoluteGamePath(string relativePath) {
    return Path.Combine(this.gameInstallDirectory, relativePath);
  }

  public string GetRelativeGamePath(string absolutePath) {
    return Path.GetRelativePath(this.gameInstallDirectory, absolutePath);
  }

  public string GetAbsoluteScratchPath(string relativePath) {
    return Path.Combine(this.scratchDirectory, relativePath);
  }

  public string GetRelativeScratchPath(string absolutePath) {
    return Path.GetRelativePath(this.scratchDirectory, absolutePath);
  }

  public bool UnpackEra(string relativeEraPath) {
    if (this.IsEraUnpacked(relativeEraPath)) {
      return false;
    }

    Console.WriteLine($"Unpacking {relativeEraPath}");

    var absoluteEraPath = this.GetAbsoluteGamePath(relativeEraPath);
    var expander =
        new EraFileExpander(absoluteEraPath);

    expander.options.Set(EraFileUtilOptions.X64);
    expander.options.Set(EraFileUtilOptions
                             .SKIP_VERIFICATION);

    expander.expanderOptions.Set(EraFileExpanderOptions
                                     .DECRYPT);
    expander.expanderOptions.Set(EraFileExpanderOptions
                                     .DONT_OVERWRITE_EXISTING_FILES);
    expander.expanderOptions.Set(EraFileExpanderOptions
                                     .DECOMPRESS_UI_FILES);
    expander.expanderOptions.Set(EraFileExpanderOptions
                                     .TRANSLATE_GFX_FILES);

    if (!expander.Read()) {
      return false;
    }

    if (!expander.ExpandTo(this.scratchDirectory,
                           Path.GetFileNameWithoutExtension(
                               absoluteEraPath))) {
      return false;
    }

    return true;
  }

  public bool IsEraUnpacked(string relativeEraPath) {
    return File.Exists(Path.Combine(this.scratchDirectory,
                                    Path.ChangeExtension(
                                        Path.GetFileName(relativeEraPath),
                                        ".eradef")));
  }

  public void ExpandAllEraFiles() {
    var files = Directory.GetFiles(this.gameInstallDirectory, "*.era");
    foreach (var eraFile in files) {
      this.UnpackEra(this.GetRelativeGamePath(eraFile));
    }
  }
}