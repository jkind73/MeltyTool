using System;
using System.Drawing;

using fin.color;

using SharpGLTF.Materials;

namespace fin.model.io.exporters.gltf;

public static class GltfMaterialUtil {
  public static MaterialBuilder WithMetallicRoughnessIfLit(
      this MaterialBuilder gltfMaterialBuilder,
      IReadOnlyMaterial finMaterial,
      bool usesDiffuseLighting = true,
      bool usesSpecularLighting = true) {
    if (finMaterial.IgnoreLights || (!usesDiffuseLighting && !usesSpecularLighting)) {
      return gltfMaterialBuilder.WithUnlitShader();
    }

    // Shamelessly stolen from:
    // https://computergraphics.stackexchange.com/questions/1515/what-is-the-accepted-method-of-converting-shininess-to-roughness-and-vice-versa
    var roughness = !usesSpecularLighting
        ? 1
        : MathF.Sqrt(2 / (finMaterial.Shininess + 2));

    return gltfMaterialBuilder.WithMetallicRoughnessShader()
                              .WithMetallicRoughness(null, roughness);
  }

  public static MaterialBuilder WithBaseColor(
      this MaterialBuilder gltfMaterialBuilder,
      Color color)
    => gltfMaterialBuilder.WithBaseColor(color.AsVector4());
}