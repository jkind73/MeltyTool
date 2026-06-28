using fin.scene;
using fin.scene.instance;
using fin.util.types;

using uni.ui.winforms.common.fileTreeView;

namespace uni.services;

[IocCandiate]
public static class SceneInstanceService {
  static SceneInstanceService() {
    SceneService.OnSceneSuccessfullyOpened
        += (fileTreeLeafNode, scene) =>
            OpenSceneInstance(fileTreeLeafNode,
                              new SceneInstanceImpl(scene));
  }

  public static event Action<IFileTreeLeafNode?, ISceneInstance>
      OnSceneInstanceOpened;

  public static void OpenSceneInstance(IFileTreeLeafNode? fileTreeLeafNode,
                                       ISceneInstance sceneInstance)
    => OnSceneInstanceOpened?.Invoke(fileTreeLeafNode, sceneInstance);
}