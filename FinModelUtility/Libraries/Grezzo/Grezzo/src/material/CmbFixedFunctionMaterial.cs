using System;
using System.Linq;

using fin.data.lazy;
using fin.image;
using fin.image.formats;
using fin.model;
using fin.image.util;

using grezzo.schema.cmb;

using SixLabors.ImageSharp.PixelFormats;

using BlendEquation = grezzo.schema.cmb.BlendEquation;
using BlendFactor = grezzo.schema.cmb.BlendFactor;
using FinBlendEquation = fin.model.BlendEquation;
using FinBlendFactor = fin.model.BlendFactor;
using FinTextureMinFilter = fin.model.TextureMinFilter;
using FinTextureMagFilter = fin.model.TextureMagFilter;
using TextureMagFilter = grezzo.schema.cmb.TextureMagFilter;
using TextureMinFilter = grezzo.schema.cmb.TextureMinFilter;

namespace grezzo.material;

public sealed class CmbFixedFunctionMaterial {
  private const bool USE_FIXED_FUNCTION_ = true;
  private const bool USE_JANKY_TRANSPARENCY_ = false;

  public CmbFixedFunctionMaterial(
      IModel finModel,
      Cmb cmb,
      int materialIndex,
      bool hasVertexColors,
      ILazyArray<IImage> textureImages) {
      var mats = cmb.Mats.Data;
      var cmbMaterials = mats.Materials;
      var cmbMaterial = cmbMaterials[materialIndex];

      // Get associated texture
      var finTextures =
          cmbMaterial
              .texMappers.Select((texMapper, i) => {
                var textureId = texMapper.TextureId;

                ITexture? finTexture = null;
                if (textureId != -1) {
                  var cmbTexture = cmb.tex.Data.Textures[textureId];

                  var rawTextureImage = textureImages[textureId];

                  // TODO: Is this logic possibly right????
                  IImage textureImage;
                  if (TransparencyTypeUtil.GetTransparencyType(rawTextureImage) !=
                      TransparencyType.OPAQUE || !USE_JANKY_TRANSPARENCY_) {
                    textureImage = rawTextureImage;
                  } else {
                    var backgroundColor = texMapper.BorderColor;

                    var processedImage = new Rgba32Image(
                        rawTextureImage.PixelFormat,
                        rawTextureImage.Width,
                        rawTextureImage.Height);
                    textureImage = processedImage;

                    rawTextureImage.Access(srcGetHandler => {
                      using var dstLock = processedImage.Lock();
                      var dstPtr = dstLock.Pixels;
                      for (var y = 0; y < rawTextureImage.Height; y++) {
                        for (var x = 0; x < rawTextureImage.Width; x++) {
                          srcGetHandler(x,
                                        y,
                                        out var r,
                                        out var g,
                                        out var b,
                                        out var a);

                          if (r == backgroundColor.Rb &&
                              g == backgroundColor.Gb &&
                              b == backgroundColor.Bb) {
                            a = 0;
                          }

                          dstPtr[y * rawTextureImage.Width + x] =
                              new Rgba32(r, g, b, a);
                        }
                      }
                    });
                  }

                  var cmbTexCoord = cmbMaterial.texCoords[i];

                  finTexture =
                      finModel.MaterialManager.CreateTexture(textureImage);
                  finTexture.Name = cmbTexture.Name;
                  finTexture.WrapModeU = this.CmbToFinWrapMode(texMapper.wrapS);
                  finTexture.WrapModeV = this.CmbToFinWrapMode(texMapper.wrapT);
                  finTexture.MinFilter =
                      texMapper.minFilter switch {
                          TextureMinFilter.NEAREST =>
                              FinTextureMinFilter.NEAR,
                          TextureMinFilter.LINEAR =>
                              FinTextureMinFilter.LINEAR,
                          TextureMinFilter
                                  .NEAREST_MIPMAP_NEAREST =>
                              FinTextureMinFilter
                                  .NEAR_MIPMAP_NEAR,
                          TextureMinFilter
                                  .LINEAR_MIPMAP_NEAREST =>
                              FinTextureMinFilter
                                  .LINEAR_MIPMAP_NEAR,
                          TextureMinFilter
                                  .NEAREST_MIPMAP_LINEAR =>
                              FinTextureMinFilter
                                  .NEAR_MIPMAP_LINEAR,
                          TextureMinFilter
                                  .LINEAR_MIPMAP_LINEAR =>
                              FinTextureMinFilter
                                  .LINEAR_MIPMAP_LINEAR,
                          _ => throw new ArgumentOutOfRangeException()
                      };
                  finTexture.MagFilter =
                      texMapper.magFilter switch {
                          TextureMagFilter.NEAREST =>
                              FinTextureMagFilter.NEAR,
                          TextureMagFilter.LINEAR =>
                              FinTextureMagFilter.LINEAR,
                          _ => throw new ArgumentOutOfRangeException()
                      };
                  finTexture.LodBias = texMapper.lodBias;
                  finTexture.MinLod = texMapper.minLodBias;
                  finTexture.UvIndex = cmbTexCoord.CoordinateIndex;
                  finTexture.BorderColor = texMapper.BorderColor;

                  finTexture.UvType =
                      cmbTexCoord.MappingMethod ==
                      TextureMappingType.UV_COORDINATE_MAP
                          ? UvType.STANDARD
                          : UvType.SPHERICAL;

                  // TODO: Better way to specify this??
                  if (cmbTexCoord.Scale.Y == 2 &&
                      texMapper.wrapT == TextureWrapMode.MIRROR) {
                    cmbTexCoord.Translation.Y -= 1;
                  }

                  finTexture.TextureTransform
                            .SetTranslation2d(cmbTexCoord.Translation.X,
                                        cmbTexCoord.Translation.Y)
                            .SetRotationRadians2d(cmbTexCoord.Rotation)
                            .SetScale2d(cmbTexCoord.Scale.X,
                                        cmbTexCoord.Scale.Y);

                  // TODO: Use LUTs/Distribution in specular calculation
                }

                return finTexture;
              })
              .ToArray();

      // Create material
      if (!USE_FIXED_FUNCTION_) {
        // TODO: Remove this hack
        var firstTexture = finTextures.FirstOrDefault();
        var firstColorFinTexture = finTextures.FirstOrDefault(tex => {
          var image = tex?.Image;
          if (image == null) {
            return false;
          }

          var isAllBlank = true;

          image.Access(getHandler => {
            for (var y = 0; y < image.Height; ++y) {
              for (var x = 0; x < image.Width; ++x) {
                getHandler(x, y, out var r, out var g, out var b, out var a);
                if (!(a == 0 || (r == 255 && g == 255 && b == 255))) {
                  isAllBlank = false;
                  return;
                }
              }
            }
          });

          return !isAllBlank;
        });


        var bestTexture = firstColorFinTexture ?? firstTexture;
        var finMaterial = bestTexture != null
            ? (IMaterial) finModel.MaterialManager.AddTextureMaterial(
                bestTexture)
            : finModel.MaterialManager.AddNullMaterial();
        this.Material = finMaterial;
      } else {
        var finMaterial = finModel.MaterialManager.AddFixedFunctionMaterial();
        this.Material = finMaterial;

        for (var i = 0; i < finTextures.Length; ++i) {
          var finTexture = finTextures[i];
          if (finTexture != null) {
            finMaterial.SetTextureSource(i, finTexture);
          }
        }

        var combinerGenerator =
            new CmbCombinerGenerator(cmbMaterial, hasVertexColors, finMaterial);

        var combiners = mats.Combiners;
        var texEnvStages =
            cmbMaterial.texEnvStagesIndices.Select(
                           i => {
                             if (i == -1) {
                               return null;
                             }

                             if (i < 0 || i >= combiners.Length) {
                               ;
                             }

                             return mats.Combiners[i];
                           })
                       .ToArray();

        combinerGenerator.AddCombiners(texEnvStages);

        if (!cmbMaterial.alphaTestEnabled) {
          finMaterial.SetAlphaCompare(AlphaOp.OR,
                                      AlphaCompareType.ALWAYS,
                                      0,
                                      AlphaCompareType.ALWAYS,
                                      0);
        } else {
          finMaterial.SetAlphaCompare(
              cmbMaterial.alphaTestFunction switch {
                  TestFunc.ALWAYS   => AlphaCompareType.ALWAYS,
                  TestFunc.EQUAL    => AlphaCompareType.EQUAL,
                  TestFunc.GEQUAL   => AlphaCompareType.G_EQUAL,
                  TestFunc.GREATER  => AlphaCompareType.GREATER,
                  TestFunc.NEVER    => AlphaCompareType.NEVER,
                  TestFunc.LESS     => AlphaCompareType.LESS,
                  TestFunc.LEQUAL   => AlphaCompareType.L_EQUAL,
                  TestFunc.NOTEQUAL => AlphaCompareType.N_EQUAL,
                  _                 => throw new ArgumentOutOfRangeException()
              },
              cmbMaterial.alphaTestReferenceValue);
        }

        finMaterial.UpdateAlphaChannel = false;

        // TODO: Fix these
        switch (cmbMaterial.blendMode) {
          case BlendMode.BLEND_NONE: {
            finMaterial.SetBlending(FinBlendEquation.ADD,
                                    FinBlendFactor.ONE,
                                    FinBlendFactor.ZERO,
                                    LogicOp.UNDEFINED);
            break;
          }
          case BlendMode.BLEND: {
            finMaterial.SetBlending(
                this.CmbBlendEquationToFin(cmbMaterial.colorEquation),
                this.CmbBlendFactorToFin(cmbMaterial.colorSrcFunc),
                this.CmbBlendFactorToFin(cmbMaterial.colorDstFunc),
                LogicOp.UNDEFINED);
            break;
          }
          case BlendMode.LOGICAL_OP:
          case BlendMode.BLEND_SEPARATE: {
            finMaterial.SetBlendingSeparate(
                this.CmbBlendEquationToFin(cmbMaterial.colorEquation),
                this.CmbBlendFactorToFin(cmbMaterial.colorSrcFunc),
                this.CmbBlendFactorToFin(cmbMaterial.colorDstFunc),
                this.CmbBlendEquationToFin(cmbMaterial.alphaEquation),
                this.CmbBlendFactorToFin(cmbMaterial.alphaSrcFunc),
                this.CmbBlendFactorToFin(cmbMaterial.alphaDstFunc),
                LogicOp.UNDEFINED);
            break;
          }
          default:                      throw new ArgumentOutOfRangeException();
        }

        finMaterial.DepthCompareType = cmbMaterial.depthTestFunction switch {
            TestFunc.NEVER    => DepthCompareType.NEVER,
            TestFunc.LESS     => DepthCompareType.LESS,
            TestFunc.EQUAL    => DepthCompareType.EQUAL,
            TestFunc.LEQUAL   => DepthCompareType.L_EQUAL,
            TestFunc.GREATER  => DepthCompareType.GREATER,
            TestFunc.NOTEQUAL => DepthCompareType.N_EQUAL,
            TestFunc.GEQUAL   => DepthCompareType.G_EQUAL,
            TestFunc.ALWAYS   => DepthCompareType.ALWAYS,
        };

        finMaterial.DepthMode = DepthMode.NONE;
        if (cmbMaterial.depthTestEnabled) {
          finMaterial.DepthMode |= DepthMode.READ;
        }

        if (cmbMaterial.depthWriteEnabled) {
          finMaterial.DepthMode |= DepthMode.WRITE;
        }
      }

      this.Material.Name = $"material{materialIndex}";
      this.Material.CullingMode = cmbMaterial.faceCulling switch {
          CullMode.FRONT_AND_BACK => CullingMode.SHOW_NEITHER,
          CullMode.FRONT        => CullingMode.SHOW_BACK_ONLY,
          CullMode.BACK         => CullingMode.SHOW_FRONT_ONLY,
          CullMode.NEVER        => CullingMode.SHOW_BOTH,
          _                     => throw new ArgumentOutOfRangeException()
      };
    }

