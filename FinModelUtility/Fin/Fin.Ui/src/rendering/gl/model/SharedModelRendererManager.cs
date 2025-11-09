using fin.model;
using fin.scene;
using fin.util.types;


namespace fin.ui.rendering.gl.model;

[IocCandiate]
public static class SharedModelRendererManager {
  public static IModelRenderer RequestRenderer(
      ISceneNodeInstance instance,
      IReadOnlyModel model,
      IReadOnlyLighting lighting) {
    return null!;
  }


  public static void RenderAll() {
    // TODO: Sort meshes by distance to the camera and layer
  }
}