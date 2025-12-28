using System.Drawing;

using fin.data.queues;
using fin.image;
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
    var finScene = new SceneImpl {
        FileBundle = fileBundle,
        Files = new HashSet<IReadOnlyGenericFile>([
            fileBundle.ObjectsFile,
            fileBundle.AnimFile,
            fileBundle.TexturesFile,
        ])
    };

    var objects = fileBundle.ObjectsFile.ReadNew<Objects>();
    var anim = fileBundle.AnimFile.ReadNew<Anim>();
    var worlds = fileBundle.WorldsFile.ReadNew<Worlds>();
    var textureImageCache = new Dictionary<int, IReadOnlyImage>();

    var finArea = finScene.AddArea();
    finArea.CreateCustomSkyboxNode();
    finArea.BackgroundColor = Color.Black;

    var worldObjectQueue
        = new FinTuple2Queue<short, ISceneNode?>((0, null));
    while (worldObjectQueue.TryDequeue(out var worldObjectIndex,
                                       out var parentFinSceneNode)) {
      var gdlWorldObject = worlds.WorldObjects[worldObjectIndex];

      var finSceneNode
          = parentFinSceneNode?.AddChildNode() ?? finArea.AddRootNode();
      finSceneNode.SetPosition(-gdlWorldObject.Position.X,
                               gdlWorldObject.Position.Y,
                               gdlWorldObject.Position.Z);

      var finModel = GauntletDarkLegacyModelImporter.ImportImpl(
          finScene.FileBundle,
          finScene.Files,
          objects,
          anim,
          fileBundle.TexturesFile,
          textureImageCache,
          gdlWorldObject.Name,
          gdlWorldObject.MbFlags);

      finSceneNode.AddSceneModel(finModel);

      if (gdlWorldObject.ChildIndex != -1) {
        worldObjectQueue.Enqueue((gdlWorldObject.ChildIndex, finSceneNode));
      }
      if (gdlWorldObject.NextIndex != -1) {
        worldObjectQueue.Enqueue((gdlWorldObject.NextIndex, parentFinSceneNode));
      }
    }

    return finScene;
  }
}