  public IMaterial Material { get; }

  public WrapMode CmbToFinWrapMode(TextureWrapMode cmbMode)
    => cmbMode switch {
        TextureWrapMode.CLAMP_TO_BORDER => WrapMode.CLAMP,
        TextureWrapMode.REPEAT        => WrapMode.REPEAT,
        TextureWrapMode.CLAMP_TO_EDGE   => WrapMode.CLAMP,
        TextureWrapMode.MIRROR        => WrapMode.MIRROR_REPEAT,
        _                             => throw new ArgumentOutOfRangeException(nameof(cmbMode), cmbMode, null)
    };

  public FinBlendEquation CmbBlendEquationToFin(
      BlendEquation cmbBlendEquation)
    => cmbBlendEquation switch {
        BlendEquation.FUNC_ADD      => FinBlendEquation.ADD,
        BlendEquation.FUNC_SUBTRACT => FinBlendEquation.SUBTRACT,
        BlendEquation.FUNC_REVERSE_SUBTRACT => FinBlendEquation
            .REVERSE_SUBTRACT,
        BlendEquation.MIN => FinBlendEquation.MIN,
        BlendEquation.MAX => FinBlendEquation.MAX,
        _ => throw new ArgumentOutOfRangeException(
            nameof(cmbBlendEquation),
            cmbBlendEquation,
            null)
    };

