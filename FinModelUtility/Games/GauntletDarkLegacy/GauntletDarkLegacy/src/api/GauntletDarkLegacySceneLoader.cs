using fin.io;
using fin.scene;

using gdl.schema.anim;
using gdl.schema.objects;
using gdl.schema.worlds;

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
    var objects = fileBundle.ObjectsFile.ReadNew<Objects>();
    var anim = fileBundle.AnimFile.ReadNew<Anim>();
    var worlds = fileBundle.WorldsFile.ReadNew<Worlds>();

    var finScene = new SceneImpl {
        FileBundle = fileBundle,
        Files = new HashSet<IReadOnlyGenericFile>([
            fileBundle.ObjectsFile,
            fileBundle.AnimFile,
            fileBundle.TexturesFile,
        ])
    };

    return finScene;
  }
}