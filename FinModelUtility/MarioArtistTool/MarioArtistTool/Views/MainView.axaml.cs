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

using MarioArtistTool.config;
using MarioArtistTool.backgrounds;

using marioartisttool.services;
using marioartisttool.util;


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

      switch (file?.FileType.ToLower()) {
        case ".tstlt": {
          try {
            var bundle = new TstltModelFileBundle(file);
            var model = TstltModelLoader.Import(bundle, out var gender);

            switch (gender) {
              case Gender.BOY: {
                area.BackgroundImage
                    = AssetLoaderUtil.LoadImage("backgrounds/boy/background.png");
                area.BackgroundImageScale = .3f;
                break;
              }
              case Gender.GIRL: {
                area.BackgroundImage
                    = AssetLoaderUtil.LoadImage("backgrounds/girl/background.png");
                area.BackgroundImageScale = .3f;
                sceneryRenderer = new GirlSceneryRenderer();
                break;
              }
              case Gender.OTHER: {
                area.BackgroundImage
                    = AssetLoaderUtil.LoadImage("backgrounds/other/background.png");
                area.BackgroundImageScale = .3f;
                break;
              }
            }

            var config = Config.INSTANCE;
            config.MostRecentFileName = file.FullPath;
            config.Save();

            var characterObj = area.AddObject();
            characterObj.AddSceneModel(model);

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

      LoadingStatusService.IsLoading = false;
    };

    var camera = this.ViewerGlPanel.Camera;
    camera.Position = new Vector3(0, -1.35f, .3f);
    camera.PitchDegrees = 0;
    camera.YawDegrees = 90;
  }
}