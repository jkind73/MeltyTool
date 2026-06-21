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
    var files = fileBundle.MainFile.AsFileSet();
    var finScene = new SceneImpl {
        FileBundle = fileBundle,
        Files = files,
    };
    var finArea = finScene.AddArea();

    var rootNode = finArea.AddRootNode();
    rootNode.SetMatrix(BarUtils.ROOT_MATRIX);

    AddToScene(fileBundle, files, rootNode);

    return finScene;
  }

  public static ISceneNode? AddToScene(
      UvctSceneFileBundle fileBundle,
      HashSet<IReadOnlyGenericFile> files,
      ISceneNode rootNode) {
    var fileChunks
        = fileBundle.MainFile.ReadNew<FileChunks>(Endianness.BigEndian);
    if (fileChunks.Chunks.Count == 0) {
      return null;
    }

    var uvct = new SchemaBinaryReader(fileChunks.Chunks[0].Buffer,
                                      Endianness.BigEndian)
        .ReadNew<Uvct>();

    var rootDirectory = fileBundle.RootDirectory;

    var uvctNode = rootNode.AddChildNode();
    uvctNode.Name = $"UVCT #{fileBundle.MainFile.NameWithoutExtension}";

    var finLevelModel = UvmdModelFileImporter.FromMaterialMeshes(
        fileBundle,
        rootDirectory,
        false,
        uvct.MaterialMeshes.Select(m => m.Impl),
        false);
    files.Add(finLevelModel.Files);
    uvctNode.AddChildNode().AddSceneModel(finLevelModel);

    foreach (var uvctModel in uvct.Models) {
      var finModel = UvmdModelFileImporter.Import(
          new UvmdModelFileBundle(
              rootDirectory.AssertGetExistingFile(
                  $"uvmd/{uvctModel.ModelIndex}.uvmd"),
              rootDirectory),
          false);
      files.Add(finModel.Files);
      var uvmdNode = uvctNode.AddChildNode();
      uvmdNode.SetMatrix(
          Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(0, MathF.PI / 2, 0)) * 
          uvctModel.RdpMatrices[0].ToMatrix4x4());
      uvmdNode.AddSceneModel(finModel);
      uvmdNode.Name = $"UVMD #{uvctModel.ModelIndex}";
    }

    return uvctNode;
  }
}