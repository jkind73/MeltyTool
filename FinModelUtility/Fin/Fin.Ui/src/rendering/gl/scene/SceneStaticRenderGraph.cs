using fin.data.queues;
using fin.image.util;
using fin.math;
using fin.model;
using fin.model.util;
using fin.scene;
using fin.ui.rendering.gl.material;
using fin.ui.rendering.gl.model;

namespace fin.ui.rendering.gl.scene;

public interface IRenderGraphParams {
  ISceneNodeInstance Node { get; }
  IReadOnlyGlMaterialShader? GlMaterialShader { get; }
  bool IsSelected { get; }
  bool IsTransparent { get; }
  int MinPrimitiveIndex { get; }
  uint InversePriority { get; }
  int VaoId { get; }
}

public class RenderGraphComponentRenderer : IRenderGraphParams {
  public required ISceneNodeInstance Node { get; init; }
  public IReadOnlyGlMaterialShader? GlMaterialShader => null;
  public required ISceneNodeRenderComponent RenderComponent { get; init; }
  public bool IsSelected => false;
  public bool IsTransparent => true;
  public int MinPrimitiveIndex => int.MaxValue;
  public uint InversePriority => uint.MaxValue;
  public int VaoId => 0;

  public override string ToString() => "(Component)";
}

public class RenderGraphMaterialRenderer : IRenderGraphParams {
  public required ISceneNodeInstance Node { get; init; }
  public required IReadOnlyGlMaterialShader GlMaterialShader { get; init; }
  public required IModelRenderer ModelRenderer { get; init; }
  public required IMaterialRenderer MaterialRenderer { get; init; }
  public bool IsSelected => this.MaterialRenderer.IsSelected;
  public required bool IsTransparent { get; init; }
  public required int MinPrimitiveIndex { get; init; }
  public required uint InversePriority { get; init; }
  public int VaoId => this.ModelRenderer.VaoId;

  public override string ToString() {
    var material = this.GlMaterialShader.Material;
    var shaderProgram = this.GlMaterialShader.ShaderProgram;

    return
        $"(Material: {material?.Name ?? "(null)"}, {shaderProgram}, inverse priority: {this.InversePriority}, transparent: {this.IsTransparent})";
  }
}

public class RenderGraphElement {
  public RenderGraphElement(IRenderGraphParams prms) {
    this.Params = prms;
    this.InitSortKey();
  }

  public IRenderGraphParams Params { get; }
  public ulong SortKey { get; private set; }

  public override string ToString() => $"{this.Params}, Sort key: {this.SortKey}";

  // Loosely based on: https://realtimecollisiondetection.net/blog/?p=86
  // Transparent bits
  public const int transparentInversePriorityBitCount = 16;
  public const ulong transparentInversePriorityMask
      = ((ulong) 1 << transparentInversePriorityBitCount) - 1;
  public const int transparentMinPrimitiveIndexBitCount = 16;
  public const ulong transparentMinPrimitiveIndexMask
      = ((ulong) 1 << transparentMinPrimitiveIndexBitCount) - 1;
  public const int transparentDepthBitCount = 31;
  public const ulong transparentDepthMax = ((ulong) 1 << transparentDepthBitCount) - 1;

  // Opaque bits
  public const int opaqueProgramIdBitCount = 16;
  public const ulong opaqueProgramIdMask = ((ulong) 1 << opaqueProgramIdBitCount) - 1;
  public const int opaqueVaoIdBitCount = 12;
  public const ulong opaqueVaoIdMask = ((ulong) 1 << opaqueVaoIdBitCount) - 1;
  public const int opaqueMaterialIndexBitCount = 12;
  public const ulong opaqueMaterialIndexMask
      = ((ulong) 1 << opaqueMaterialIndexBitCount) - 1;
  public const int opaqueDepthBitCount = 23;
  public const ulong opaqueDepthMax = ((ulong) 1 << opaqueDepthBitCount) - 1;

  public void InitSortKey() {
    ulong sortKey = 0;

    var prms = this.Params;
    var materialShader = prms.GlMaterialShader;
    var material = materialShader?.Material;

    var isTransparent = prms.IsTransparent;
    sortKey |= (isTransparent ? (ulong) 1 : 0) << 63;

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
    } else {
      var programIdBits
          = (ulong) (materialShader?.ShaderProgram.ProgramId ?? 0) & opaqueProgramIdMask;
      sortKey
          |= programIdBits <<
             (opaqueDepthBitCount +
              opaqueMaterialIndexBitCount +
              opaqueVaoIdBitCount);

      var vaoIdBits = (ulong) (prms.VaoId) & opaqueVaoIdMask;
      sortKey
          |= vaoIdBits <<
             (opaqueDepthBitCount +
              opaqueMaterialIndexBitCount);

      var materialIndexBits = (ulong) (material?.Index ?? 0) & opaqueMaterialIndexMask;
      sortKey |= materialIndexBits << opaqueDepthBitCount;
    }

