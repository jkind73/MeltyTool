using System;
using System.Collections.Generic;
using System.IO;

using hw.schema.xtt;

namespace HaloWarsTools;
// TODO put value cache in HWContext

public enum HwResourceType {
  NONE,
  XTT, // Terrain Mesh/Albedo
  XTD, // Terrain Opacity/AO
  SCN, // Scenario
  SC2, // Scenario
  SC3, // Scenario
  GLS, // Scenario Lighting
  UGX, // Mesh
  VIS  // Visual Representation
}

public sealed class HwResourceTypeDefinition(
    HwResourceType type,
    Type resourceClass) {
  public HwResourceType type = type;
  public Type @class = resourceClass;
}

public abstract class HwResource {
  private static LazyValueCache staticValuesCache_ = new LazyValueCache();

  private static Dictionary<string, HwResourceTypeDefinition>
      typeDefinitions_ =
          new Dictionary<string, HwResourceTypeDefinition>() {
              {
                  ".xtt",
                  new HwResourceTypeDefinition(
                      HwResourceType.XTT, typeof(Xtt))
              }, {
                  ".xtd",
                  new HwResourceTypeDefinition(
                      HwResourceType.XTD, typeof(HwXtdResource))
              },
              /*{".scn", new HWResourceTypeDefinition(HWResourceType.Scn, typeof(HWScnResource))},
              {".sc2", new HWResourceTypeDefinition(HWResourceType.Sc2, typeof(HWSc2Resource))},
              {".sc3", new HWResourceTypeDefinition(HWResourceType.Sc3, typeof(HWSc3Resource))},
              {".gls", new HWResourceTypeDefinition(HWResourceType.Gls, typeof(HWGlsResource))},*/
              {".ugx", new HwResourceTypeDefinition(HwResourceType.UGX, typeof(HwUgxResource))},
              {".vis", new HwResourceTypeDefinition(HwResourceType.VIS, typeof(HwVisResource))},
          };

  protected static Dictionary<HwResourceType, string> TypeExtensions =>
      staticValuesCache_.Get(() => {
        var dictionary = new Dictionary<HwResourceType, string>();
        foreach (var kvp in typeDefinitions_) {
          dictionary.Add(kvp.Value.type, kvp.Key);
        }
        return dictionary;
      });

  protected abstract void Load(byte[] bytes);

  public HwContext Context { get; protected set; }
  public string RelativePath { get; protected set; }
  public string AbsolutePath => this.Context.GetAbsoluteScratchPath(this.RelativePath);
  public HwResourceType type;

  protected static HwResource GetOrCreateFromFile(HwContext? context,
                                                  string filename,
                                                  HwResourceType
                                                      expectedType =
                                                      HwResourceType.NONE) {
    // Set the extension based on the resource type if the filename doesn't have one
    if (string.IsNullOrEmpty(Path.GetExtension(filename)) &&
        TypeExtensions.TryGetValue(expectedType,
                                   out string defaultExtension)) {
      filename = Path.ChangeExtension(filename, defaultExtension);
    }

    var resource = CreateResource(context, filename);
    resource?.Load(File.ReadAllBytes(filename));

    return resource;
  }

  protected static HwResource
      CreateResource(HwContext? context, string filename) {
    string extension = Path.GetExtension(filename).ToLowerInvariant();

    if (typeDefinitions_.TryGetValue(extension,
                                    out HwResourceTypeDefinition
                                        definition)) {
      if (Activator.CreateInstance(definition.@class) is HwResource resource) {
        resource.type = definition.type;
        resource.Context = context;
        resource.RelativePath = filename;
        return resource;
      }
    }

    return null;
  }
}