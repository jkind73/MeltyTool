using fin.data.dictionaries;
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
  ///   Mesh renderer that merges primitives by material.
  /// </summary>
  private sealed class MergedMaterialMeshRenderer(
      IReadOnlyModel model,
      IReadOnlyMesh mesh,
      IModelRequirements modelRequirements,
      IGlBufferManager bufferManager,
      IReadOnlyTextureTransformManager? textureTransformManager = null)
      : IMeshRenderer {
    private MergedMaterialPrimitivesRenderer[]? materialMeshRenderers_;

    private MergedMaterialMeshRenderer[] children_
        = mesh.SubMeshes.Select(subMesh => new MergedMaterialMeshRenderer(
                                    model,
                                    subMesh,
                                    modelRequirements,
                                    bufferManager,
                                    textureTransformManager))
              .ToArray();

    public IReadOnlyIndexableDictionary<IReadOnlyMesh, bool>? HiddenMeshes {
      get;
      set {
        field = value;
        foreach (var child in this.children_) {
          child.HiddenMeshes = value;
        }
      }
    }

    public IEnumerable<IMeshRenderer> Children => this.children_;
    public IEnumerable<IMaterialRenderer> MaterialRenderers => this.materialMeshRenderers_!;

    // Generates buffer manager and model within the current GL context.
    public void GenerateModelIfNull() {
      if (this.materialMeshRenderers_ != null) {
        return;
      }

      var primitiveMerger = new PrimitiveMerger();
      var materialQueue = new RenderPriorityOrderedSet<IReadOnlyMaterial?>();
      var primitivesByMaterial
          = new ListDictionary<IReadOnlyMaterial?, IReadOnlyPrimitive>(
              new NullFriendlyDictionary<IReadOnlyMaterial?,
                  IList<IReadOnlyPrimitive>>());
      foreach (var primitive in mesh.Primitives) {
        primitivesByMaterial.Add(primitive.Material, primitive);
        materialQueue.Add(
            primitive.Material,
            primitive.Index,
            primitive.InversePriority,
            (primitive.Material?.GetTransparencyType() ??
             TransparencyType.OPAQUE) ==
            TransparencyType.TRANSPARENT);
      }

      var mergedPrimitives = new List<MergedPrimitive>();
      var materialTuples
          = new List<(int minPrimitiveIndex, uint inversePriority, IReadOnlyMaterial? material)>();
      foreach (var (minPrimitiveIndex, inversePriority, material) in materialQueue) {
        var primitives = primitivesByMaterial[material];
        if (!primitiveMerger.TryToMergePrimitives(
                primitives,
                out var mergedPrimitive)) {
          continue;
        }

        mergedPrimitives.Add(mergedPrimitive);
        materialTuples.Add((minPrimitiveIndex, inversePriority, material));
      }

      this.materialMeshRenderers_
          = bufferManager
            .CreateRenderers(mergedPrimitives)
            .Select((renderer, i) => {
              var (minPrimitiveIndex, inversePriority, material) = materialTuples[i];
              return new MergedMaterialPrimitivesRenderer(
                  textureTransformManager,
                  model,
                  modelRequirements,
                  mesh,
                  material,
                  renderer) {
                  MinPrimitiveIndex = minPrimitiveIndex,
                  InversePriority = inversePriority,
                  HiddenMeshes = this.HiddenMeshes
              };
            })
            .ToArray();
    }

    ~MergedMaterialMeshRenderer() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      foreach (var materialMeshRenderer in this.materialMeshRenderers_ ?? []) {
        materialMeshRenderer.Dispose();
      }
    }

    public void Render() {
      this.GenerateModelIfNull();

      foreach (var materialMeshRenderer in this.materialMeshRenderers_!) {
        materialMeshRenderer.Render();
      }

      foreach (var child in this.children_) {
        child.Render();
      }
    }

    public IEnumerable<IGlMaterialShader> GetMaterialShaders(
        IReadOnlyMaterial material)
      => this.materialMeshRenderers_?
             .Where(r => r.Material == material)
             .Select(r => r.GlMaterialShader) ??
         [];
  }
}