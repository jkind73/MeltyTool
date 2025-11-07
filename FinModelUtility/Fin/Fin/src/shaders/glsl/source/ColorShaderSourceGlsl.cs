using System.Text;

using fin.model;
using fin.util.enumerables;
using fin.util.strings;

namespace fin.shaders.glsl.source;

public sealed class ColorShaderSourceGlsl : IShaderSourceGlsl {
  public ColorShaderSourceGlsl(IReadOnlyModel model,
                               IModelRequirements modelRequirements,
                               IReadOnlyMaterial material,
                               IShaderRequirements shaderRequirements) {
    this.VertexShaderSource
        = GlslUtil.GetVertexSrc(model, modelRequirements, shaderRequirements);

    var hasColors = shaderRequirements.UsedColors.AnyTrue();
    var hasNormals = shaderRequirements.HasNormals;
    var hasLighting = !material.IgnoreLights && hasNormals;

    var sb = new BracketStringBuilder();
    sb.AppendLine($"#version {GlslConstants.FRAGMENT_SHADER_VERSION}");
    sb.AppendLine(GlslConstants.FLOAT_PRECISION);
    sb.AppendLine();

    if (hasLighting) {
      sb.AppendLine(
          $"""
           {GlslUtil.LIGHT_HEADER}

           """);
    }

    sb.AppendLine("uniform vec4 diffuseColor;");

    if (hasLighting) {
      sb.AppendLine(
          $"uniform float {GlslConstants.UNIFORM_SHININESS_NAME};");
    }

    sb.AppendLine(
        """

        out vec4 fragColor;

        """);

    var hadAnyIns = false;
    if (hasColors) {
      hadAnyIns = true;
      sb.AppendLine($"in vec4 {GlslConstants.IN_VERTEX_COLOR_NAME}0;");
    }

    if (hasLighting) {
      hadAnyIns = true;
      sb.AppendLine(
          """
          in vec3 vertexPosition;
          in vec3 vertexNormal;
          """);
      sb.AppendLine(
          $"""

           {GlslUtil.GetGetIndividualLightColorsFunction()}

           {GlslUtil.GetGetMergedLightColorsFunction()}

           {GlslUtil.GetApplyMergedLightColorsFunction(false)}
           """
      );
    }

    if (hadAnyIns) {
      sb.AppendLine();
    }

    sb.AppendBlock(
        "void main()",
        () => {
          sb.AppendLine(
              $"fragColor = diffuseColor{hasColors switch {
                  false => "",
                  true  => $" * {GlslConstants.IN_VERTEX_COLOR_NAME}0",
              }};");

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