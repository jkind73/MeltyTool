using System.Drawing;
using System.Numerics;

using fin.data.lazy;
using fin.data.sets;
using fin.image;
using fin.io;
using fin.io.bundles;
using fin.math.floats;
using fin.math.rotations;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.enumerables;
using fin.util.sets;

using schema.binary;

namespace facade.api;

public sealed record FacadeRoomModelFileBundle(
    IReadOnlyTreeFile MainFile,
    IReadOnlyTreeDirectory PrereqsDirectory)
    : IModelFileBundle;

/// <summary>
///   The models are unfortunately hardcoded in the game's logic. They're
///   defined within the function starting at 0x100925a0 in animEngineDLL.dll.
///   These calls have been replicated here.
/// </summary>
public sealed class FacadeRoomModelImporter
    : IModelImporter<FacadeRoomModelFileBundle> {
  public IModel Import(FacadeRoomModelFileBundle fileBundle)
    => new FacadeRoomModelBuilder(fileBundle).Model;
}

public enum FilmstripId {
  PLANT = 0x0,
  ASHTRAY = 0x1,
  MARTINI_GLASS = 0x2,
  RED_WINE_BOTTLE = 0x3,
  WHITE_WINE_BOTTLE = 0x4,
  PAINTING_1 = 0x10,
  LITTLE_PAINTING = 0x11,
  WEDDING_PHOTO = 0x12,
  TUSCANY = 0x13,
  GOWN_SKETCH_1 = 0x14,
  POST_ITS = 0x15,
  GOWN_SKETCH_4 = 0x16,
  PHONE = 0x17,
  ANSWERING_MACHINE = 0x18,
  EIGHT_BALL = 0x19,
  BRASS_BULL = 0x1A,
  TRINKET_1 = 0x1B,
  TRINKET_2 = 0x1C,
  TRINKET_3 = 0x1D,
  TRINKET_4 = 0x1E,
  TRINKET_5 = 0x1F,
  TRINKET_6 = 0x20,
  TRINKET_7 = 0x21,
  TRINKET_8 = 0x22,
  RUG = 0x23,
  FRONT_DOOR = 0x24,
  BEDROOM_DOOR = 0x25,
  ELEVATOR = 0x26,
  ELEVATOR_BUTTON = 0x27,
  WHITE_CABINET_DOOR = 0x2C,
  WHITE_CABINET_DOOR_2 = 0x2D,
  NYC = 0x2E,
}

public enum ColorId {
  WINDOW = 0x20,
  COLOR_0x2E = 0x2E,
  COLOR_0x2F = 0x2F,
  COLOR_0x30 = 0x30,
  COLOR_0x31 = 0x31,
  COLOR_0x32 = 0x32,
  COLOR_0x33 = 0x33,
  COLOR_0x34 = 0x34,
  COLOR_0x35 = 0x35,
  COLOR_0x36 = 0x36,
  COLOR_0x37 = 0x37,
  COLOR_0x38 = 0x38,
  COLOR_0x39 = 0x39,
  COLOR_0x3A = 0x3A,
  FLOOR = 0x3b,
  WALL = 0xFF,
}

