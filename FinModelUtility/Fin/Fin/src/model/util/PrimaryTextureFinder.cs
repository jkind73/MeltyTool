using System;
using System.Collections.Generic;
using System.Linq;

using fin.image.util;
using fin.language.equations.fixedFunction;

namespace fin.model.util;

public static class PrimaryTextureFinder {
  public static IReadOnlyTexture? GetFor(IReadOnlyMaterial material) {
    if (material is IReadOnlyNullMaterial
                    or IReadOnlyHiddenMaterial
                    or IReadOnlyColorMaterial) {
      return null;
    }

    if (material is IReadOnlyFixedFunctionMaterial fixedFunctionMaterial) {
      return GetFor(fixedFunctionMaterial);
    }

    if (material is IReadOnlyTextureMaterial textureMaterial) {
      return GetFor(textureMaterial);
    }

    if (material is IReadOnlyStandardMaterial standardMaterial) {
      return GetFor(standardMaterial);
    }

    throw new NotImplementedException();
  }

  public static IReadOnlyTexture? GetFor(IReadOnlyTextureMaterial material)
    => material.Texture;

  public static IReadOnlyTexture? GetFor(
      IReadOnlyFixedFunctionMaterial material) {
    var equations = material.Equations;

    var textures = material.Textures;

    // TODO: Use some kind of priority class

    var compiledTexture = material.CompiledTexture;
    if (compiledTexture != null) {
      return compiledTexture;
    }

    return textures.GetPrimaryByPriority();

    // TODO: Prioritize textures w/ color rather than intensity
    // TODO: Prioritize textures w/ standard texture sets
  }

  public static IEnumerable<IReadOnlyTexture> Prioritize(
      this IEnumerable<IReadOnlyTexture> textures)
    => textures
       .OrderByDescending(t => t.UvType == UvType.STANDARD)
       .ThenByDescending(t => t.ColorType == ColorType.COLOR)
       .ThenBy(t => t.UvIndex)
       .ThenByDescending(t => TransparencyTypeUtil
                                  .GetTransparencyType(t.Image) ==
                              TransparencyType.OPAQUE);

  public static IReadOnlyTexture? GetPrimaryByPriority(
      this IEnumerable<IReadOnlyTexture> textures)
    => textures.Prioritize().FirstOrDefault();

  public static IReadOnlyTexture? GetFor(IReadOnlyStandardMaterial material)
    => material.DiffuseTexture ?? material.AmbientOcclusionTexture;
}