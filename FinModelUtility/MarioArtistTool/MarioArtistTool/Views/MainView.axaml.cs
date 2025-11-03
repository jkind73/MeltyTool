using System;
using System.Linq;
using System.Numerics;

using Avalonia.Controls;
using Avalonia.Threading;

using fin.io.web;
using fin.model.io;
using fin.scene;
using fin.scene.components;
using fin.scene.instance;
using fin.services;
using fin.ui.rendering.gl.scene;
using fin.util.asserts;

using marioartist.api;

using marioartisttool.services;

using MarioArtistTool.backgrounds;
using MarioArtistTool.config;
using MarioArtistTool.view;

using marioartisttool.ViewModels;


namespace marioartisttool.Views;

public partial class MainView : UserControl {
  private (Vector3 translation, float pitchDegrees, float yawDegrees)?
      ma3d1CameraTransform_ = null;

  private IModelFileBundle? currentModelFileBundle_;

  public MainView() {
    InitializeComponent();

    MfsFileSystemService.OnFileSelected += file => {
      LoadingStatusService.IsLoading = true;

      var camera = this.ViewerGlPanel.Camera;
      if (this.currentModelFileBundle_ is Ma3d1ModelFileBundle or null) {
        this.ma3d1CameraTransform_ = (
            camera.Position, camera.PitchDegrees, camera.YawDegrees);
      }

      var scene = new SceneImpl {
          FileBundle = null,
          Files = null
      };

      var area = scene.AddArea();

      var allowMovingCamera = true;
      var showGrid = true;

      var viewerCursor = MainViewModel.ArrowCursor;
      if (this.ma3d1CameraTransform_ != null) {
        (camera.Position, camera.PitchDegrees, camera.YawDegrees)
            = this.ma3d1CameraTransform_.Value;
      }

      this.ViewerGlPanel.BackdropRenderer = null;

      switch (file?.FileType.ToLower()) {
        case ".ma3d1": {
          var bundle = new Ma3d1ModelFileBundle(file);
          var model = new Ma3d1ModelLoader().Import(bundle);

          var obj = area.AddRootNode();
          obj.AddSceneModel(model);

          var lightingObj = area.AddRootNode();
          scene.CreateDefaultLighting(lightingObj);

          this.currentModelFileBundle_ = bundle;

          break;
        }
        case ".tstlt": {
          allowMovingCamera = false;
          showGrid = false;

          try {
            var bundle = new TstltModelFileBundle(file);
            var model = TstltModelLoader.Import(bundle, out var gender);

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
                                      false),
                    model.Skeleton
                        .Bones
                        .Single(b => b.Name?.StartsWith(
                                         $"{JointIndex.TORSO}:") ??
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

            this.currentModelFileBundle_ = bundle;
            viewerCursor = MainViewModel.ThumbOutCursor;

            camera.Position = new Vector3(0, -1.5f, .35f);
            camera.PitchDegrees = 0;
            camera.YawDegrees = 90;

            this.ViewerGlPanel.BackdropRenderer
                = new TalentStudioBackdropRenderer(gender);
          } catch (Exception e) {
            ExceptionService.HandleException(e, new LoadFileException(file));
            this.ViewerGlPanel.Scene = null;
          }
          
          break;
        }
      }

      this.ViewerGlPanel.BackdropRenderer ??= new PolygonStudioSkyboxRenderer();

      Dispatcher.UIThread.Invoke(() => {
        var mainViewModel = this.DataContext.AssertAsA<MainViewModel>();
        mainViewModel.ViewerCursor = viewerCursor;
      });

      var sceneInstance = new SceneInstanceImpl(scene);
      this.ViewerGlPanel.Scene = sceneInstance;

      this.ViewerGlPanel.AllowMovingCamera = allowMovingCamera;
      this.ViewerGlPanel.ShowGrid = showGrid;

      LoadingStatusService.IsLoading = false;
    };
  }
}