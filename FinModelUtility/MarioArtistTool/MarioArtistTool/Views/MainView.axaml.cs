using System;
using System.Numerics;

using Avalonia.Controls;

using fin.io.web;
using fin.scene;
using fin.scene.instance;
using fin.services;
using fin.ui.rendering;

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

    MfsFileSystemService.OnFileSelected += file => {
      LoadingStatusService.IsLoading = true;

      var scene = new SceneImpl {
          FileBundle = null,
          Files = null
      };

      var area = scene.AddArea();
      IRenderable? sceneryRenderer = null;

      var allowMovingCamera = true;

      switch (file?.FileType.ToLower()) {
        case ".ma3d1": {
          var bundle = new Ma3d1ModelFileBundle(file);
          var model = new Ma3d1ModelLoader().Import(bundle);

          var obj = area.AddObject();
          obj.AddSceneModel(model);

          var lightingObj = area.AddObject();
          scene.CreateDefaultLighting(lightingObj);

          break;
        }
        case ".tstlt": {
          allowMovingCamera = false;
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
                break;
              }
            }

            var config = Config.INSTANCE;
            config.MostRecentFileName = file.FullPath;
            config.Save();

            var characterObj = area.AddObject();
            characterObj.AddSceneModel(model);

            var talentHeadTickComponent = new RotateTalentTickComponent();
            characterObj.AddComponent(talentHeadTickComponent);

            var shadowObj = area.AddObject();
            shadowObj.SetPosition(20, -20, -100);
            shadowObj.SetScale(1, 1, 0);
            shadowObj.AddComponent(new ShadowRenderer(model));

            var lightingObj = area.AddObject();
            scene.CreateDefaultLighting(lightingObj);
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
        area.CreateCustomSkyboxObject();
      }

      if (sceneryRenderer != null) {
        var backgroundObj = area.AddObject();
        backgroundObj.AddRenderable(sceneryRenderer);
      }

      var sceneInstance = new SceneInstanceImpl(scene);
      this.ViewerGlPanel.Scene = sceneInstance;

      this.ViewerGlPanel.AllowMovingCamera = allowMovingCamera;

      LoadingStatusService.IsLoading = false;
    };
  }
}