using fin.math;
using fin.model;
using fin.shaders.glsl;
using fin.util.asserts;


namespace fin.ui.rendering.gl.material;

public static class GlMaterialShader {
  public static IGlMaterialShader FromMaterial(
      IReadOnlyModel model,
      IModelRequirements modelRequirements,
      IReadOnlyMaterial? material,
      IReadOnlyTextureTransformManager textureTransformManager,
      IReadOnlyTextureFlipbookSwapManager textureFlipbookSwapManager)
    => material.GetShaderType() switch {
        FinShaderType.FIXED_FUNCTION => new GlFixedFunctionMaterialShader(
            model,
            modelRequirements,
            Asserts.AsA<IReadOnlyFixedFunctionMaterial>(material),
            textureTransformManager,
            textureFlipbookSwapManager),
        FinShaderType.TEXTURE => new GlTextureMaterialShader(model,
          modelRequirements,
          Asserts.AsA<IReadOnlyTextureMaterial>(material),
          textureTransformManager,
          textureFlipbookSwapManager),
        FinShaderType.COLOR => new GlColorMaterialShader(model,
          modelRequirements,
          Asserts.AsA<IReadOnlyColorMaterial>(material),
          textureTransformManager,
          textureFlipbookSwapManager),
        FinShaderType.SHADER => new GlShaderMaterialShader(model,
          modelRequirements,
          Asserts.AsA<IReadOnlyShaderMaterial>(material),
          textureTransformManager,
          textureFlipbookSwapManager),
        FinShaderType.STANDARD => new GlStandardMaterialShader(model,
          modelRequirements,
          Asserts.AsA<IReadOnlyStandardMaterial>(material),
          textureTransformManager,
          textureFlipbookSwapManager),
        FinShaderType.HIDDEN => new GlHiddenMaterialShader(
            model,
            modelRequirements,
            textureTransformManager,
            textureFlipbookSwapManager),
        FinShaderType.NULL
            => new GlNullMaterialShader(
                model,
                modelRequirements,
                textureTransformManager,
                textureFlipbookSwapManager),
        _ => throw new ArgumentOutOfRangeException()
    };
}