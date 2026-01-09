using fin.scene;
using fin.ui.rendering.gl.ubo;


namespace fin.ui.rendering.gl.scene;

public sealed class SceneRenderer(ISceneInstance scene) : IRenderable {
  private const bool USE_RENDER_GRAPH = false;

  private LightsUbo? lightsUbo_;

  public IReadOnlyList<SceneAreaRenderer> AreaRenderers { get; } = scene.Areas
      .Select(area => new SceneAreaRenderer(area))
      .ToArray();
  private readonly SceneStaticRenderGraph renderGraph_ = new(scene);

  ~SceneRenderer() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.lightsUbo_?.Dispose();

    this.renderGraph_.Dispose();
    foreach (var areaRenderer in this.AreaRenderers) {
      areaRenderer.Dispose();
    }
  }

  public void Render() {
    this.lightsUbo_ ??= new LightsUbo();
    this.lightsUbo_.UpdateData(scene.Lighting);
    this.lightsUbo_.Bind();

    if (USE_RENDER_GRAPH) {
      this.renderGraph_.Render();
    } else {
      foreach (var areaRenderer in this.AreaRenderers) {
        areaRenderer.Render();
      }
    }
  }
}