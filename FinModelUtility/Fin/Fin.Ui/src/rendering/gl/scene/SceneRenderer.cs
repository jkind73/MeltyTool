using fin.scene;
using fin.ui.rendering.gl.ubo;


namespace fin.ui.rendering.gl.scene;

public sealed class SceneRenderer : IRenderable {
  private const bool USE_RENDER_GRAPH = true;

  private readonly ISceneInstance scene_;

  private LightsUbo? lightsUbo_;

  private IReadOnlyList<SceneAreaRenderer> areaRenderers_;
  private readonly SceneStaticRenderGraph renderGraph_;

  public float Scale {
    set => this.renderGraph_?.Scale = value;
  }
  public float NearPlane {
    set => this.renderGraph_?.NearPlane = value;
  }
  public float FarPlane {
    set => this.renderGraph_?.FarPlane = value;
  }

  public SceneRenderer(ISceneInstance scene) {
    this.scene_ = scene;
    if (USE_RENDER_GRAPH) {
      this.renderGraph_ = new(scene);
    } else {
      this.areaRenderers_ = scene.Areas
      .Select(area => new SceneAreaRenderer(area))
      .ToArray();
    }
  }

  ~SceneRenderer() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.lightsUbo_?.Dispose();

    if (USE_RENDER_GRAPH) {
      this.renderGraph_.Dispose();
    } else {
      foreach (var areaRenderer in this.areaRenderers_) {
        areaRenderer.Dispose();
      }
    }
  }

  public void Render() {
    this.lightsUbo_ ??= new LightsUbo();
    this.lightsUbo_.UpdateData(this.scene_.Lighting);
    this.lightsUbo_.Bind();

    if (USE_RENDER_GRAPH) {
      this.renderGraph_.Render();
    } else {
      foreach (var areaRenderer in this.areaRenderers_) {
        areaRenderer.Render();
      }
    }
  }
}