  public FinBlendFactor CmbBlendFactorToFin(BlendFactor cmbBlendFactor)
    => cmbBlendFactor switch {
        BlendFactor.ZERO        => FinBlendFactor.ZERO,
        BlendFactor.ONE         => FinBlendFactor.ONE,
        BlendFactor.SOURCE_COLOR => FinBlendFactor.SRC_COLOR,
        BlendFactor.ONE_MINUS_SOURCE_COLOR => FinBlendFactor
            .ONE_MINUS_SRC_COLOR,
        BlendFactor.SOURCE_ALPHA => FinBlendFactor.SRC_ALPHA,
        BlendFactor.ONE_MINUS_SOURCE_ALPHA => FinBlendFactor
            .ONE_MINUS_SRC_ALPHA,
        BlendFactor.DESTINATION_COLOR => FinBlendFactor.DST_COLOR,
        BlendFactor.ONE_MINUS_DESTINATION_COLOR => FinBlendFactor
            .ONE_MINUS_DST_COLOR,
        BlendFactor.DESTINATION_ALPHA => FinBlendFactor.DST_ALPHA,
        BlendFactor.ONE_MINUS_DESTINATION_ALPHA => FinBlendFactor
            .ONE_MINUS_DST_ALPHA,
        BlendFactor.CONSTANT_ALPHA => FinBlendFactor.CONST_ALPHA,
        BlendFactor.ONE_MINUS_CONSTANT_ALPHA => FinBlendFactor.ONE_MINUS_CONST_ALPHA,
        _ => throw new NotSupportedException(),
    };
}