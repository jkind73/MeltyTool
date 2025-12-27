using fin.data.queues;
using fin.io;
using fin.model;
using fin.model.util;
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
    var finScene = new SceneImpl {
        FileBundle = fileBundle,
        Files = new HashSet<IReadOnlyGenericFile>([
            fileBundle.ObjectsFile,
            fileBundle.AnimFile,
            fileBundle.TexturesFile,
        ])
    };

    var worlds = fileBundle.WorldsFile.ReadNew<Worlds>();

    var boneByIndex = new Dictionary<int, IBone>();
    var worldObjectsAndBoneByName = new Dictionary<string, WorldObject>();

    /*var worldObj = finScene.AddArea().AddRootNode();
    var finModel = GauntletDarkLegacyModelImporter.ImportImpl(
        new GauntletDarkLegacyModelFileBundle {
            ObjectsFile = fileBundle.ObjectsFile,
            AnimFile = fileBundle.AnimFile,
            TexturesFile = fileBundle.TexturesFile,
        },
        // TODO: Change each piece into a separate object instead
        (gdlObject, finSkeleton) => {
          if (worldObjectsByName.TryGetValue(gdlObject.Name, out var worldObject)) {
            return finSkeleton.Root.AddChild(worldObject.Position);
          }

          return finSkeleton.Root;
        });
    worldObj.AddSceneModel(finModel);*/

    return finScene;
  }
}