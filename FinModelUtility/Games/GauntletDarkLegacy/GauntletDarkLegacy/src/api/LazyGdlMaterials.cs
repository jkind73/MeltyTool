using System.Numerics;

using fin.data.lazy;
using fin.image;
using fin.io;
using fin.io.bundles;
using fin.language.equations.fixedFunction;
using fin.model;
using fin.model.impl;
using fin.util.enums;
using fin.util.hex;

using gdl.schema.anim;
using gdl.schema.objects;

using schema.binary;

namespace gdl.api;

using KeyTuple = (ushort textureIndex, short lmIndex, bool hasVertexColors, MbFlags);

public sealed class LazyGdlMaterials {
  private readonly LazyDictionary<KeyTuple, IReadOnlyMaterial> impl_;

  public IReadOnlyMaterial this[KeyTuple key] => this.impl_[key];

  public LazyGdlMaterials(
      IModel finModel,
      Dictionary<int, IReadOnlyImage> textureImageCache,
      IBinaryReader textureBr,
      Objects objects) {
    var lazyFinImages = new LazyDictionary<int, IReadOnlyImage>(textureIndex
          => {
        if (textureImageCache.TryGetValue(textureIndex,
                                          out var existingTextureImage)) {
          return existingTextureImage;
        }

        var gdlTexture = objects.Textures[textureIndex];
        textureBr.Position = gdlTexture.TextureDataPointer;
        return textureImageCache[textureIndex]
            = new GdlImageReader(gdlTexture).ReadImage(textureBr);
      });
    var lazyFinTextures = new LazyDictionary<(ushort textureIndex, bool isLightmap), IReadOnlyTexture>(tuple
          => {
        var (textureIndex, isLightmap) = tuple;

        var gdlTexture = objects.Textures[textureIndex];
        var finImage = lazyFinImages[textureIndex];

        var finTexture = finModel.MaterialManager.CreateTexture(finImage);
        var prefix = !isLightmap ? "texture" : "lightmap";
        finTexture.Name
            = $"{prefix}{textureIndex}_{gdlTexture.Format}_{gdlTexture.TextureDataPointer.ToHex()}";

        finTexture.WrapModeU = gdlTexture.Flags.CheckFlag(TextureFlags.CLAMP_U)
            ? WrapMode.CLAMP
            : WrapMode.REPEAT;
        finTexture.WrapModeV = gdlTexture.Flags.CheckFlag(TextureFlags.CLAMP_V)
            ? WrapMode.CLAMP
            : WrapMode.REPEAT;

        finTexture.UvIndex = !isLightmap ? 0 : 1;

        return finTexture;
      });

    this.impl_ = new LazyDictionary<KeyTuple, IReadOnlyMaterial>(tuple => {
          var (textureIndex, lmIndex, hasVertexColors, boneMbFlags) = tuple;

          var finMaterial = finModel.MaterialManager.AddFixedFunctionMaterial();
          var equations = finMaterial.Equations;

          var outputColorAlpha = finMaterial.GenerateDiffuse(
              (equations.ColorOps.One, equations.ScalarOps.One),
              lazyFinTextures[(textureIndex, false)],
              (hasVertexColors, false));

          var noShading = boneMbFlags.CheckFlag(MbFlags.NO_SHADING);
          if (!noShading) {
            if (objects.LmTexNum > 0) {
              if (lmIndex > 0) {
                var lightmapColor = finMaterial.AddTextureSourceColor(
                    lazyFinTextures[((ushort) lmIndex, true)]);

                outputColorAlpha = (outputColorAlpha.Item1.Multiply(lightmapColor),
                                    outputColorAlpha.Item2);
              } else {
                outputColorAlpha
                    = (equations.ColorOps.MultiplyWithConstant(
                           outputColorAlpha.Item1,
                           .25f), outputColorAlpha.Item2);
              }
            } else {
              outputColorAlpha
                  = equations.GenerateLighting(outputColorAlpha,
                                               equations.ColorOps.One);
            }
          }

          equations.SetOutputColorAlpha(outputColorAlpha);

          finMaterial.Shininess = MaterialConstants.DISABLED_SHININESS;

          finMaterial.DepthMode = (
              boneMbFlags.CheckFlag(MbFlags.NO_Z_TEST),
              boneMbFlags.CheckFlag(MbFlags.NO_Z_WRITE)) switch {
              (false, false) => DepthMode.READ_AND_WRITE,
              (false, true)  => DepthMode.READ_ONLY,
              (true, false)  => DepthMode.WRITE_ONLY,
              (true, true) => DepthMode.NONE,
          };

          var blendSrcAndDst = GetBlendMode_(boneMbFlags);
          if (blendSrcAndDst != null) {
            finMaterial.SetBlending(BlendEquation.ADD,
                                    blendSrcAndDst.Value.src,
                                    blendSrcAndDst.Value.dst,
                                    LogicOp.UNDEFINED);
          }

          return finMaterial;
        });
  }

  private static (BlendFactor src, BlendFactor dst)? GetBlendMode_(
      MbFlags boneFlags) {
    if (boneFlags.CheckFlag(MbFlags.BLEND_ADD)) {
      return (BlendFactor.SRC_ALPHA, BlendFactor.ONE);
    }

    if (boneFlags.CheckFlag(MbFlags.BLEND_MULTIPLY)) {
      return (BlendFactor.DST_COLOR, BlendFactor.ZERO);
    }

    return null;
  }
}