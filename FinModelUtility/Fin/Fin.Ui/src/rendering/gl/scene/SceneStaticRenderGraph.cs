using fin.data.dictionaries;
using fin.data.queues;
using fin.image.util;
using fin.scene;
using fin.ui.rendering.gl.material;
using fin.ui.rendering.gl.model;


namespace fin.ui.rendering.gl.scene;

public interface IRenderGraphParams {
  ISceneNodeInstance Node { get; }
  IReadOnlyGlMaterialShader? GlMaterialShader { get; }
  int MinPrimitiveIndex { get; }
  uint InversePriority { get; }
}

public class RenderGraphComponentRenderer : IRenderGraphParams {
  public required ISceneNodeInstance Node { get; init; }
  public IReadOnlyGlMaterialShader? GlMaterialShader => null;
  public required ISceneNodeRenderComponent RenderComponent { get; init; }
  public int MinPrimitiveIndex => int.MaxValue;
  public uint InversePriority => uint.MaxValue;

  public override string ToString() => "(Component)";
}

public class RenderGraphMaterialRenderer : IRenderGraphParams {
  public required ISceneNodeInstance Node { get; init; }
  public required IReadOnlyGlMaterialShader GlMaterialShader { get; init; }
  public required IModelRenderer ModelRenderer { get; init; }
  public required IMaterialRenderer MaterialRenderer { get; init; }
  public required int MinPrimitiveIndex { get; init; }
  public required uint InversePriority { get; init; }

  public override string ToString() {
    var material = this.GlMaterialShader.Material;
    var shaderProgram = this.GlMaterialShader.ShaderProgram;

    return
        $"(Material: {material?.Name ?? "(null)"}, {shaderProgram}, inverse priority: {this.InversePriority}, {material?.TransparencyType ?? TransparencyType.TRANSPARENT})";
  }
}

public record RenderGraphElement(IRenderGraphParams Params) {
  public float Distance { get; set; }

  public override string ToString()
    => $"{this.Params}, Distance: {this.Distance}";
}

public sealed class SceneStaticRenderGraph : IRenderable {
  private readonly ISceneInstance scene_;

  private readonly
      List<(IReadOnlySceneNodeInstance, SimpleModelRenderComponent)>
      modelRenderComponents_ = new();

  private List<RenderGraphElement>? elements_;
  private IReadOnlySceneNode? selectedNode_;

  public SceneStaticRenderGraph(ISceneInstance scene) {
    this.scene_ = scene;
    SelectedNodeService.OnNodeSelected += selectedNode
        => this.selectedNode_ = selectedNode;
  }

