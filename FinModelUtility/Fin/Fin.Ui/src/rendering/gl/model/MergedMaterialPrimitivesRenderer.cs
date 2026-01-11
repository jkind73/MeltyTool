using fin.data.indexable;
using fin.math;
using fin.model;
using fin.shaders.glsl;
using fin.ui.rendering.gl.material;

namespace fin.ui.rendering.gl.model;

public partial class ModelRenderer {
  /// <summary>
  ///   Renderer for all primitives sharing a material in a single mesh.
  /// </summary>
  private sealed class MergedMaterialPrimitivesRenderer : IMaterialRenderer {
    private readonly IGlBufferRenderer bufferRenderer_;
    private bool isMaterialSelected_;
    private bool isMeshSelected_;

    public IReadOnlyMesh Mesh { get; }
    public IReadOnlyMaterial? Material { get; }
    public required int MinPrimitiveIndex { get; init; }
    public required uint InversePriority { get; init; }
    public IGlMaterialShader GlMaterialShader { get; }
    
    public IReadOnlyIndexableDictionary<IReadOnlyMesh, bool>? HiddenMeshes { get; set; }

    public MergedMaterialPrimitivesRenderer(
        IReadOnlyTextureTransformManager? textureTransformManager,
        IReadOnlyModel model,
        IModelRequirements modelRequirements,
        IReadOnlyMesh mesh,
        IReadOnlyMaterial? material,
        IGlBufferRenderer bufferRenderer) {
      this.Mesh = mesh;
      this.Material = material;

      this.GlMaterialShader = gl.material.GlMaterialShader.FromMaterial(model,
        modelRequirements,
        material,
        textureTransformManager);

      this.bufferRenderer_ = bufferRenderer;

      SelectedMaterialsService.OnMaterialsSelected
          += selectedMaterials =>
              this.isMaterialSelected_ = this.Material != null &&
                                 (selectedMaterials?.Contains(this.Material) ??
                                  false);
      SelectedMeshService.OnMeshSelected += selectedMesh
          => this.isMeshSelected_ = this.Mesh == selectedMesh;
    }

    ~MergedMaterialPrimitivesRenderer()
      => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      this.GlMaterialShader?.Dispose();
      this.bufferRenderer_?.Dispose();
    }

    public void Render() {
      if ((this.HiddenMeshes?.TryGetValue(this.Mesh, out var isHidden) ??
           false) && isHidden) {
        return;
      }

      var isSelected = this.isMaterialSelected_ || this.isMeshSelected_;

      if (isSelected) {
        GlUtil.RenderOutline(this.RenderImpl_);
      }

      this.RenderImpl_();

      if (isSelected) {
        GlUtil.RenderHighlight(this.RenderImpl_);
      }
    }

    private void RenderImpl_() {
      this.GlMaterialShader?.Use();

      if (this.Material != null) {
        GlUtil.SetBlendingSeparate(this.Material.ColorBlendEquation,
                                   this.Material.ColorSrcFactor,
                                   this.Material.ColorDstFactor,
                                   this.Material.AlphaBlendEquation,
                                   this.Material.AlphaSrcFactor,
                                   this.Material.AlphaDstFactor,
                                   this.Material.LogicOp);
      } else {
        GlUtil.ResetBlending();
      }

      GlUtil.SetCulling(this.Material?.CullingMode ?? CullingMode.SHOW_BOTH);
      GlUtil.SetDepth(
          this.Material?.DepthMode ?? DepthMode.READ_AND_WRITE,
          this.Material?.DepthCompareType ??
          DepthCompareType.LEqual);
      GlUtil.SetChannelUpdateMask(this.Material?.UpdateColorChannel ?? true,
                                  this.Material?.UpdateAlphaChannel ?? false);

      this.bufferRenderer_.Render();
    }
  }
}