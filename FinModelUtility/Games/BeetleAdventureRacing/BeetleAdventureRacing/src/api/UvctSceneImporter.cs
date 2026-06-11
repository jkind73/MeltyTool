using System.Numerics;

using bar.schema;

using fin.io;
using fin.scene;
using fin.util.sets;

using schema.binary;

namespace bar.api;

public sealed record UvctSceneFileBundle(
    IReadOnlyTreeFile MainFile,
    IReadOnlyTreeDirectory RootDirectory)
    : ISceneFileBundle;

public sealed class UvctSceneFileImporter
    : ISceneImporter<UvctSceneFileBundle> {
  public IScene Import(UvctSceneFileBundle fileBundle) {
    var fileChunks
        = fileBundle.MainFile.ReadNew<FileChunks>(Endianness.BigEndian);
    if (fileChunks.Chunks.Count == 0) {
      return new SceneImpl {
          FileBundle = fileBundle,
          Files = new HashSet<IReadOnlyGenericFile>(),
      };
    }

    var files = fileBundle.MainFile.AsFileSet();
    var finScene = new SceneImpl {
        FileBundle = fileBundle,
        Files = files,
    };
    var finArea = finScene.AddArea();

    var uvct = new SchemaBinaryReader(fileChunks.Chunks[0].Buffer,
                                      Endianness.BigEndian)
        .ReadNew<Uvct>();

    var rootDirectory = fileBundle.RootDirectory;

    var rootNode = finArea.AddRootNode();
    rootNode.SetMatrix(
        Matrix4x4.CreateFromQuaternion(
            Quaternion.CreateFromYawPitchRoll(0, -MathF.PI / 2, 0)));

    var finLevelModel = UvmdModelFileImporter.FromMaterialMeshes(
        fileBundle,
        rootDirectory,
        uvct.MaterialMeshes.Select(m => m.Impl),
        false);
    files.Add(finLevelModel.Files);
    rootNode.AddChildNode().AddSceneModel(finLevelModel);

    foreach (var uvctModel in uvct.Models) {
      var finModel = UvmdModelFileImporter.Import(
          new UvmdModelFileBundle(
              rootDirectory.AssertGetExistingFile(
                  $"{uvctModel.ModelIndex}.UVMD"),
              rootDirectory),
          false);
      files.Add(finModel.Files);
      rootNode.AddChildNode()
              .SetMatrix(uvctModel.RdpMatrices[0].ToMatrix4x4())
              .AddSceneModel(finModel);
    }

    return finScene;
  }
}