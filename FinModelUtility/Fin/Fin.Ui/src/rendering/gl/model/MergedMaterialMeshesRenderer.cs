using fin.data.indexable;
using fin.image.util;
using fin.math;
using fin.model;
using fin.model.util;
using fin.shaders.glsl;
using fin.ui.rendering.gl.material;


namespace fin.ui.rendering.gl.model;

public partial class ModelRenderer {
  /// <summary>
  ///   Model renderer that merges primitives by material.
  /// </summary>
  private sealed class MergedMaterialMeshesRenderer : IDynamicModelRenderer {
    private readonly bool dynamic_;
    private readonly IReadOnlyTextureTransformManager? textureTransformManager_;

    private IGlBufferManager? bufferManager_;
    private IDynamicGlBufferManager? dynamicBufferManager_;

    private MergedMaterialMeshRenderer[] materialMeshRenderers_ = [];

    public MergedMaterialMeshesRenderer(
        IReadOnlyModel model,
        IReadOnlyTextureTransformManager? textureTransformManager = null,
        bool dynamic = false) {
      this.Model = model;
      this.textureTransformManager_ = textureTransformManager;
      this.dynamic_ = dynamic;
    }

    // Generates buffer manager and model within the current GL context.
    public void GenerateModelIfNull() {
      if (this.bufferManager_ != null) {
        return;
      }

      var modelRequirements = ModelRequirements.FromModel(this.Model);

      if (!this.dynamic_) {
        this.bufferManager_
            = GlBufferManager.CreateStatic(this.Model, modelRequirements);
      } else {
        this.bufferManager_ = this.dynamicBufferManager_
            = GlBufferManager.CreateDynamic(this.Model, modelRequirements);
      }

      var allMaterialMeshRenderers = new List<MergedMaterialMeshRenderer>();

      // TODO: Optimize this with something like a "MinMap"?
      var meshQueue = new RenderPriorityOrderedSet<IReadOnlyMesh>();
      foreach (var mesh in this.Model.Skin.Meshes) {
        if (mesh.IsSubMesh) {
          continue;
        }

        meshQueue.Add(mesh, int.MaxValue, UInt32.MaxValue, false);
        foreach (var primitive in mesh.Primitives) {
          meshQueue.Add(mesh,
                        primitive.Index,
                        primitive.InversePriority,
                        (primitive.Material?.GetTransparencyType() ??
                         TransparencyType.OPAQUE) ==
                        TransparencyType.TRANSPARENT);
        }
      }

      foreach (var mesh in meshQueue) {
        var materialMeshRenderer = new MergedMaterialMeshRenderer(
                                         this.Model,
                                         mesh.item,
                                         modelRequirements,
                                         this.bufferManager_,
                                         this.textureTransformManager_) {
            HiddenMeshes = this.HiddenMeshes,
        };
        materialMeshRenderer.GenerateModelIfNull();
        allMaterialMeshRenderers.Add(materialMeshRenderer);
      }

      this.materialMeshRenderers_ = allMaterialMeshRenderers.ToArray();
    }

    ~MergedMaterialMeshesRenderer() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      foreach (var meshRenderer in this.materialMeshRenderers_) {
        meshRenderer.Dispose();
      }

      this.bufferManager_?.Dispose();
    }

    public IReadOnlyModel Model { get; }

    public IReadOnlyIndexableDictionary<IReadOnlyMesh, bool>? HiddenMeshes {
      get;
      set;
    }

    public IEnumerable<IMeshRenderer> MeshRenderers => this.materialMeshRenderers_;

    public void UpdateBuffer() => this.dynamicBufferManager_?.UpdateBuffer();

    public void UpdateMatricesUbo() { }
    public void BindMatricesUbo() { }

    public void Render() {
      this.GenerateModelIfNull();

      foreach (var meshRenderer in this.materialMeshRenderers_) {
        meshRenderer.Render();
      }
    }

    public IEnumerable<IGlMaterialShader> GetMaterialShaders(
        IReadOnlyMaterial material)
      => this.materialMeshRenderers_.SelectMany(p => p.GetMaterialShaders(
                                                    material));
  }
}