file class FacadeRoomModelBuilder {
  private readonly FacadeRoomModelFileBundle fileBundle_;

  private readonly
      LazyDictionary<(
          FilmstripId? filmstripId,
          ColorId? colorId,
          bool removeBackground),
          IReadOnlyMaterial> lazyMaterials_;

  public ModelImpl Model { get; }

  public FacadeRoomModelBuilder(FacadeRoomModelFileBundle fileBundle) {
    this.fileBundle_ = fileBundle;

    var files = fileBundle.MainFile.AsFileSet();
    this.Model = new ModelImpl {
        FileBundle = fileBundle,
        Files = files,
    };

    this.lazyMaterials_
        = new LazyDictionary<(FilmstripId? filmstripId, ColorId? colorId, bool
            removeBackground),
            IReadOnlyMaterial>(tuple => {
          var (filmstripId, colorId, removeBackground) = tuple;

          var textureFileName = filmstripId switch {
              FilmstripId.PLANT                => "plant.bmp",
              FilmstripId.ASHTRAY              => "ashtray.bmp",
              FilmstripId.MARTINI_GLASS        => "martini_empty_hold.bmp",
              FilmstripId.RED_WINE_BOTTLE      => "redwinebottle.bmp",
              FilmstripId.WHITE_WINE_BOTTLE    => "whitewinebottle.bmp",
              FilmstripId.PAINTING_1           => "painting1.bmp",
              FilmstripId.LITTLE_PAINTING      => "littlepainting.bmp",
              FilmstripId.WEDDING_PHOTO        => "weddingpic.bmp",
              FilmstripId.TUSCANY              => "tuscany.bmp",
              FilmstripId.GOWN_SKETCH_1        => "gownsketch1.bmp",
              FilmstripId.POST_ITS             => "post_its.bmp",
              FilmstripId.GOWN_SKETCH_4        => "gownsketch4.bmp",
              FilmstripId.PHONE                => "phoneontable.bmp",
              FilmstripId.ANSWERING_MACHINE    => "answeringmachine.bmp",
              FilmstripId.EIGHT_BALL           => "eightball.bmp",
              FilmstripId.BRASS_BULL           => "brassbull.bmp",
              FilmstripId.TRINKET_1            => "trinket1.bmp",
              FilmstripId.TRINKET_2            => "trinket2.bmp",
              FilmstripId.TRINKET_3            => "trinket3.bmp",
              FilmstripId.TRINKET_4            => "trinket4.bmp",
              FilmstripId.TRINKET_5            => "trinket5.bmp",
              FilmstripId.TRINKET_6            => "trinket6.bmp",
              FilmstripId.TRINKET_7            => "trinket7.bmp",
              FilmstripId.TRINKET_8            => "trinket8.bmp",
              FilmstripId.RUG                  => "rug.bmp",
              FilmstripId.FRONT_DOOR           => "frontdoor.bmp",
              FilmstripId.BEDROOM_DOOR         => "bedroomdoor.bmp",
              FilmstripId.ELEVATOR             => "elevator.bmp",
              FilmstripId.ELEVATOR_BUTTON      => "elevatorbutton.bmp",
              FilmstripId.WHITE_CABINET_DOOR   => "whitecabinetdoor.bmp",
              FilmstripId.WHITE_CABINET_DOOR_2 => "whitecabinetdoor2.bmp",
              FilmstripId.NYC                  => "nyc.bmp",
              _                                => null,
          };

          Color? color = colorId switch {
              ColorId.WINDOW => Color.FromArgb((byte) (255 * .2f),
                                               (byte) (255 * .2f),
                                               (byte) (255 * .2f),
                                               (byte) (255 * .4f)),
              ColorId.COLOR_0x2E => Color.FromArgb((byte) (255 * .63f),
                                                   (byte) (255 * .46f),
                                                   (byte) (255 * .32f)),
              ColorId.COLOR_0x2F => Color.FromArgb((byte) (255 * .23f),
                                                   (byte) (255 * .16f),
                                                   (byte) (255 * .02f)),
              ColorId.COLOR_0x30 => Color.FromArgb((byte) (255 * .33f),
                                                   (byte) (255 * .26f),
                                                   (byte) (255 * .12f)),
              ColorId.COLOR_0x31 => Color.FromArgb((byte) (255 * .43f),
                                                   (byte) (255 * .36f),
                                                   (byte) (255 * .22f)),
              ColorId.COLOR_0x32 => Color.FromArgb((byte) (255 * .48f),
                                                   (byte) (255 * .41f),
                                                   (byte) (255 * .27f)),

              // Dark grays
              ColorId.COLOR_0x33 => Color.FromArgb((byte) (255 * .05f),
                                                   (byte) (255 * .05f),
                                                   (byte) (255 * .05f)),
              ColorId.COLOR_0x34 => Color.FromArgb((byte) (255 * .1f),
                                                   (byte) (255 * .1f),
                                                   (byte) (255 * .1f)),
              ColorId.COLOR_0x35 => Color.FromArgb((byte) (255 * .15f),
                                                   (byte) (255 * .15f),
                                                   (byte) (255 * .15f)),
              ColorId.COLOR_0x36 => Color.FromArgb((byte) (255 * .2f),
                                                   (byte) (255 * .2f),
                                                   (byte) (255 * .2f)),
              ColorId.COLOR_0x37 => Color.FromArgb((byte) (255 * .25f),
                                                   (byte) (255 * .25f),
                                                   (byte) (255 * .25f)),
              ColorId.COLOR_0x38 => Color.FromArgb((byte) (255 * .3f),
                                                   (byte) (255 * .3f),
                                                   (byte) (255 * .3f)),
              ColorId.COLOR_0x39 => Color.FromArgb((byte) (255 * .35f),
                                                   (byte) (255 * .35f),
                                                   (byte) (255 * .35f)),
              ColorId.COLOR_0x3A => Color.FromArgb((byte) (255 * .4f),
                                                   (byte) (255 * .4f),
                                                   (byte) (255 * .4f)),
              ColorId.FLOOR => Color.FromArgb((byte) (255 * .45f),
                                              (byte) (255 * .45f),
                                              (byte) (255 * .45f)),
              ColorId.WALL => Color.Black,
              _            => null,
          };

          if (textureFileName == null) {
            var colorMaterial
                = this.Model.MaterialManager.AddColorMaterial(
                    color ?? Color.White);
            colorMaterial.CullingMode = CullingMode.SHOW_BOTH;
            return colorMaterial;
          } else {
            var textureFile
                = fileBundle.PrereqsDirectory.AssertGetExistingFile(
                    textureFileName);
            files.Add(textureFile);

            var image = FinImage.FromFile(textureFile);
            if (removeBackground) {
              image = image.RemoveBackgroundColor(Color.White);
            }

            var (textureMaterial, _)
                = this.Model.MaterialManager.AddSimpleTextureMaterialFromImage(
                    image,
                    textureFile.NameWithoutExtension.ToString());
            textureMaterial.CullingMode = CullingMode.SHOW_BOTH;

            if (color != null) {
              textureMaterial.DiffuseColor = color;
            }

            return textureMaterial;
          }
        });

    this.AddScenery_();
    this.AddRoom_();
    this.AddSprites_();
  }

  private void AddScenery_() {
    this.AddSceneryObject_(
        "couch",
        new Vector3(150, 0, -350),
        0x1ac5fc,
        0x1aeeb4,
        []);
    this.AddSceneryObject_(
        "cabinet",
        new Vector3(-177, 0, -350),
        0x1b4c24,
        0x1b70dc,
        [
            (0, 1, 2, 3),
            (0xe, 0xf, 0x10, 0x11),
            (0x12, 0x13, 0x14, 0x15),
            (6, 7, 8, 9),
            (0x1e, 0x1f, 0x20, 0x21),
            (0x22, 0x23, 0x24, 0x25),
            (0x26, 0x27, 0x28, 0x29),
            (0x2a, 0x2b, 0x2c, 0x2d),
        ]);
    this.AddSceneryObject_(
        "side table",
        new Vector3(170, 0, -210),
        0x1af6b4,
        0x1b1b6c,
        [
            (0x21, 0x24, 0x26, 0x22),
        ]);
    this.AddSceneryObject_(
        "work table",
        new Vector3(160, 0, -30),
        0x1b1f6c,
        0x1b4424,
        []);
    this.AddSceneryObject_(
        "bar",
        new Vector3(-160, 0, -120),
        0x1bcfdc,
        0x1bf494,
        [
            (4, 10, 9, 6),
            (2, 4, 10, 0xb),
            (1, 2, 0xb, 0xc),
        ]);
    this.AddSceneryObject_(
        "side table 2",
        new Vector3(-180, 0, -60),
        0x1af6b4,
        0x1b1b6c,
        []);
  }

  private void AddSceneryObject_(
      string name,
      Vector3 objectPosition,
      uint vertexOffset,
      uint faceOffset,
      IEnumerable<(int, int, int, int)> collisionQuadIndices) {
    var animEngineDll = this.fileBundle_.PrereqsDirectory.AssertGetExistingFile(
        "animEngineDLL.dll");
    using var br = animEngineDll.OpenReadAsBinary();

    var bone = this.Model.Skeleton.Root.AddChild(objectPosition);
    bone.Name = name;

    var skin = this.Model.Skin;
    var boneWeights
        = skin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE, bone);

    br.Position = vertexOffset;

    var vertices = new List<IReadOnlyVertex>();
    while (!br.ReadSingle().IsRoughly(666)) {
      br.Position -= sizeof(float);

      var vertex = skin.AddVertex(br.ReadVector3());
      vertex.SetBoneWeights(boneWeights);

      vertices.Add(vertex);
    }

    br.Position = faceOffset;
    var allFaceInts = new List<int>();
    while (br.ReadUInt32() != 666) {
      br.Position -= sizeof(int);
      allFaceInts.AddRange(br.ReadInt32s(60));
    }

    // (The collision is just the subset of vertices from collisionQuadIndices.)

    var mesh = skin.AddMesh();
    mesh.Name = name;

    var faceInts = new List<int>();
    var quads = new List<(IReadOnlyVertex, IReadOnlyVertex, IReadOnlyVertex,
        IReadOnlyVertex)>();

    void TryToAddFace() {
      if (faceInts.Count == 0) {
        return;
      }

      // TODO: What on earth is all the other data here
      // TODO: What on earth does it mean if the count is less than 4?
      if (faceInts.Count >= 4) {
        var v0 = faceInts[0];
        var v1 = faceInts[1];
        var v2 = faceInts[2];
        var v3 = faceInts[3];

        if (v0 < vertices.Count &&
            v1 < vertices.Count &&
            v2 < vertices.Count &&
            v3 < vertices.Count) {
          quads.Add((vertices[v0], vertices[v1], vertices[v2], vertices[v3]));
        }
      }

      faceInts.Clear();
    }

    foreach (var faceInt in allFaceInts) {
      if (faceInt == -1) {
        TryToAddFace();
        continue;
      }

      faceInts.Add(faceInt);
    }

    TryToAddFace();

    mesh.AddQuads(quads);
  }

  private void AddRoom_() {
    this.AddPlane_(
        "LR_backwall1",
        null,
        ColorId.WALL,
        new Vector2(70, 160),
        new Vector3(-165, 80, -500),
        0);
    this.AddPlane_(
        "LR_backwallsliver1",
        null,
        ColorId.WALL,
        new Vector2(6.5f, 160),
        new Vector3(-66, 80, -500),
        0);
    this.AddPlane_(
        "LR_backwallsliver2",
        null,
        ColorId.WALL,
        new Vector2(6.5f, 160),
        new Vector3(6, 80, -500),
        0);
    this.AddPlane_(
        "LR_backwall3",
        null,
        ColorId.WALL,
        new Vector2(30, 160),
        new Vector3(85, 80, -500),
        0);
    this.AddPlane_(
        "LR_backwall4",
        null,
        ColorId.WALL,
        new Vector2(100, 160),
        new Vector3(150, 80, -500),
        0);
    this.AddPlane_(
        "LR_backwalltoppanel",
        null,
        ColorId.WALL,
        new Vector2(200, 10),
        new Vector3(-30, 155, -500),
        0);

    this.AddPlane_(
        "LR_leftwall0",
        null,
        ColorId.WALL,
        new Vector2(70, 160),
        new Vector3(-200, 80, -5),
        90);
    this.AddPlane_(
        "LR_leftwall1",
        null,
        ColorId.WALL,
        new Vector2(100, 160),
        new Vector3(-200, 80, -90),
        90);
    this.AddPlane_(
        "LR_leftwall2",
        null,
        ColorId.WALL,
        new Vector2(100, 160),
        new Vector3(-200, 80, -190),
        90);
    this.AddPlane_(
        "LR_leftwall3",
        null,
        ColorId.WALL,
        new Vector2(100, 160),
        new Vector3(-200, 80, -290),
        90);
    this.AddPlane_(
        "LR_leftwall4",
        null,
        ColorId.WALL,
        new Vector2(60, 160),
        new Vector3(-200, 80, -370),
        90);
    this.AddPlane_(
        "LR_leftwall5",
        null,
        ColorId.WALL,
        new Vector2(100, 160),
        new Vector3(-200, 80, -450),
        90);

    this.AddPlane_(
        "LR_rightwall0",
        null,
        ColorId.WALL,
        new Vector2(100, 160),
        new Vector3(200, 80, 50),
        90);
    this.AddPlane_(
        "LR_rightwall1",
        null,
        ColorId.WALL,
        new Vector2(100, 160),
        new Vector3(200, 80, -50),
        90);
    this.AddPlane_(
        "LR_rightwall2",
        null,
        ColorId.WALL,
        new Vector2(100, 160),
        new Vector3(200, 80, -150),
        90);
    this.AddPlane_(
        "LR_rightwall3",
        null,
        ColorId.WALL,
        new Vector2(100, 160),
        new Vector3(200, 80, -250),
        90);
    this.AddPlane_(
        "LR_rightwall4",
        null,
        ColorId.WALL,
        new Vector2(100, 160),
        new Vector3(200, 80, -350),
        90);
    this.AddPlane_(
        "LR_rightwall5",
        null,
        ColorId.WALL,
        new Vector2(100, 160),
        new Vector3(200, 80, -450),
        90);

    this.AddPlane_(
        "LR_backwindow1",
        null,
        ColorId.WINDOW,
        new Vector2(66, 150),
        new Vector3(-101, 77, -498),
        0);
    this.AddPlane_(
        "LR_backwindow2",
        null,
        ColorId.WINDOW,
        new Vector2(66, 150),
        new Vector3(-30, 77, -498),
        0);
    this.AddPlane_(
        "LR_backwindow3",
        null,
        ColorId.WINDOW,
        new Vector2(66, 150),
        new Vector3(41, 77, -498),
        0);

    this.AddFloor_(
        "floor1",
        null,
        ColorId.FLOOR,
        new Vector2(500, 600),
        new Vector3(-250, 0, 300));
    this.AddFloor_(
        "floor2",
        null,
        ColorId.FLOOR,
        new Vector2(500, 600),
        new Vector3(250, 0, 300));
    this.AddFloor_(
        "floor3",
        null,
        ColorId.FLOOR,
        new Vector2(500, 600),
        new Vector3(-250, 0, -300));
    this.AddFloor_(
        "floor4",
        null,
        ColorId.FLOOR,
        new Vector2(500, 600),
        new Vector3(250, 0, -300));
  }

  private void AddSprites_() {
    this.AddSprite_(
        "plant",
        FilmstripId.PLANT,
        new Vector2(120),
        new Vector3(160, 60, 65));
    this.AddSprite_(
        "ashtray",
        FilmstripId.ASHTRAY,
        new Vector2(10, 5),
        new Vector3(165, 60, 10));
    this.AddSprite_(
        "phone",
        FilmstripId.PHONE,
        new Vector2(16),
        new Vector3(145, 60, -60));

    this.AddSprite_("martini glass 1",
                    FilmstripId.MARTINI_GLASS,
                    new Vector2(12),
                    new Vector3(-184, 41, -274));
    this.AddSprite_("martini glass 2",
                    FilmstripId.MARTINI_GLASS,
                    new Vector2(12),
                    new Vector3(-183, 41, -286));
    this.AddSprite_("martini glass 3",
                    FilmstripId.MARTINI_GLASS,
                    new Vector2(12),
                    new Vector3(-183.5f, 41, -297));

    this.AddPlane_(
        "big abstract painting",
        FilmstripId.PAINTING_1,
        null,
        new Vector2(52, 70),
        new Vector3(190, 100, -380),
        90);
    this.AddPlane_(
        "maoPainting",
        FilmstripId.LITTLE_PAINTING,
        null,
        new Vector2(40, 50),
        new Vector3(215, 100, 400),
        90);
    this.AddPlane_(
        "little painting",
        FilmstripId.LITTLE_PAINTING,
        null,
        new Vector2(20, 32),
        new Vector3(190, 118, -335),
        90);
    // (rug)
    this.AddPlane_(
        "wedding photo",
        FilmstripId.WEDDING_PHOTO,
        null,
        new Vector2(60, 70),
        new Vector3(190, 90, -145),
        90);
    this.AddPlane_(
        "italy picture",
        FilmstripId.TUSCANY,
        null,
        new Vector2(32),
        new Vector3(-199, 90, -60),
        270);

    this.AddPlane_(
        "wedding gown post its",
        FilmstripId.POST_ITS,
        null,
        new Vector2(36),
        new Vector3(190, 50, -28),
        90);
    this.AddPlane_(
        "wedding gown sketch 4",
        FilmstripId.GOWN_SKETCH_4,
        null,
        new Vector2(30),
        new Vector3(190, 83, -35),
        90);

    this.AddSprite_(
        "advice ball",
        FilmstripId.EIGHT_BALL,
        new Vector2(8),
        new Vector3(-149, 71, -170));
    this.AddSprite_(
        "brass bull",
        FilmstripId.BRASS_BULL,
        new Vector2(16),
        new Vector3(-185, 75, -300));
    this.AddSprite_(
        "trinket 1",
        FilmstripId.TRINKET_1,
        new Vector2(28),
        new Vector3(-185, 48, -450));
    this.AddSprite_(
        "trinket 2",
        FilmstripId.TRINKET_2,
        new Vector2(28),
        new Vector3(-186, 48, -425));
    this.AddSprite_(
        "trinket 3",
        FilmstripId.TRINKET_3,
        new Vector2(20),
        new Vector3(-184, 46, -400));
  }

  private void AddSprite_(
      string meshName,
      FilmstripId filmstripId,
      Vector2 widthAndHeight,
      Vector3 position) {
    var bone = this.Model.Skeleton.Root.AddChild(position);
    bone.AlwaysFaceTowardsCamera(FaceTowardsCameraType.YAW_ONLY,
                                 QuaternionUtil.CreateZyxRadians(
                                     -MathF.PI / 2,
                                     0,
                                     0));

    var material = this.lazyMaterials_[(filmstripId, null, true)];

    var (width, height) = (widthAndHeight.X, widthAndHeight.Y);

    var ul = new Vector3(-width / 2, height / 2, 0);
    var ur = new Vector3(width / 2, height / 2, 0);
    var lr = new Vector3(width / 2, -height / 2, 0);
    var ll = new Vector3(-width / 2, -height / 2, 0);

    var skin = this.Model.Skin;
    var mesh = skin.AddMesh();
    mesh.Name = meshName;
    mesh.AddSimpleQuad(skin,
                       (ul, new Vector2(0, 0), null),
                       (ur, new Vector2(1, 0), null),
                       (lr, new Vector2(1, 1), null),
                       (ll, new Vector2(0, 1), null),
                       material,
                       bone);
  }

  private void AddPlane_(
      string meshName,
      FilmstripId? filmstripId,
      ColorId? colorId,
      in Vector2 widthAndHeight,
      Vector3 position,
      float degrees) {
    var material = this.lazyMaterials_[(filmstripId, colorId, false)];

    var (width, height) = (widthAndHeight.X, widthAndHeight.Y);

    var xN = MathF.Cos(degrees / 180 * MathF.PI);
    var yN = -MathF.Sin(degrees / 180 * MathF.PI);

    var rotationX = width / 2 * xN;
    var rotationY = width / 2 * yN;

    var ul = position + new Vector3(-rotationX, height / 2, -rotationY);
    var ur = position + new Vector3(rotationX, height / 2, rotationY);
    var lr = position + new Vector3(rotationX, -height / 2, rotationY);
    var ll = position + new Vector3(-rotationX, -height / 2, -rotationY);

    var skin = this.Model.Skin;
    var mesh = skin.AddMesh();
    mesh.Name = meshName;
    mesh.AddSimpleQuad(skin,
                       (ul, new Vector2(1, 0), null),
                       (ur, new Vector2(0, 0), null),
                       (lr, new Vector2(0, 1), null),
                       (ll, new Vector2(1, 1), null),
                       material);
  }

  private void AddFloor_(
      string meshName,
      FilmstripId? filmstripId,
      ColorId? colorId,
      in Vector2 widthAndHeight,
      Vector3 position) {
    var material = this.lazyMaterials_[(filmstripId, colorId, false)];

    var (width, height) = (widthAndHeight.X, widthAndHeight.Y);

    var ul = position + new Vector3(-width / 2, 0, height / 2);
    var ur = position + new Vector3(width / 2, 0, height / 2);
    var lr = position + new Vector3(width / 2, 0, -height / 2);
    var ll = position + new Vector3(-width / 2, 0, -height / 2);

    var skin = this.Model.Skin;
    var mesh = skin.AddMesh();
    mesh.Name = meshName;
    mesh.AddSimpleQuad(skin,
                       (ul, new Vector2(1, 0), null),
                       (ur, new Vector2(0, 0), null),
                       (lr, new Vector2(0, 1), null),
                       (ll, new Vector2(1, 1), null),
                       material);
  }
}