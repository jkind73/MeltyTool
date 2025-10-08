using System.Text;

using fin.model;
using fin.util.enumerables;

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

    var fragmentSrc = new StringBuilder();
    fragmentSrc.AppendLine($"#version {GlslConstants.FRAGMENT_SHADER_VERSION}");
    fragmentSrc.AppendLine(GlslConstants.FLOAT_PRECISION);
    fragmentSrc.AppendLine();

    if (hasLighting) {
      fragmentSrc.AppendLine(
          $"""
           {GlslUtil.LIGHT_HEADER}

           """);
    }

    fragmentSrc.AppendLine("uniform vec4 diffuseColor;");

    if (hasLighting) {
      fragmentSrc.AppendLine(
          $"uniform float {GlslConstants.UNIFORM_SHININESS_NAME};");
    }

    fragmentSrc.AppendLine(
        """

        out vec4 fragColor;

        """);

    var hadAnyIns = false;
    if (hasColors) {
      hadAnyIns = true;
      fragmentSrc.AppendLine($"in vec4 {GlslConstants.IN_VERTEX_COLOR_NAME}0;");
    }

    if (hasLighting) {
      hadAnyIns = true;
      fragmentSrc.AppendLine(
          """
          in vec3 vertexPosition;
          in vec3 vertexNormal;
          """);
      fragmentSrc.AppendLine(
          $"""

           {GlslUtil.GetGetIndividualLightColorsFunction()}

           {GlslUtil.GetGetMergedLightColorsFunction()}

           {GlslUtil.GetApplyMergedLightColorsFunction(false)}
           """
      );
    }

    if (hadAnyIns) {
      fragmentSrc.AppendLine();
    }

    fragmentSrc.AppendLine("void main() {");
    fragmentSrc.AppendLine(
        $"  fragColor = diffuseColor{hasColors switch {
            false => "",
            true  => $" * {GlslConstants.IN_VERTEX_COLOR_NAME}0",
        }};");


    if (hasLighting) {
      fragmentSrc.AppendLine(
          $"""
           
             // Have to renormalize because the vertex normals can become distorted when interpolated.
             vec3 fragNormal = normalize(vertexNormal);
             fragColor.rgb = mix(fragColor.rgb, applyMergedLightingColors(vertexPosition, fragNormal, {GlslConstants.UNIFORM_SHININESS_NAME}, fragColor, vec4(1)).rgb,  {GlslConstants.UNIFORM_USE_LIGHTING_NAME});
           """);
    }

    GlslUtil.AppendAlphaDiscard(fragmentSrc, material);
    fragmentSrc.Append('}');

    this.FragmentShaderSource = fragmentSrc.ToString();
  }

  public string VertexShaderSource { get; }
  public string FragmentShaderSource { get; }
}