    this.SortKey = sortKey;
  }

  public void UpdateSortKey(ICamera camera, float nearPlane, float farPlane, float scale) {
    ulong sortKey = this.SortKey;

    var prms = this.Params;
    var transform = prms.Node.Transform;

    var isTransparent = prms.IsTransparent;
    sortKey |= (isTransparent ? (ulong) 1 : 0) << 63;

    var distance = (camera.Position - transform.LocalTranslation).Length() * scale;
    var distance0To1
        = ((distance - nearPlane) / (farPlane - nearPlane))
        .Clamp(0, 1);

    if (isTransparent) {
      var distance1To0 = 1 - distance0To1;
      var depthBits = (ulong) (distance1To0 * transparentDepthMax);

      sortKey = (sortKey & ~transparentDepthMax) | depthBits;
    } else {
      var depthBits = (ulong) (distance0To1 * opaqueDepthMax);
      sortKey = (sortKey & ~opaqueDepthMax) | depthBits;
    }

    this.SortKey = sortKey;
  }
}

public sealed class SceneStaticRenderGraph : IRenderable {
  private readonly ISceneInstance scene_;

  private readonly
      List<(IReadOnlySceneNodeInstance, SimpleModelRenderComponent)>
      modelRenderComponents_ = new();

  private RenderGraphElement[] elements_;

  private IReadOnlySceneNode? selectedNode_;
  private IReadOnlyMesh? selectedMesh_;
  private IReadOnlySet<IReadOnlyMaterial>? selectedMaterials_;

  public float Scale { get; set; }
  public float NearPlane { get; set; }
  public float FarPlane { get; set; }

  public SceneStaticRenderGraph(ISceneInstance scene) {
    this.scene_ = scene;

    SelectedNodeService.OnNodeSelected += selectedNode
        => this.selectedNode_ = selectedNode;
    SelectedMeshService.OnMeshSelected += selectedMesh
        => this.selectedMesh_ = selectedMesh;
    SelectedMaterialsService.OnMaterialsSelected += selectedMaterials
        => this.selectedMaterials_ = selectedMaterials;
  }

  ~SceneStaticRenderGraph() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() { }

  private void GenerateModelIfNull_() {
    if (this.elements_ != null) {
      return;
    }

    var elements = new LinkedList<RenderGraphElement>();
    foreach (var node in this.scene_.EnumerateAllNodes()) {
      foreach (var renderComponent in node.Definition.Components
                                          .OfType<
                                              ISceneNodeRenderComponent>()) {
        if (renderComponent is not SimpleModelRenderComponent
            modelRenderComponent) {
          elements.AddLast(
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

        foreach (var primitiveRenderer in modelRenderer.MaterialRenderers) {
          var transparencyType = primitiveRenderer.GlMaterialShader.Material?.GetTransparencyType() ??
                         TransparencyType.OPAQUE;
          elements.AddLast(
              new RenderGraphElement(
                  new RenderGraphMaterialRenderer {
                      Node = node,
                      GlMaterialShader = primitiveRenderer.GlMaterialShader,
                      IsTransparent
                          = transparencyType is TransparencyType.TRANSPARENT,
                      MinPrimitiveIndex = primitiveRenderer.MinPrimitiveIndex,
                      InversePriority = primitiveRenderer.InversePriority,
                      ModelRenderer = modelRenderer,
                      MaterialRenderer = primitiveRenderer,
                  }));
        }
      }
    }

    this.elements_ = elements.ToArray();
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
    foreach (var element in this.elements_) {
      element.UpdateSortKey(camera, this.NearPlane, this.FarPlane, this.Scale);
    }

    this.elements_.Sort(this.comparer_);

    var isSomethingSelected = this.selectedNode_ != null ||
                              this.selectedMesh_ != null ||
                              (this.selectedMaterials_?.Any() ?? false);

    if (isSomethingSelected) {
      foreach (var element in this.elements_) {
        var prms = element.Params;
        if (prms.IsSelected || this.selectedNode_ == prms.Node.Definition) {
          GlUtil.RenderOutline(() => RenderParams_(prms));
        }
      }
    }

    foreach (var element in this.elements_) {
      var prms = element.Params;
      RenderParams_(prms);
    }

    if (isSomethingSelected) {
      foreach (var element in this.elements_) {
        var prms = element.Params;
        if (prms.IsSelected || this.selectedNode_ == prms.Node.Definition) {
          GlUtil.RenderHighlight(() => RenderParams_(prms));
        }
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