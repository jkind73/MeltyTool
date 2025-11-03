using System;
using System.IO;
using System.Threading.Tasks;

using fin.io;
using fin.model.io;
using fin.model.io.exporters;
using fin.model.io.exporters.gltf;

using marioartist.api;

namespace MarioArtistTool.services;

public static class ExportService {
  public static void ExportBundles(IModelFileBundle[] bundles,
                                   ISystemDirectory outputDirectory) {
    // TODO: Prompt about files before overwriting
    foreach (var bundle in bundles) {
      try {
        var model = bundle switch {
            Ma3d1ModelFileBundle ma3dBundle
                => new Ma3d1ModelLoader().Import(ma3dBundle),
            TstltModelFileBundle tstltBundle
                => new TstltModelLoader().Import(tstltBundle),
            _ => null,
        };

        if (model == null) {
          continue;
        }

        var modelName = bundle.DisplayName;

        var dstDirectory = outputDirectory.GetOrCreateSubdir(modelName);
        var dstFile
            = new FinFile(Path.Join(dstDirectory.FullPath, $"{modelName}.glb"));

        var gltfModelExporter = new GltfModelExporter();
        gltfModelExporter.ExportModel(new ModelExporterParams {
            Model = model,
            OutputFile = dstFile,
        });
      } catch (Exception e) {
        Console.Write(e);
      }
    }
  }
}