using fin.image.util;
using fin.math.transform;
using fin.ui.rendering.gl.material;


namespace fin.ui.rendering.gl.scene;

public interface IRenderGraphRenderable : IRenderable {
  IReadOnlyTransform3d? Transform { get; }
  IReadOnlyGlMaterialShader? GlMaterialShader { get; }
}

public record RenderGraphElement(IRenderGraphRenderable Renderable) {
  public float Distance { get; set; }
}

public sealed class SceneStaticRenderGraph : IRenderable {
  //  TODO: How to get the default capacity?
  private readonly List<RenderGraphElement> elements_ = new();

  private SceneStaticRenderGraph() { }

  ~SceneStaticRenderGraph() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    foreach (var element in this.elements_) {
      element.Renderable.Dispose();
    }

    this.elements_.Clear();
  }

  public void Render() {
    var camera = Camera.Instance;
    foreach (var element in this.elements_) {
      var transform = element.Renderable.Transform;
      element.Distance = transform != null
          ? (camera.Position - element.Renderable.Transform.Translation)
          .LengthSquared()
          : 0;
    }

    this.elements_.Sort((lhs, rhs) => {
      const int LHS_FIRST = -1;
      const int RHS_SECOND = LHS_FIRST;
      const int RHS_FIRST = 1;
      const int LHS_SECOND = RHS_FIRST;

      var frontToBackDepthComparison = lhs.Distance.CompareTo(rhs.Distance);
      var backToFrontDepthComparison = -frontToBackDepthComparison;

      var lhsRenderable = lhs.Renderable;
      var rhsRenderable = rhs.Renderable;

      var lhsMaterialShader = lhsRenderable.GlMaterialShader;
      var rhsMaterialShader = rhsRenderable.GlMaterialShader;

      var lhsMaterial = lhsMaterialShader?.Material;
      var rhsMaterial = rhsMaterialShader?.Material;

      if (lhsMaterial == null && rhsMaterial == null) {
        return backToFrontDepthComparison;
      }
      if (lhsMaterial == null) {
        return LHS_SECOND;
      }
      if (rhsMaterial == null) {
        return RHS_SECOND;
      }

      var isTransparent =
          lhsMaterial.TransparencyType is TransparencyType.TRANSPARENT;
      if (isTransparent &&
          rhsMaterial.TransparencyType != TransparencyType.TRANSPARENT) {
        return lhsMaterial.TransparencyType.CompareTo(
            rhsMaterial.TransparencyType);
      }

      if (isTransparent) {
        return backToFrontDepthComparison;
      }

      var lhsShaderProgram = lhsMaterialShader!.ShaderProgram;
      var rhsShaderProgram = rhsMaterialShader!.ShaderProgram;
      if (lhsShaderProgram != rhsShaderProgram) {
        return lhsShaderProgram.GetHashCode()
                               .CompareTo(rhsShaderProgram.GetHashCode());
      }

      if (lhsMaterial != rhsMaterial) {
        return lhsMaterial.GetHashCode()
                          .CompareTo(rhsMaterial.GetHashCode());
      }

      return frontToBackDepthComparison;
    });

    foreach (var element in this.elements_) {
      element.Renderable.Render();
    }
  }
}