  ~SceneStaticRenderGraph() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.elements_?.Clear();
  }

  private void GenerateModelIfNull_() {
    if (this.elements_ != null) {
      return;
    }

    this.elements_ = new();
    foreach (var node in this.scene_.EnumerateAllNodes()) {
      foreach (var renderComponent in node.Definition.Components
                                          .OfType<
                                              ISceneNodeRenderComponent>()) {
        if (renderComponent is not SimpleModelRenderComponent
            modelRenderComponent) {
          this.elements_.Add(
              new RenderGraphElement(
                  new RenderGraphComponentRenderer {
                      Node = node,
                      RenderComponent = renderComponent,
                  }));
          continue;
        }

        this.modelRenderComponents_.Add((node, modelRenderComponent));
        var modelRenderer = modelRenderComponent.ModelRenderer;
        modelRenderer.GenerateModelIfNull();

        var meshRendererQueue
            = new FinQueue<IMeshRenderer>(modelRenderer.MeshRenderers);
        while (meshRendererQueue.TryDequeue(out var meshRenderer)) {
          meshRendererQueue.Enqueue(meshRenderer.Children);

          foreach (var primitiveRenderer in meshRenderer.MaterialRenderers) {
            this.elements_.Add(
                new RenderGraphElement(
                    new RenderGraphMaterialRenderer {
                        Node = node,
                        GlMaterialShader = primitiveRenderer.GlMaterialShader,
                        MinPrimitiveIndex = primitiveRenderer.MinPrimitiveIndex,
                        InversePriority = primitiveRenderer.InversePriority,
                        ModelRenderer = modelRenderer,
                        MaterialRenderer = primitiveRenderer,
                    }));
          }
        }
      }
    }
  }

  private readonly RenderGraphComparer comparer_ = new();

  public void Render() {
    this.GenerateModelIfNull_();

    foreach (var (node, modelRenderComponent) in this.modelRenderComponents_) {
      GlTransform.PushMatrix();
      GlTransform.MultMatrix(node.Transform.WorldMatrix);

      modelRenderComponent.TickAnimatables();
      modelRenderComponent.ModelRenderer.UpdateMatricesUbo();

      GlTransform.PopMatrix();
    }

    var camera = Camera.Instance;
    foreach (var element in this.elements_!) {
      var transform = element.Params.Node.Transform;
      element.Distance
          = (camera.Position - transform.LocalTranslation).LengthSquared();
    }

    this.elements_.Sort(this.comparer_);

    if (this.selectedNode_ != null) {
      foreach (var element in this.elements_) {
        var prms = element.Params;
        if (this.selectedNode_ != prms.Node.Definition) {
          continue;
        }

        GlUtil.RenderOutline(() => RenderParams_(prms));
      }
    }

    foreach (var element in this.elements_) {
      var prms = element.Params;
      RenderParams_(prms);
    }

    if (this.selectedNode_ != null) {
      foreach (var element in this.elements_) {
        var prms = element.Params;

        if (this.selectedNode_ != prms.Node.Definition) {
          continue;
        }

        GlUtil.RenderHighlight(() => RenderParams_(prms));
      }
    }
  }

  private static void RenderParams_(IRenderGraphParams prms) {
    GlTransform.PushMatrix();
    GlTransform.MultMatrix(prms.Node.Transform.WorldMatrix);

    switch (prms) {
      case RenderGraphComponentRenderer componentRenderer: {
        componentRenderer.RenderComponent.Render(prms.Node);
        break;
      }
      case RenderGraphMaterialRenderer primitiveRenderer: {
        primitiveRenderer.ModelRenderer.BindMatricesUbo();
        primitiveRenderer.MaterialRenderer.Render();
        break;
      }
    }

    GlTransform.PopMatrix();
  }

  private sealed class RenderGraphComparer : IComparer<RenderGraphElement> {
    public int Compare(RenderGraphElement lhs, RenderGraphElement rhs) {
      const int LHS_FIRST = -1;
      const int RHS_SECOND = LHS_FIRST;
      const int RHS_FIRST = 1;
      const int LHS_SECOND = RHS_FIRST;

      var frontToBackDepthComparison = lhs.Distance.CompareTo(rhs.Distance);
      var backToFrontDepthComparison = -frontToBackDepthComparison;

      var lhsRenderable = lhs.Params;
      var rhsRenderable = rhs.Params;

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
        var lhsInversePriority = lhsRenderable.InversePriority;
        var rhsInversePriority = rhsRenderable.InversePriority;
        if (lhsInversePriority != rhsInversePriority) {
          return lhsInversePriority.CompareTo(rhsInversePriority);
        }

        // Helps keep rendering stable.
        var lhsMinPrimitiveIndex = lhsRenderable.MinPrimitiveIndex;
        var rhsMinPrimitiveIndex = rhsRenderable.MinPrimitiveIndex;
        if (lhsMinPrimitiveIndex != rhsMinPrimitiveIndex) {
          return lhsMinPrimitiveIndex.CompareTo(rhsMinPrimitiveIndex);
        }

        return backToFrontDepthComparison;
      }

      var lhsShaderProgram = lhsMaterialShader!.ShaderProgram.ProgramId;
      var rhsShaderProgram = rhsMaterialShader!.ShaderProgram.ProgramId;
      if (lhsShaderProgram != rhsShaderProgram) {
        return lhsShaderProgram.CompareTo(rhsShaderProgram);
      }

      if (lhsMaterial != rhsMaterial) {
        return lhsMaterial.Index.CompareTo(rhsMaterial.Index);
      }

      return frontToBackDepthComparison;
    }
  }
}