using System.Linq;
using System.Text;

using fin.model;
using fin.util.enumerables;
using fin.util.strings;

namespace fin.shaders.glsl.source;

public sealed class TextureShaderSourceGlsl : IShaderSourceGlsl {
  public TextureShaderSourceGlsl(IReadOnlyModel model,
                                 IModelRequirements modelRequirements,
                                 IReadOnlyTextureMaterial material,
                                 IShaderRequirements shaderRequirements) {
    this.VertexShaderSource
        = GlslUtil.GetVertexSrc(model, modelRequirements, shaderRequirements);

    var animations = model.AnimationManager.Animations;

    var diffuseTexture = material.Textures.FirstOrDefault();
    var uvIndex = diffuseTexture?.UvIndex ?? 0;
    var hasColors = shaderRequirements.UsedColors.AnyTrue();
    var hasNormals = shaderRequirements.HasNormals;
    var hasLighting = !material.IgnoreLights && hasNormals;

    var sb = new BracketStringBuilder();
    sb.AppendLine($"#version {GlslConstants.FRAGMENT_SHADER_VERSION}");
    sb.AppendLine(GlslConstants.FLOAT_PRECISION);

    if (hasLighting) {
      sb.AppendLine(
          $"""

           {GlslUtil.LIGHT_HEADER}
           """);
    }

    sb.AppendTextureHeadersIfNeeded(material.Textures, animations);
    sb.AppendLine();

    if (material.DiffuseColor != null) {
      sb.AppendLine("uniform vec4 diffuseColor;");
    }

    sb.AppendLine(
        $"uniform {GlslUtil.GetTypeOfTexture(diffuseTexture, animations)} diffuseTexture;");

    if (hasLighting) {
      sb.AppendLine(
          $"uniform float {GlslConstants.UNIFORM_SHININESS_NAME};");
    }

    sb.AppendLine(
        """

        out vec4 fragColor;

        """);

    if (hasColors) {
      sb.AppendLine($"in vec4 {GlslConstants.IN_VERTEX_COLOR_NAME}0;");
    }

    if (hasLighting) {
      sb.AppendLine(
          """
          in vec3 vertexPosition;
          in vec3 vertexNormal;
          """);
    }

    sb.AppendLine($"in vec2 {GlslConstants.IN_UV_NAME}{uvIndex};");

    if (hasLighting) {
      sb.AppendLine(
          $"""

           {GlslUtil.GetGetIndividualLightColorsFunction()}

           {GlslUtil.GetGetMergedLightColorsFunction()}

           {GlslUtil.GetApplyMergedLightColorsFunction(false)}
           """
      );
    }

    sb.AppendLine();
    sb.AppendBlock(
        "void main()",
        () => {
          sb.AppendLine(
              $"fragColor = {GlslUtil.ReadColorFromTexture("diffuseTexture", $"uv{uvIndex}", diffuseTexture, animations)}" +
              (hasColors ? " * vertexColor0" : "") +
              (material.DiffuseColor != null ? " * diffuseColor" : "") +
              ";");

          if (hasLighting) {
            sb.AppendLine(
                $"""

                 // Have to renormalize because the vertex normals can become distorted when interpolated.
                 vec3 fragNormal = normalize(vertexNormal);
                 fragColor.rgb = mix(fragColor.rgb, applyMergedLightingColors(vertexPosition, fragNormal, {GlslConstants.UNIFORM_SHININESS_NAME}, fragColor, vec4(1)).rgb,  {GlslConstants.UNIFORM_USE_LIGHTING_NAME});
                 """);
          }

          GlslUtil.AppendAlphaDiscard(sb, material);
        });

    this.FragmentShaderSource = sb.ToString();
  }

  public string VertexShaderSource { get; }
  public string FragmentShaderSource { get; }
}