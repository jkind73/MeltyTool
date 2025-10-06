using fin.model;
using fin.scene;

namespace fin.ui.rendering.gl.scene;

public sealed class SceneAreaRenderer : IRenderable, IDisposable {
  private readonly SceneNodeRenderer[] rootNodeRenderers_;

  public SceneAreaRenderer(ISceneAreaInstance sceneArea,
                           IReadOnlyLighting? lighting) {
    var customSkybox = sceneArea.CustomSkyboxObject;
    this.CustomSkyboxRenderer = customSkybox != null
        ? new SceneNodeRenderer(customSkybox, lighting)
        : null;

    this.rootNodeRenderers_
        = sceneArea
          .RootNodes
          .Where(obj => obj != customSkybox)
          .Select(obj => new SceneNodeRenderer(obj, lighting))
          .ToArray();
  }

  ~SceneAreaRenderer() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.CustomSkyboxRenderer?.Dispose();
    foreach (var objRenderer in this.RootNodeRenderers) {
      objRenderer.Dispose();
    }
  }

  public IReadOnlyList<SceneNodeRenderer> RootNodeRenderers
    => this.rootNodeRenderers_;

  public SceneNodeRenderer? CustomSkyboxRenderer { get; }

  public void Render() {
    foreach (var objRenderer in this.rootNodeRenderers_) {
      objRenderer.Render();
    }
  }
}