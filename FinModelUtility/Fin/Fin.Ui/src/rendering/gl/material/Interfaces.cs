using fin.model;

using readOnly;


namespace fin.ui.rendering.gl.material;

[GenerateReadOnly]
public partial interface IGlMaterialShader : IDisposable {
  IReadOnlyShaderProgram ShaderProgram { get; }
  IReadOnlyMaterial? Material { get; }

  void Use();
}