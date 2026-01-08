using fin.scene;
using fin.ui.rendering.gl.ubo;


namespace fin.ui.rendering.gl.scene;

public sealed class SceneRenderer(ISceneInstance scene) : IRenderable {
  private LightsUbo? lightsUbo_;

  public IReadOnlyList<SceneAreaRenderer> AreaRenderers { get; } = scene.Areas
      .Select(area => new SceneAreaRenderer(area))
      .ToArray();

  ~SceneRenderer() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.lightsUbo_?.Dispose();

    foreach (var areaRenderer in this.AreaRenderers) {
      areaRenderer.Dispose();
    }
  }

  public void Render() {
    this.lightsUbo_ ??= new LightsUbo();
    this.lightsUbo_.UpdateData(scene.Lighting);
    this.lightsUbo_.Bind();

    foreach (var areaRenderer in this.AreaRenderers) {
      areaRenderer.Render();
    }
  }
}