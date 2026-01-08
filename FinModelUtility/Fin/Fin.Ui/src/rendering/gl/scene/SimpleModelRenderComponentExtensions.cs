using fin.model;
using fin.ui.rendering.gl.scene;

namespace fin.scene;

public static class SimpleModelRenderComponentExtensions {
  public static void AddSceneModel(this ISceneNode sceneNode,
                                   IReadOnlyModel model)
    => sceneNode.AddComponent(new SimpleModelRenderComponent(model));
}