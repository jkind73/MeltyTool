namespace fin.shaders.glsl.source;

public sealed class HiddenShaderSourceGlsl : IShaderSourceGlsl {
  public string VertexShaderSource

    => $$"""
         #version {{GlslConstants.FragmentShaderVersion}}

         void main() {}
         """;

  public string FragmentShaderSource
    => $$"""
         #version {{GlslConstants.FragmentShaderVersion}}
         {{GlslConstants.FLOAT_PRECISION}}
         
         void main() {
           discard;
         }
         """;
}