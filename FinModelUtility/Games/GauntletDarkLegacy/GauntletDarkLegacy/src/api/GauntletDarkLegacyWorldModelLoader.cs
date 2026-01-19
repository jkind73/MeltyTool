using System.Numerics;

using fin.data.queues;
using fin.image;
using fin.io;
using fin.io.bundles;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;

using gdl.schema.anim;
using gdl.schema.objects;
using gdl.schema.worlds;

using schema.binary;


namespace gdl.api;

public sealed class GauntletDarkLegacyWorldModelFileBundle : IModelFileBundle {
  public required IReadOnlyTreeFile WorldsFile { get; init; }
  public required IReadOnlyTreeFile ObjectsFile { get; init; }
  public required IReadOnlyTreeFile AnimFile { get; init; }
  public required IReadOnlyTreeFile TexturesFile { get; init; }

  public IReadOnlyTreeFile MainFile => this.ObjectsFile;
}

public sealed class GauntletDarkLegacyWorldModelImporter
    : IModelImporter<GauntletDarkLegacyWorldModelFileBundle> {
  public IModel Import(GauntletDarkLegacyWorldModelFileBundle fileBundle) {
    var objects = fileBundle.ObjectsFile.ReadNew<Objects>();
    var anim = fileBundle.AnimFile.ReadNew<Anim>();

    return ImportImpl(
        fileBundle,
        new HashSet<IReadOnlyGenericFile>([
            fileBundle.WorldsFile,
            fileBundle.ObjectsFile,
            fileBundle.AnimFile,
            fileBundle.TexturesFile,
        ]),
        fileBundle.WorldsFile.ReadNew<Worlds>(),
        objects,
        anim,
        fileBundle.TexturesFile,
        new Dictionary<int, IReadOnlyImage>());
  }

  public static IModel ImportImpl(
      IFileBundle fileBundle,
      IReadOnlySet<IReadOnlyGenericFile> files,
      Worlds worlds,
      Objects objects,
      Anim anim,
      IReadOnlyGenericFile texturesFile,
      Dictionary<int, IReadOnlyImage> textureImageCache) {
    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = files,
    };

    using var textureBr = texturesFile.OpenReadAsBinary(Endianness.BigEndian);
    var lazyFinMaterials =
        new LazyGdlMaterials(finModel, textureImageCache, textureBr, objects);

    var objectByName = objects.ObjectDefinitions.Zip(objects.RootObjects)
                              .ToDictionary(k => k.First.Name);

    var worldObjectQueue
        = new FinTuple2Queue<short, Vector3>((0, Vector3.Zero));
    while (worldObjectQueue.TryDequeue(out var worldObjectIndex,
                                       out var parentTranslation)) {
      var gdlWorldObject = worlds.WorldObjects[worldObjectIndex];

      var position = gdlWorldObject.Position with {
          X = -gdlWorldObject.Position.X
      };

      var translation = parentTranslation + position;

      if (gdlWorldObject.ChildIndex != -1) {
        worldObjectQueue.Enqueue((gdlWorldObject.ChildIndex, translation));
      }
      if (gdlWorldObject.NextIndex != -1) {
        worldObjectQueue.Enqueue((gdlWorldObject.NextIndex, parentTranslation));
      }

      if (!objectByName.TryGetValue(gdlWorldObject.Name, out var tuple)) {
        continue;
      }

      var (definition, obj) = tuple;
      var mbFlags = gdlWorldObject.MbFlags;

      GauntletDarkLegacyModelImporter.SetFaceTowardsCamera(
          finModel.Skeleton.Root,
          mbFlags);

      MeshUtil.AddObjectMesh(finModel,
                             null,
                             Matrix4x4.CreateTranslation(translation),
                             definition,
                             obj,
                             lazyFinMaterials,
                             mbFlags,
                             out _);
    }

    return finModel;
  }
}