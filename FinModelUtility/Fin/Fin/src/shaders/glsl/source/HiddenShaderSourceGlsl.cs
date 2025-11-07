using fin.util.strings;

namespace fin.shaders.glsl.source;

public sealed class HiddenShaderSourceGlsl : IShaderSourceGlsl {
  public string VertexShaderSource

    => $$"""
         #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}

         void main() {}
         """;

  public string FragmentShaderSource
    => $$"""
         #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
         {{GlslConstants.FLOAT_PRECISION}}
         
         void main() {
           discard;
         }
         """;

  public void AppendFragmentMain(BracketStringBuilder sb) {
    sb.AppendLine("discard;");
  }
}