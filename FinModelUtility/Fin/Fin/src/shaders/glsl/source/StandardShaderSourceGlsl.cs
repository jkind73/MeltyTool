using System.Text;

using fin.model;
using fin.util.enumerables;
using fin.util.strings;

namespace fin.shaders.glsl.source;

public sealed class StandardShaderSourceGlsl : IShaderSourceGlsl {
  public StandardShaderSourceGlsl(
      IReadOnlyModel model,
      IModelRequirements modelRequirements,
      IReadOnlyStandardMaterial material,
      IShaderRequirements shaderRequirements) {
    this.VertexShaderSource
        = GlslUtil.GetVertexSrc(model, modelRequirements, shaderRequirements);

    var animations = model.AnimationManager.Animations;

    var sb = new BracketStringBuilder();
    sb.AppendLine(
        $"#version {GlslConstants.FRAGMENT_SHADER_VERSION}");
    sb.AppendLine(GlslConstants.FLOAT_PRECISION);
    sb.AppendLine();

    var hasColor = shaderRequirements.UsedColors.AnyTrue();

    var diffuseTexture = material.DiffuseTexture;
    var hasDiffuseTexture = diffuseTexture != null;

    var normalTexture = material.NormalTexture;
    var hasNormalTexture = normalTexture != null;
    var hasNormals = shaderRequirements.HasNormals;
    var hasLighting = !material.IgnoreLights && hasNormals;
    var hasTangents = shaderRequirements.TangentType != TangentType.NOT_PRESENT;
    var hasBinormals = hasNormals && hasTangents;

    var ambientOcclusionTexture = material.AmbientOcclusionTexture;
    var hasAmbientOcclusionTexture = ambientOcclusionTexture != null;

    var emissiveTexture = material.EmissiveTexture;
    var hasEmissiveTexture = emissiveTexture != null;

    var specularTexture = material.SpecularTexture;
    var hasSpecularTexture = specularTexture != null;

    if (hasLighting) {
      sb.AppendLine(
          $"""
           {GlslUtil.LIGHT_HEADER}

           """);
    }

    sb.AppendTextureHeadersIfNeeded(
        material.Textures,
        animations);

    var needsNewline = false;
    if (hasDiffuseTexture) {
      sb.AppendLine(
          $"uniform {GlslUtil.GetTypeOfTexture(diffuseTexture, animations)} diffuseTexture;");
      needsNewline = true;
    }

    if (hasLighting) {
      if (hasNormalTexture) {
        sb.AppendLine(
            $"uniform {GlslUtil.GetTypeOfTexture(normalTexture, animations)} normalTexture;");
        needsNewline = true;
      }

      if (hasSpecularTexture) {
        sb.AppendLine(
            $"uniform {GlslUtil.GetTypeOfTexture(specularTexture, animations)} specularTexture;");
        needsNewline = true;
      }

      if (hasAmbientOcclusionTexture) {
        sb.AppendLine(
            $"uniform {GlslUtil.GetTypeOfTexture(ambientOcclusionTexture, animations)} ambientOcclusionTexture;");
        needsNewline = true;
      }
    }

    if (hasEmissiveTexture) {
      sb.AppendLine(
          $"uniform {GlslUtil.GetTypeOfTexture(emissiveTexture, animations)} emissiveTexture;");
      needsNewline = true;
    }

    if (hasLighting) {
      sb.AppendLine(
          $"uniform float {GlslConstants.UNIFORM_SHININESS_NAME};");
      needsNewline = true;
    }

    if (needsNewline) {
      sb.AppendLine();
    }

    sb.AppendLine(
        """
        out vec4 fragColor;

        """);

    var needsLineAboveMain = false;

    if (hasColor) {
      needsLineAboveMain = true;
      sb.AppendLine(
          $"in vec4 {GlslConstants.IN_VERTEX_COLOR_NAME}0;");
    }

    if (hasLighting) {
      needsLineAboveMain = true;
      sb.AppendLine(
          """
          in vec3 vertexPosition;
          in vec3 vertexNormal;
          """);

      if (hasTangents) {
        needsLineAboveMain = true;
        sb.AppendLine(
            """
            in vec3 tangent;
            """);
      }

      if (hasBinormals) {
        needsLineAboveMain = true;
        sb.AppendLine(
            """
            in vec3 binormal;
            """);
      }
    }

    var usedUvs = shaderRequirements.UsedUvs;
    for (var i = 0; i < usedUvs.Length; ++i) {
      if (usedUvs[i]) {
        needsLineAboveMain = true;
        sb.AppendLine($"in vec2 {GlslConstants.IN_UV_NAME}{i};");
      }
    }

    if (hasLighting) {
      if (needsLineAboveMain) {
        sb.AppendLine();
      }

      sb.AppendLine(
          $"""
           {GlslUtil.GetGetIndividualLightColorsFunction()}

           {GlslUtil.GetGetMergedLightColorsFunction()}

           {GlslUtil.GetApplyMergedLightColorsFunction(hasAmbientOcclusionTexture)}
           """);
    }

    if (needsLineAboveMain) {
      sb.AppendLine();
    }

    sb.AppendBlock(
        "void main()",
        () => {
          var getDiffuseTextureColor = GlslUtil.ReadColorFromTexture(
              "diffuseTexture",
              $"{GlslConstants.IN_UV_NAME}{diffuseTexture?.UvIndex ?? 0}",
              diffuseTexture,
              animations);
          sb.AppendLine(
              $"fragColor = {(hasDiffuseTexture, hasColor) switch {
                  (false, false) => "vec4(1)",
                  (false, true) => $"{GlslConstants.IN_VERTEX_COLOR_NAME}0",
                  (true, false) => getDiffuseTextureColor,
                  (true, true) => $"{getDiffuseTextureColor} * {GlslConstants.IN_VERTEX_COLOR_NAME}0"
              }};");

          if (hasLighting) {
            sb.AppendLine();
            if (hasAmbientOcclusionTexture) {
              sb.AppendLine(
                  $"vec4 ambientOcclusionColor = {GlslUtil.ReadColorFromTexture("ambientOcclusionTexture", $"{GlslConstants.IN_UV_NAME}{ambientOcclusionTexture?.UvIndex ?? 0}", ambientOcclusionTexture, animations)};");
            }

            if (!hasNormalTexture) {
              sb.AppendLine(
                  """
                  // Have to renormalize because the vertex normals can become distorted when interpolated.
                  vec3 fragNormal = normalize(vertexNormal);
                  """);
            } else {
              sb.AppendLine(
                  $"""
                   // Have to renormalize because the vertex normals can become distorted when interpolated.
                   vec3 fragNormal = normalize(vertexNormal);
                   vec3 textureNormal = {GlslUtil.ReadColorFromTexture("normalTexture", $"{GlslConstants.IN_UV_NAME}{normalTexture?.UvIndex ?? 0}", normalTexture, animations)}.xyz * 2.0 - 1.0;
                   fragNormal = normalize(mat3(tangent, binormal, fragNormal) * textureNormal);
                   """);
            }

            // TODO: Is this right?
            sb.AppendLine(
                $"fragColor.rgb = mix(fragColor.rgb, applyMergedLightingColors(vertexPosition, fragNormal, {GlslConstants.UNIFORM_SHININESS_NAME}, fragColor, {(hasSpecularTexture ? $"{GlslUtil.ReadColorFromTexture("specularTexture", "uv0", specularTexture, animations)}" : "vec4(1)")}{(hasAmbientOcclusionTexture ? ", ambientOcclusionColor.r" : "")}).rgb, {GlslConstants.UNIFORM_USE_LIGHTING_NAME});");
          }

          if (hasEmissiveTexture) {
            sb.AppendLine();
            sb.AppendLine(
                $"vec4 emissiveColor = {GlslUtil.ReadColorFromTexture("emissiveTexture", $"{GlslConstants.IN_UV_NAME}{emissiveTexture?.UvIndex ?? 0}", emissiveTexture, animations)};");
            sb.AppendLine(
                """
                fragColor.rgb += emissiveColor.rgb;
                fragColor.rgb = min(fragColor.rgb, 1.0);
                """);
          }

          GlslUtil.AppendAlphaDiscard(sb, material);
        });

    this.FragmentShaderSource = sb.ToString();
  }

  public string VertexShaderSource { get; }

  public string FragmentShaderSource { get; set; }
}