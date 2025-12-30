using System.Numerics;
using System.Text.RegularExpressions;

using fin.data.lazy;
using fin.image;
using fin.io;
using fin.math;
using fin.math.splines;
using fin.model;
using fin.model.impl;
using fin.model.io.importers.assimp;
using fin.model.util;
using fin.scene;
using fin.util.asserts;
using fin.util.enumerables;
using fin.util.linq;
using fin.util.sets;

using gm.api;

using Newtonsoft.Json;


namespace vhr.api;

public sealed class VictoryHeatRallyTrackSceneFileBundle : ISceneFileBundle {
  public IReadOnlyTreeFile TrackJsonFile { get; set; }
  public IReadOnlyTreeDirectory ExtractedDirectory { get; set; }
  public IReadOnlyTreeDirectory DataDirectory { get; set; }
  public IReadOnlyTreeFile? MainFile => TrackJsonFile;
}

public sealed partial class VictoryHeatRallyTrackSceneImporter
    : ISceneImporter<VictoryHeatRallyTrackSceneFileBundle> {
  public IScene Import(VictoryHeatRallyTrackSceneFileBundle fileBundle) {
    var trackJsonFile = fileBundle.TrackJsonFile;

    var fileSet = fileBundle.MainFile.AsFileSet();
    var finScene = new SceneImpl { FileBundle = fileBundle, Files = fileSet };

    var finArea = finScene.AddArea();
    finScene.CreateDefaultLighting(finArea.AddRootNode());

    var dataDirectory = fileBundle.DataDirectory;
    var spriteDirectory =
        fileBundle.ExtractedDirectory.AssertGetExistingSubdir("dataWin\\sprt");

    var lazySpriteImages
        = new LazyCaseInvariantStringDictionary<IImage>(spriteName => {
          if (!spriteDirectory.TryToGetExistingFile(
                  $"{spriteName}.png",
                  out var spriteFile)) {
            spriteFile =
                spriteDirectory.AssertGetExistingFile($"{spriteName}_0.png");
          }

          return FinImage.FromFile(spriteFile);
        });

    var modelDirectory = dataDirectory.AssertGetExistingSubdir("MODEL");
    var lazyTrackItemModels
        = new LazyDictionary<(int, string[]), IModel?>(tuple => {
          var (modelIndex, spriteNames) = tuple;
          var modelPath = modelIndex switch {
              15 => "Vehicles/Red_Bus.obj",

              22 => "Jungle/Jungle_Stone Platform Arch.obj",
              23 => "Jungle/Jungle_Stone Platforms Leaning.obj",

              44 => "VHN/VHN_Billboard_Tower.obj",
              45 => "VHN/VHN_Main_Stage.obj",
              46 => "VHN/VHN_Stadium_Quad.obj",
              47 => "VHN/VHN_Stadium_Seats_Double.obj",

              49 => "Forest/Forest_Tree Wall.obj",

              50 => "County/Danger_County_Barn_Closed.obj",
              51 => "County/Danger_County_Barn_Open.obj",
              52 => "County/Danger_County_Brush_Wall.obj",
              53 => "County/Wooden_Fence.obj",

              58 => "Vehicles/Blue_Bus.obj",
              59 => "Vehicles/Blue_VHN Truck.obj",
              60 => "Vehicles/Boat_Big.obj",
              61 => "Vehicles/Boat_Small.obj",
              63 => "Vehicles/Red_Team Truck.obj",
              64 => "Vehicles/Yellow_Bus.obj",
              65 => "Vehicles/Yellow_Team Truck.obj",

              66 => "Waku Land/Carousel.obj",
              67 => "Waku Land/Kiosk.obj",
              68 => "Waku Land/Waku_Tent.obj",
              69 => "Waku Land/Wooden_Coaster.obj",
              70 => "Waku Land/Drop_Tower.obj",

              72 => "Beach/Tiki_Bar.obj",
              73 => "Beach/Tiki_Hut.obj",

              74 => "Forest/Castle_Bridge.obj",
              75 => "Forest/Castle_L Fortress.obj",
              76 => "Forest/Castle_Ruins.obj",

              _ => null,
          };

          if (modelPath == null) {
            return null;
          }

          var finModel = new AssimpModelImporter().Import(
              new AssimpModelFileBundle {
                  MainFile = modelDirectory.AssertGetExistingFile(modelPath)
              });

          var spriteName = spriteNames[0];
          var spriteImage = lazySpriteImages[spriteName];
          var (finMaterial, finTexture)
              = finModel.MaterialManager.AddSimpleTextureMaterialFromImage(
                  spriteImage,
                  spriteName);
          finMaterial.CullingMode = CullingMode.SHOW_BOTH;
          finTexture.WrapModeU = finTexture.WrapModeV = WrapMode.REPEAT;
          finTexture.MinFilter = TextureMinFilter.NEAR;

          foreach (var primitive in
                   finModel.Skin.Meshes.SelectMany(m => m.Primitives)) {
            primitive.SetMaterial(finMaterial);
          }

          return finModel;
        });

    if (dataDirectory.TryToGetExistingFile(
            Path.Join("TRK\\MODEL",
                      $"{trackJsonFile.NameWithoutExtension}.vbuff"),
            out var vbFile)) {
      fileSet.Add(vbFile);

      var trackModel =
          new VbModelImporter().Import(new VbModelFileBundle(vbFile));

      var (textureMaterial, texture) =
          trackModel.MaterialManager.AddSimpleTextureMaterialFromFile(
              spriteDirectory.AssertGetExistingFile("spr_roadtex_0.png"));
      textureMaterial.CullingMode = CullingMode.SHOW_BOTH;
      texture.MinFilter = TextureMinFilter.NEAR;
      texture.MagFilter = TextureMagFilter.NEAR;

      trackModel.Skin.Meshes[0].Primitives[0].SetMaterial(textureMaterial);

      var trackObject = finArea.AddRootNode();
      trackObject.AddSceneModel(trackModel);
    }

    var rawJsonLines = trackJsonFile.ReadAllLines();
    var validJson = $"[{string.Join(',', rawJsonLines)}]";

    var trackItems = JsonConvert.DeserializeObject<List<TrackItem>>(validJson,
      new JsonSerializerSettings {
          Converters = [new SingleOrArrayConverter<string>()]
      })!;

    // Gets nodes for track
    var trackNodes = trackItems
                     .Where(i => i is { type: "Node", my_array: not null })
                     .Select(i => new TrackNode(i.my_array!))
                     .ToArray();
    var nodePositions = trackNodes.Select(n => n.Translation).ToArray();
    {
      var nodesModel = new ModelImpl
          { FileBundle = fileBundle, Files = fileSet };
      var nodesMaterial = nodesModel.MaterialManager.AddNullMaterial();
      nodesMaterial.DepthMode = DepthMode.NONE;
      nodesMaterial.DepthCompareType = DepthCompareType.Always;

      var nodesSkin = nodesModel.Skin;
      var nodeVertices =
          nodePositions.Select(n => nodesSkin.AddVertex(n)).ToArray();

      var nodesMesh = nodesSkin.AddMesh();
      nodesMesh.AddLineStrip(nodeVertices);

      var nodesObject = finArea.AddRootNode();
      nodesObject.AddSceneModel(nodesModel);
    }

    var trackPath = new TrackPath(trackNodes);
    var visibleItems =
        trackItems.Where(i => i.type is "Model" or "Object" or "Sprite");
    foreach (var trackItem in visibleItems) {
      var myStruct = trackItem.my_struct;

      var position =
          myStruct != null
              ? new Vector3(myStruct.x ?? 0,
                            -myStruct.z ?? 0,
                            myStruct.y ?? 0)
              : Vector3.Zero;

      var spriteScale = .5f;
      var xScale = (myStruct?.image_xscale ?? 1) *
                   (myStruct?.xscale ?? 1) *
                   spriteScale;
      var yScale = (myStruct?.image_yscale ?? 1) *
                   (myStruct?.yscale ?? 1) *
                   spriteScale;

      var trackItemObj = finArea.AddRootNode();
      trackItemObj.SetPosition(position);
      trackItemObj.SetScale(myStruct?.scale ?? 1);
      trackItemObj.SetRotationDegrees(0, myStruct.rotation ?? 0, 0);

      ISceneNode? followItemObj = null;
      if (myStruct.follow.IsTruthy()) {
        followItemObj = finArea.AddRootNode();
        followItemObj.SetPosition(trackPath.GetTranslationAtOffset(
                                      (myStruct.position ?? 0) * 33f,
                                      -myStruct.xoff_percent ?? 0));
        followItemObj.SetScale(myStruct?.scale ?? 1);
        followItemObj.SetRotationDegrees(0, myStruct.rotation ?? 0, 0);
      }

      switch (trackItem.type) {
        case "Model": {
          var finModels
              = GenerateModelForTrackItemModel_(trackItem.my_struct,
                                                lazyTrackItemModels);

          foreach (var finModel in finModels) {
            trackItemObj.AddSceneModel(finModel);
          }

          break;
        }
        case "Object": {
          break;
        }
        case "Sprite": {
          var spriteModel = new ModelImpl
              { FileBundle = fileBundle, Files = fileSet };

          var spriteIndex = trackItem.my_struct.sprite_index.AssertNonnull();
          var spriteImage = lazySpriteImages[spriteIndex];

          var (spriteMaterial, _) = spriteModel.MaterialManager
                                               .AddSimpleTextureMaterialFromImage(
                                                   spriteImage);
          spriteMaterial.CullingMode = CullingMode.SHOW_FRONT_ONLY;

          var spriteSkin = spriteModel.Skin;
          var spriteMesh = spriteSkin.AddMesh();
          spriteMesh.AddSimpleYawOnlyBillboard(
              spriteModel.Skeleton.Root,
              spriteSkin,
              spriteImage.Width * xScale,
              spriteImage.Height * yScale,
              spriteMaterial,
              true,
              (myStruct.flip_x ?? 1) == -1);

          trackItemObj.AddSceneModel(spriteModel);
          followItemObj?.AddSceneModel(spriteModel);

          break;
        }
      }
    }

    // Adds floor
    if (trackItems.Where(i => i.type == "Other")
                  .TryGetSingle(out var otherTrackItem)) {
      // TODO: Match bounds of scene

      var floorModel = new ModelImpl
          { FileBundle = fileBundle, Files = fileSet };

      var floorImage
          = lazySpriteImages[otherTrackItem.floortex.AssertNonnull()];

      var (floorMaterial, floorTexture) = floorModel.MaterialManager
                                                    .AddSimpleTextureMaterialFromImage(
                                                        floorImage);
      floorMaterial.CullingMode = CullingMode.SHOW_FRONT_ONLY;
      floorTexture.WrapModeU = floorTexture.WrapModeV = WrapMode.REPEAT;

      var textureSize = 250f;
      var textureRepeat = 250;
      var floorSize = textureSize * textureRepeat;

      var floorSkin = floorModel.Skin;
      var floorMesh = floorSkin.AddMesh();


      // TODO: Either off by one tile or flipped
      var ul = (new Vector3(-floorSize / 2, 0, -floorSize / 2), new Vector2(0, 0));
      var ur = (new Vector3(floorSize / 2, 0, -floorSize / 2), new Vector2(textureRepeat, 0));
      var ll = (new Vector3(-floorSize / 2, 0, floorSize / 2), new Vector2(0, textureRepeat));
      var lr = (new Vector3(floorSize / 2, 0, floorSize / 2), new Vector2(textureRepeat, textureRepeat));

      floorMesh.AddSimpleQuad(floorSkin, ul, ur, lr, ll, floorMaterial);

      finArea.AddRootNode().AddSceneModel(floorModel);
    }

    return finScene;
  }

  [GeneratedRegex("@ref sprite\\(([a-zA-Z0-9_]+)\\)")]
  private static partial Regex REF_SPRITE_REGEX();

  private static IEnumerable<IModel> GenerateModelForTrackItemModel_(
      TrackItemStruct? trackItem,
      ILazyDictionary<(int, string[]), IModel?> lazyTrackItemModels) {
    var vhrModel = trackItem?.model;
    if (vhrModel == null) {
      yield break;
    }

    var refSpriteRegex = REF_SPRITE_REGEX();
    var spriteNames
        = vhrModel
          .sprite!
          .Select(sprite => refSpriteRegex.Match(sprite).Groups[1].Value)
          .ToArray();

    if (trackItem.model_index is { } modelIndex &&
        lazyTrackItemModels[(modelIndex, spriteNames)] is { } lazyModel) {
      yield return lazyModel;
      yield break;
    }

    if (vhrModel.cmesh?.triangles == null) {
      yield break;
    }

    var cmesh = vhrModel.cmesh;

    var finModel = ModelImpl.CreateForViewer();

    var vhrVertices = cmesh.triangles.SelectMany(t => t.SeparateTriplets());

    var finSkin = finModel.Skin;
    var finVertices = vhrVertices.Select(position => {
                                   var finVertex = finSkin.AddVertex(position.X,
                                     -position.Z,
                                     position.Y);

                                   return (IReadOnlyVertex) finVertex;
                                 })
                                 .ToArray();

    finSkin.AddMesh().AddTriangles(finVertices);

    yield return finModel;
  }

  private class TrackItem {
    public float[]? my_array;
    public TrackItemStruct? my_struct;
    public string? type;

    // Background
    public string[] bgindex;
    public int[]? xoff;
    public int[]? xparallax;
    public int[]? yoff;
    public int[]? yparallax;

    // Other
    public string? bgm;
    public string? floortex;
    public int? fog_enabled;
    public int? laps;
    public int? rally;
    public int? startpos;
    public string? spr_barrier;
    public int? timeofday;
    public string? track_name;
  }

  private class TrackItemStruct {
    public int? alt_texture;
    public int? flip_x;
    public int? follow;
    public int? image_index;
    public float? image_xscale;
    public float? image_yscale;
    public TrackItemModel? model;
    public int? model_index;
    public string? @object;
    public int? position;
    public float? rotation;
    public float? scale;
    public string? sprite_index;
    public string? type;
    public float? x;
    public float? xoffset;
    public float? xoff_percent;
    public float? xscale;
    public float? y;
    public float? yscale;
    public float? z;
  }

  private class TrackItemModel {
    public bool? array;
    public TrackItemCmesh? cmesh;
    public string? model;

    [JsonConverter(typeof(SingleOrArrayConverter<string>))]
    public List<string>? sprite;

    public int? subdiv;
    public int? tilt;

    [JsonConverter(typeof(SingleOrArrayConverter<string>))]
    public List<string>? texture;
  }

  private class TrackItemCmesh {
    public int? group;
    public int? matrix;
    public string? name;
    public string? shapeList;
    public bool? solid;
    public int? submeshes;
    public int? triangle;
    public float[][]? triangles;
    public int? type;
  }
}