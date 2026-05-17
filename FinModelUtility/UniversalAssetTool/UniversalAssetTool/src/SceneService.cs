using fin.io.web;
using fin.scene;
using fin.services;
using fin.ui.rendering;
using fin.ui.rendering.gl.scene;

using uni.api;
using uni.ui.winforms.common.fileTreeView;

namespace uni;

public static class SceneService {
  static SceneService() {
    FileBundleService.OnFileBundleOpened
        += (fileTreeLeafNode, fileBundle) => {
          if (fileBundle is ISceneFileBundle sceneFileBundle) {
            SceneTypeService.IsASingleModel = false;
            LoadingStatusService.IsLoading = true;

            try {
              var scene = new GlobalSceneImporter().Import(sceneFileBundle);
              OpenScene(fileTreeLeafNode, scene);
            } catch (Exception e) {
              ExceptionService.HandleException(
                  e,
                  new LoadFileBundleExceptionContext(fileBundle));
            }

            LoadingStatusService.IsLoading = false;
          }
        };

    ModelService.OnModelOpened
        += (fileTreeLeafNode, model) => {
          SceneTypeService.IsASingleModel = true;
          var scene = new SceneImpl {
              FileBundle = model.FileBundle,
              Files = model.Files
          };
          var area = scene.AddArea();
          var obj = area.AddRootNode();

          scene.CreateDefaultLighting(obj, [model]);
          obj.AddComponent(new SimpleModelRenderComponent(model));

          OpenScene(fileTreeLeafNode, scene);
        };
  }

  public static event Action<IFileTreeLeafNode?, IScene> OnSceneOpened;

  public static void OpenScene(IFileTreeLeafNode? fileTreeLeafNode,
                               IScene scene)
    => OnSceneOpened?.Invoke(fileTreeLeafNode, scene);
}