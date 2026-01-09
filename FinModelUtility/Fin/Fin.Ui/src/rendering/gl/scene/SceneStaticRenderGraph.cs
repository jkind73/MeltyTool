using fin.data.queues;
using fin.image.util;
using fin.math;
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
  public ulong SortKey { get; set; }
  public override string ToString() => $"{this.Params}, Sort key: {this.SortKey}";
}

public sealed class SceneStaticRenderGraph : IRenderable {
  private readonly ISceneInstance scene_;

  private readonly
      List<(IReadOnlySceneNodeInstance, SimpleModelRenderComponent)>
      modelRenderComponents_ = new();

  private List<RenderGraphElement>? elements_;
  private IReadOnlySceneNode? selectedNode_;

  public float Scale { get; set; }
  public float NearPlane { get; set; }
  public float FarPlane { get; set; }

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

    // Loosely based on: https://realtimecollisiondetection.net/blog/?p=86
    // Transparent bits
    const int transparentInversePriorityBitCount = 16;
    var transparentInversePriorityMask
        = ((ulong) 1 << transparentInversePriorityBitCount) - 1;
    const int transparentMinPrimitiveIndexBitCount = 16;
    var transparentMinPrimitiveIndexMask
        = ((ulong) 1 << transparentMinPrimitiveIndexBitCount) - 1;
    const int transparentDepthBitCount = 31;
    var transparentDepthMax = ((ulong) 1 << transparentDepthBitCount) - 1;

    // Opaque bits
    const int opaqueProgramIdBitCount = 20;
    var opaqueProgramIdMask = ((ulong) 1 << opaqueProgramIdBitCount) - 1;
    const int opaqueMaterialIndexBitCount = 12;
    var opaqueMaterialIndexMask
        = ((ulong) 1 << opaqueMaterialIndexBitCount) - 1;
    const int opaqueDepthBitCount = 31;
    var opaqueDepthMax = ((ulong) 1 << opaqueDepthBitCount) - 1;

    var camera = Camera.Instance;
    foreach (var element in this.elements_!) {
      var transform = element.Params.Node.Transform;

      ulong sortKey = 0;

      var materialShader = element.Params.GlMaterialShader;
      var material = materialShader?.Material;

      var isTransparent
          = material?.TransparencyType is TransparencyType.TRANSPARENT or null;
      sortKey |= (isTransparent ? (ulong) 1 : 0) << 63;

      var distance = (camera.Position - transform.LocalTranslation).Length() * this.Scale;
      var distance0To1
          = ((distance - this.NearPlane) / (this.FarPlane - this.NearPlane))
          .Clamp(0, 1);

      var prms = element.Params;

      if (isTransparent) {
        var inversePriorityBits
            = prms.InversePriority & transparentInversePriorityMask;
        sortKey
            |= inversePriorityBits <<
               (transparentDepthBitCount +
                transparentMinPrimitiveIndexBitCount);

        var minPrimitiveIndexBits
            = (ulong) prms.MinPrimitiveIndex & transparentMinPrimitiveIndexMask;
        sortKey |= minPrimitiveIndexBits << transparentDepthBitCount;

        var distance1To0 = 1 - distance0To1;
        var depthBits = (ulong) (distance1To0 * transparentDepthMax);
        sortKey |= depthBits;
      } else {
        var programIdBits
            = (ulong) (materialShader?.ShaderProgram.ProgramId ?? 0) & opaqueProgramIdMask;
        sortKey
            |= programIdBits <<
               (opaqueDepthBitCount +
                opaqueMaterialIndexBitCount);

        var materialIndexBits = (ulong) (material?.Index ?? 0) & opaqueMaterialIndexMask;
        sortKey |= materialIndexBits << opaqueDepthBitCount;

        var depthBits = (ulong) (distance0To1 * opaqueDepthMax);
        sortKey |= depthBits;
      }

      element.SortKey = sortKey;
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
    public int Compare(RenderGraphElement lhs, RenderGraphElement rhs)
      => lhs.SortKey.CompareTo(rhs.SortKey);
  }
}