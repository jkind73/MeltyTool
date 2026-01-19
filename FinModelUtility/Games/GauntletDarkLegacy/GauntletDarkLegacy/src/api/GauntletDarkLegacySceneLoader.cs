using System.Drawing;

using fin.io;
using fin.scene;
using fin.ui.rendering.gl.scene;

namespace gdl.api;

public sealed class GauntletDarkLegacySceneFileBundle : ISceneFileBundle {
  public required IReadOnlyTreeFile WorldsFile { get; init; }
  public required IReadOnlyTreeFile ObjectsFile { get; init; }
  public required IReadOnlyTreeFile AnimFile { get; init; }
  public required IReadOnlyTreeFile TexturesFile { get; init; }

  public IReadOnlyTreeFile MainFile => this.WorldsFile;
}

public sealed class GauntletDarkLegacySceneImporter
    : ISceneImporter<GauntletDarkLegacySceneFileBundle> {
  public IScene Import(GauntletDarkLegacySceneFileBundle fileBundle) {
    var finScene = new SceneImpl {
        FileBundle = fileBundle,
        Files = new HashSet<IReadOnlyGenericFile>([
            fileBundle.WorldsFile,
            fileBundle.ObjectsFile,
            fileBundle.AnimFile,
            fileBundle.TexturesFile,
        ])
    };

    var finArea = finScene.AddArea();
    finArea.CreateCustomSkyboxNode();
    finArea.BackgroundColor = Color.Black;

    var mapNode = finArea.AddRootNode();
    mapNode.AddComponent(
        new SimpleModelRenderComponent(
            new GauntletDarkLegacyWorldModelImporter().Import(
                new GauntletDarkLegacyWorldModelFileBundle {
                    WorldsFile = fileBundle.WorldsFile,
                    ObjectsFile = fileBundle.ObjectsFile,
                    AnimFile = fileBundle.AnimFile,
                    TexturesFile = fileBundle.TexturesFile
                })));

    return finScene;
  }
}