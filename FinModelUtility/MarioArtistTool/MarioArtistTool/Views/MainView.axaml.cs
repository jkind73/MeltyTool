using System;
using System.Linq;
using System.Numerics;

using Avalonia.Controls;

using fin.io.web;
using fin.scene;
using fin.scene.components;
using fin.scene.instance;
using fin.services;
using fin.ui.rendering;
using fin.ui.rendering.gl.scene;

using marioartist.api;
using marioartist.schema.talent_studio;

using marioartisttool.services;
using marioartisttool.util;

using MarioArtistTool.backgrounds;
using MarioArtistTool.config;
using MarioArtistTool.view;


namespace marioartisttool.Views;

public partial class MainView : UserControl {
  public MainView() {
    InitializeComponent();

    this.ViewerGlPanel.SkyboxRenderer = new PolygonStudioSkyboxRenderer();

    MfsFileSystemService.OnFileSelected += file => {
      LoadingStatusService.IsLoading = true;

      var scene = new SceneImpl {
          FileBundle = null,
          Files = null
      };

      var area = scene.AddArea();
      IRenderable? sceneryRenderer = null;

      var allowMovingCamera = true;
      var showGrid = true;

      switch (file?.FileType.ToLower()) {
        case ".ma3d1": {
          var bundle = new Ma3d1ModelFileBundle(file);
          var model = new Ma3d1ModelLoader().Import(bundle);

          var obj = area.AddRootNode();
          obj.AddSceneModel(model);

          var lightingObj = area.AddRootNode();
          scene.CreateDefaultLighting(lightingObj);

          break;
        }
        case ".tstlt": {
          allowMovingCamera = false;
          showGrid = false;

          try {
            var bundle = new TstltModelFileBundle(file);
            var model = TstltModelLoader.Import(bundle, out var gender);

            switch (gender) {
              case Gender.BOY: {
                area.BackgroundImage
                    = AssetLoaderUtil.LoadImage(
                        "backgrounds/boy/background.png");
                area.BackgroundImageScale = .3f;
                break;
              }
              case Gender.GIRL: {
                area.BackgroundImage
                    = AssetLoaderUtil.LoadImage(
                        "backgrounds/girl/background.png");
                area.BackgroundImageScale = .3f;
                sceneryRenderer = new GirlSceneryRenderer();
                break;
              }
              case Gender.OTHER: {
                area.BackgroundImage
                    = AssetLoaderUtil.LoadImage(
                        "backgrounds/other/background.png");
                area.BackgroundImageScale = .3f;
                sceneryRenderer = new OtherSceneryRenderer();
                break;
              }
            }

            var config = Config.INSTANCE;
            config.MostRecentFileName = file.FullPath;
            config.Save();

            var lightingObj = area.AddRootNode();
            var lighting = scene.CreateDefaultLighting(lightingObj, [model]);

            var modelRenderComponent
                = new SimpleModelRenderComponent(model, lighting);

            var characterObj = area.AddRootNode();
            characterObj.AddComponent(
                new LookAtMouseTickComponent(
                    modelRenderComponent.SimpleBoneTransformView,
                    model.Skeleton
                         .Bones
                         .Single(b => b.Name?.StartsWith(
                                          $"{JointIndex.NECK}:") ??
                                      false)));

            var modelObj = characterObj.AddChildNode();
            modelObj.AddComponent(modelRenderComponent);
            modelObj.AddComponent(new RotateTalentTickComponent());

            var shadowPlacementObj = characterObj.AddChildNode();
            shadowPlacementObj.SetPosition(20, -20, -100);
            shadowPlacementObj.SetScale(1, 1, 0);

            var shadowModelObj = shadowPlacementObj.AddChildNode();
            shadowModelObj.AddComponent(
                new ShadowRenderComponent(
                    new LambdaSceneNodeRenderComponent(_ => modelRenderComponent
                        .Render(false))));
            shadowModelObj.AddComponent(new RotateTalentTickComponent());
          } catch (Exception e) {
            ExceptionService.HandleException(e, new LoadFileException(file));
            this.ViewerGlPanel.Scene = null;
          }

          var camera = this.ViewerGlPanel.Camera;
          camera.Position = new Vector3(0, -1.35f, .3f);
          camera.PitchDegrees = 0;
          camera.YawDegrees = 90;

          break;
        }
      }

      if (area.BackgroundImage != null) {
        // Hides the default skybox.
        area.CreateCustomSkyboxNode();
      }

      if (sceneryRenderer != null) {
        var backgroundObj = area.AddRootNode();
        backgroundObj.AddRenderable(sceneryRenderer);
      }

      var sceneInstance = new SceneInstanceImpl(scene);
      this.ViewerGlPanel.Scene = sceneInstance;

      this.ViewerGlPanel.AllowMovingCamera = allowMovingCamera;
      this.ViewerGlPanel.ShowGrid = showGrid;

      LoadingStatusService.IsLoading = false;
    };
  }
}