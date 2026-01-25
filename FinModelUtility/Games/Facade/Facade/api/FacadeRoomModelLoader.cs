using System.Drawing;
using System.Numerics;

using fin.data.lazy;
using fin.image;
using fin.io;
using fin.math.rotations;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.sets;

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

file class FacadeRoomModelBuilder {
  private readonly
      LazyDictionary<(string textureFileName, bool removeBackground),
          IReadOnlyTextureMaterial> lazyMaterials_;

  public ModelImpl Model { get; }

  public FacadeRoomModelBuilder(FacadeRoomModelFileBundle fileBundle) {
    var files = fileBundle.MainFile.AsFileSet();
    this.Model = new ModelImpl {
        FileBundle = fileBundle,
        Files = files,
    };

    this.lazyMaterials_
        = new LazyDictionary<(string textureFileName, bool removeBackground),
            IReadOnlyTextureMaterial>(tuple => {
          var (textureFileName, removeBackground) = tuple;

          var textureFile
              = fileBundle.PrereqsDirectory.AssertGetExistingFile(
                  textureFileName);
          files.Add(textureFile);

          var image = FinImage.FromFile(textureFile);
          if (removeBackground) {
            image = image.RemoveBackgroundColor(Color.White);
          }

          var (material, _)
              = this.Model.MaterialManager.AddSimpleTextureMaterialFromImage(
                  image,
                  textureFile.NameWithoutExtension.ToString());
          material.CullingMode = CullingMode.SHOW_BOTH;

          return material;
        });

    this.AddRoom_();
    this.AddSprites_();
  }

  private void AddRoom_() {
    this.AddPlane_(
        "LR_backwall1",
        "wall.bmp",
        new Vector2(70, 160),
        new Vector3(-165, 80, -500),
        0);
    this.AddPlane_(
        "LR_backwallsliver1",
        "wall.bmp",
        new Vector2(6.5f, 160),
        new Vector3(-66, 80, -500),
        0);
    this.AddPlane_(
        "LR_backwallsliver2",
        "wall.bmp",
        new Vector2(6.5f, 160),
        new Vector3(6, 80, -500),
        0);
    this.AddPlane_(
        "LR_backwall3",
        "wall.bmp",
        new Vector2(30, 160),
        new Vector3(85, 80, -500),
        0);
    this.AddPlane_(
        "LR_backwall4",
        "wall.bmp",
        new Vector2(100, 160),
        new Vector3(150, 80, -500),
        0);
    this.AddPlane_(
        "LR_backwalltoppanel",
        "wall.bmp",
        new Vector2(200, 10),
        new Vector3(-30, 155, -500),
        0);

    this.AddPlane_(
        "LR_leftwall0",
        "wall.bmp",
        new Vector2(70, 160),
        new Vector3(-200, 80, -5),
        90);
    this.AddPlane_(
        "LR_leftwall1",
        "wall.bmp",
        new Vector2(100, 160),
        new Vector3(-200, 80, -90),
        90);
    this.AddPlane_(
        "LR_leftwall2",
        "wall.bmp",
        new Vector2(100, 160),
        new Vector3(-200, 80, -190),
        90);
    this.AddPlane_(
        "LR_leftwall3",
        "wall.bmp",
        new Vector2(100, 160),
        new Vector3(-200, 80, -290),
        90);
    this.AddPlane_(
        "LR_leftwall4",
        "wall.bmp",
        new Vector2(60, 160),
        new Vector3(-200, 80, -370),
        90);
    this.AddPlane_(
        "LR_leftwall5",
        "wall.bmp",
        new Vector2(100, 160),
        new Vector3(-200, 80, -450),
        90);

    this.AddPlane_(
        "LR_rightwall0",
        "wall.bmp",
        new Vector2(100, 160),
        new Vector3(200, 80, 50),
        90);
    this.AddPlane_(
        "LR_rightwall1",
        "wall.bmp",
        new Vector2(100, 160),
        new Vector3(200, 80, -50),
        90);
    this.AddPlane_(
        "LR_rightwall2",
        "wall.bmp",
        new Vector2(100, 160),
        new Vector3(200, 80, -150),
        90);
    this.AddPlane_(
        "LR_rightwall3",
        "wall.bmp",
        new Vector2(100, 160),
        new Vector3(200, 80, -250),
        90);
    this.AddPlane_(
        "LR_rightwall4",
        "wall.bmp",
        new Vector2(100, 160),
        new Vector3(200, 80, -350),
        90);
    this.AddPlane_(
        "LR_rightwall5",
        "wall.bmp",
        new Vector2(100, 160),
        new Vector3(200, 80, -450),
        90);

    /*this.AddPlane_(
        "LR_backwindow1",
        "wall.bmp",
        new Vector2(66, 150),
        new Vector3(-101, 77, -498),
        0);*/
  }

  private void AddSprites_() {
    this.AddSprite_(
        "plant",
        "plant.bmp",
        new Vector2(120),
        new Vector3(160, 60, 65));
    this.AddSprite_(
        "ashtray",
        "ashtray.bmp",
        new Vector2(10, 5),
        new Vector3(165, 60, 10));
    this.AddSprite_(
        "phone",
        "phoneontable.bmp",
        new Vector2(16),
        new Vector3(145, 60, -60));

    this.AddPlane_(
        "big abstract picture",
        "painting1.bmp",
        new Vector2(52, 70),
        new Vector3(190, 100, -380),
        90);
    this.AddPlane_(
        "little painting",
        "littlepainting.bmp",
        new Vector2(20, 32),
        new Vector3(190, 118, -335),
        90);
    this.AddPlane_(
        "italy picture",
        "tuscany.bmp",
        new Vector2(32),
        new Vector3(-199, 90, -60),
        90);

    this.AddPlane_(
        "wedding gown post its",
        "gownsketch1.bmp",
        new Vector2(36),
        new Vector3(190, 50, -28),
        90);
  }

  private void AddSprite_(
      string meshName,
      string textureName,
      Vector2 widthAndHeight,
      Vector3 position) {
    var bone = this.Model.Skeleton.Root.AddChild(position);
    bone.AlwaysFaceTowardsCamera(FaceTowardsCameraType.YAW_ONLY,
                                 QuaternionUtil.CreateZyxRadians(
                                     -MathF.PI / 2,
                                     0,
                                     0));

    var material = this.lazyMaterials_[(textureName, true)];

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
      string textureName,
      in Vector2 widthAndHeight,
      Vector3 position,
      float degrees) {
    var material = this.lazyMaterials_[(textureName, false)];

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
                       (ul, new Vector2(0, 0), null),
                       (ur, new Vector2(1, 0), null),
                       (lr, new Vector2(1, 1), null),
                       (ll, new Vector2(0, 1), null),
                       material);
  }
}