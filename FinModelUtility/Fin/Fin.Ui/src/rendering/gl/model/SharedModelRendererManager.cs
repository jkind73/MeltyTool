using fin.image.util;
using fin.math;
using fin.math.transform;
using fin.model;
using fin.model.skeleton;
using fin.util.asserts;
using fin.util.types;


namespace fin.ui.rendering.gl.model;

[IocCandiate]
public partial class ModelRenderer {
  private class TransformTuple {
    public float Distance { get; set; }
    public required IReadOnlyTransform3d Transform { get; init; }
  }

  private class MeshRendererTuple {
    public required TransformTuple TransformTuple { get; init; }
    public required IModelRenderer ModelRenderer { get; init; }
    public required PrimitivesWithCommonMaterialRenderer MaterialRenderer { get; init; }
  }

  private static readonly List<TransformTuple> transformTuples_ = new();
  private static readonly List<ModelRenderer> modelRenderers_ = new();

  private static readonly List<(TransformTuple, ModelRenderer)> modelRendererTuples_ = new();
  private static readonly List<MeshRendererTuple> materialRendererTuples_ = new();

  public static void ResetRenderers() {
    modelRendererTuples_.Clear();

    foreach (var modelRenderer in modelRenderers_) {
      modelRenderer.Dispose();
    }

    transformTuples_.Clear();
    modelRenderers_.Clear();
    materialRendererTuples_.Clear();
  }

  public static IModelRenderer RequestRenderer(
      IReadOnlyTransform3d transform,
      IReadOnlyModel model,
      IReadOnlyLighting lighting,
      IReadOnlyBoneTransformManager boneTransformManager,
      IReadOnlyTextureTransformManager textureTransformManager) {
    var modelRenderer = new ModelRenderer(model,
                                          lighting,
                                          boneTransformManager,
                                          textureTransformManager);
    modelRenderers_.Add(modelRenderer);

    var transformTuple = new TransformTuple { Transform = transform };
    transformTuples_.Add(transformTuple);

    modelRendererTuples_.Add((transformTuple, modelRenderer));

    return modelRenderer;
  }

  public static void RenderAllShared() {
    if (modelRendererTuples_.Count > 0) {
      foreach (var (transformTuple, modelRenderer) in modelRendererTuples_) {
        var impl = modelRenderer.impl_.AssertAsA<MergedMaterialMeshesRenderer>();

        foreach (var meshRenderer in impl.MeshRenderers) {
          meshRenderer.GenerateModelIfNull();

          foreach (var materialRenderer in meshRenderer.MaterialRenderers) {
            materialRendererTuples_.Add(new MeshRendererTuple {
                ModelRenderer = modelRenderer,
                TransformTuple = transformTuple,
                MaterialRenderer = materialRenderer,
            });
          }
        }
      }

      modelRendererTuples_.Clear();
    }

    var cameraPosition = Camera.Instance.Position;
    foreach (var tuple in transformTuples_) {
      tuple.Distance = (cameraPosition - tuple.Transform.Translation).Length();
    }

    materialRendererTuples_.Sort((lhs, rhs) => {
      var lhsMaterial = lhs.MaterialRenderer.Material;
      var rhsMaterial = rhs.MaterialRenderer.Material;

      var lhsTransparencyType
          = lhsMaterial?.TransparencyType ?? TransparencyType.OPAQUE;
      var rhsTransparencyType
          = rhsMaterial?.TransparencyType ?? TransparencyType.OPAQUE;

      if (lhsTransparencyType != rhsTransparencyType) {
        return lhsTransparencyType.CompareTo(rhsTransparencyType);
      }

      if (lhsMaterial != rhsMaterial) {
        return (lhsMaterial?.GetHashCode() ?? 0).CompareTo(rhsMaterial?.GetHashCode() ?? 0);
      }

      return lhs.TransformTuple.Distance.CompareTo(rhs.TransformTuple.Distance);
    });

    foreach (var tuple in materialRendererTuples_) {
      var materialRenderer = tuple.MaterialRenderer;
      if (tuple.ModelRenderer.HiddenMeshes?.Contains(materialRenderer.Mesh) ??
          false) {
        continue;
      }

      GlTransform.PushMatrix();
      GlTransform.MultMatrix(tuple.TransformTuple.Transform.WorldMatrix);

      materialRenderer.Render();
      GlTransform.PopMatrix();
    }
  }
}