using fin.scene;

namespace fin.ui.rendering {
  public static class SelectedNodeService {
    public static event Action<IReadOnlySceneNode?> OnNodeSelected;

    public static void SelectNode(IReadOnlySceneNode? node)
      => OnNodeSelected?.Invoke(node);
  }
}