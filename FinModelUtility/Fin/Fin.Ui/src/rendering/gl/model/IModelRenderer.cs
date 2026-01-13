using fin.model;
using fin.model.skin;
using fin.ui.rendering.gl.material;

namespace fin.ui.rendering.gl.model;

public interface IModelRenderer : IRenderable {
  IReadOnlyModel Model { get; }
  IReadOnlyMeshVisibilityDictionary? MeshVisibility { get; set; }

  int VaoId { get; }
  void UpdateMatricesUbo();
  void BindMatricesUbo();

  void GenerateModelIfNull();
  IEnumerable<IGlMaterialShader> GetMaterialShaders(IReadOnlyMaterial material);

  IEnumerable<IMeshRenderer> MeshRenderers { get; }
}

public interface IDynamicModelRenderer : IModelRenderer {
  void UpdateBuffer();
}

public interface IMeshRenderer : IRenderable {
  void GenerateModelIfNull();
  IEnumerable<IMeshRenderer> Children { get; }
  IEnumerable<IMaterialRenderer> MaterialRenderers { get; }
}

public interface IMaterialRenderer : IRenderable {
  int MinPrimitiveIndex { get; }
  uint InversePriority { get; }
  IGlMaterialShader GlMaterialShader { get; }
}