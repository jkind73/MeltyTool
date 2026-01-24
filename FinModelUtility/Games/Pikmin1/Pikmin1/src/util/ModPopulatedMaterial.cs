using System;
using System.Drawing;
using System.Linq;
using System.Numerics;

using fin.color;
using fin.util.asserts;
using fin.util.enums;

using gx;

using pikmin1.schema.mod;


namespace pikmin1.util {
  internal class ModPopulatedMaterial : IPopulatedMaterial {
    public ModPopulatedMaterial(int materialIndex,
                                Material material,
                                int tevInfoIndex,
                                TevInfo tevInfo) {
      if (materialIndex == 45) {
        ;
      }

      // TODO: Where does this come from?
      this.CullMode = GxCullMode.BACK;

      this.KonstColors =
          tevInfo.KonstColors.Select(FinColor.ToSystemColor).ToArray();
      this.ColorRegisters =
          tevInfo.ColorRegisters
                 .Select(reg => reg.color)
                 .Select((rgba, i) => (IColorRegister) new GxColorRegister {
                     // TODO: Support nonclipped colors
                     Color = rgba.ToColor(),
                     Index = 3 * tevInfoIndex + i,
                 })
                 .ToArray();

      // TODO: This is a guess
      {
        // Not sure where this comes from, used for various things:
        //   - Pikmin color
        //   - Armored cannon beetle eye color
        var unkMatColor = (123, Color.White);
        var unkAmbColor = (1234, Color.FromArgb(0, 0, 0, 0));

        var materialColor = (materialIndex,
                             FinColor.ToSystemColor(
                                 material.colorInfo.DiffuseColour));
        var ambientColor
            = (materialIndex,
               FinColor.ToSystemColor(FinColor.FromIntensityFloat(.1f)));
        this.MaterialColors = [materialColor, unkMatColor];
        this.AmbientColors = [ambientColor, unkAmbColor];
      }

      this.TevStageInfos = tevInfo.tevStages.Select(tevStage => {
                                    var colorCombiner = tevStage.ColorCombiner;
                                    var alphaCombiner = tevStage.AlphaCombiner;

                                    return new TevStagePropsImpl {
                                        ColorA = colorCombiner.colorA,
                                        ColorB = colorCombiner.colorB,
                                        ColorC = colorCombiner.colorC,
                                        ColorD = colorCombiner.colorD,
                                        ColorOp = colorCombiner.colorOp,
                                        ColorBias = colorCombiner.colorBias,
                                        ColorScale = colorCombiner.colorScale,
                                        ColorClamp = colorCombiner.colorClamp,
                                        ColorRegid
                                            = colorCombiner.colorRegister,

                                        AlphaA = alphaCombiner.alphaA,
                                        AlphaB = alphaCombiner.alphaB,
                                        AlphaC = alphaCombiner.alphaC,
                                        AlphaD = alphaCombiner.alphaD,
                                        AlphaOp = alphaCombiner.alphaOp,
                                        AlphaBias = alphaCombiner.alphaBias,
                                        AlphaScale = alphaCombiner.alphaScale,
                                        AlphaClamp = alphaCombiner.alphaClamp,
                                        AlphaRegid
                                            = alphaCombiner.alphaRegister
                                    };
                                  })
                                  .ToArray();

      this.TevOrderInfos = tevInfo.tevStages.Select(tevStage => {
                                    var texMap = tevStage.TexMap;
                                    var texCoordId
                                        = GxTexCoord.GX_TEXCOORD_NULL;
                                    var texGenData
                                        = material.texInfo.texGenData;
                                    if ((sbyte) texMap >= 0 &&
                                        (sbyte) texMap < texGenData.Length) {
                                      texCoordId = material.texInfo
                                          .texGenData[(int) texMap].texCoordId;
                                    }

                                    if (tevStage.TexCoord !=
                                        GxTexCoord.GX_TEXCOORD_NULL) {
                                      texCoordId = tevStage.TexCoord;
                                    }

                                    var tevOrderImpl = new TevOrderImpl {
                                        TexMap = texMap,
                                        TexCoordId = texCoordId,
                                        ColorChannelId = tevStage.ColorChannel,
                                        KonstColorSel
                                            = tevStage.KonstColorSelection,
                                        KonstAlphaSel
                                            = tevStage.KonstAlphaSelection,
                                    };

                                    return tevOrderImpl;
                                  })
                                  .ToArray();

      this.ColorChannelControls
          = GetColorChannelControls_(material.LightingInfo);

      {
        this.TextureIndices = material.texInfo.texturesInMaterial
                                      .Select(tex => (short) tex.texAttrIndex)
                                      .ToArray();
        this.TexCoordGens
            = material
              .texInfo
              .texGenData
              .Select(tex => {
                var texMatrix = tex.texMatrix switch {
                    10 => GxTexMatrix.IDENTITY,
                    >= 0 and < 8
                        => (GxTexMatrix) ((int) GxTexMatrix.TEX_MTX0 +
                                          3 * tex.texMatrix),
                };
                return new TexCoordGenImpl(
                    GxTexGenType.MATRIX2_X4,
                    tex.TexGenSrc,
                    texMatrix);
              })
              .ToArray();
        this.TextureMatrices
            = material.texInfo.texturesInMaterial.Select(
                          t => new TextureMatrixInfoImpl(
                              GxTexGenType.MATRIX2_X4,
                              new Vector3 {
                                  X = t.Center.X,
                                  Y = t.Center.Y
                              },
                              t.Scale,
                              t.Translation,
                              (short) (t.rotation / MathF.PI * 32768f)
                          ))
                      .ToArray();
        this.TextureWrapModeOverrides
            = material.texInfo.texturesInMaterial
                      .Select(tex => (tex.WrapModeS, tex.WrapModeT))
                      .ToArray();
      }

      {
        GetPeInfoValues_(material.peInfo,
                         material.flags,
                         out var blendFunction,
                         out var alphaCompare,
                         out var depthFunction);
        this.BlendMode = blendFunction;
        this.AlphaCompare = alphaCompare;
        this.DepthFunction = depthFunction;
      }
    }


    public string Name => "material";
    public GxCullMode CullMode { get; }
    public (int, Color)[] MaterialColors { get; }
    public IColorChannelControl?[] ColorChannelControls { get; }
    public (int, Color)[] AmbientColors { get; }
    public Color?[] LightColors { get; } = [];
    public Color[] KonstColors { get; }
    public IColorRegister[] ColorRegisters { get; }
    public ITevOrder?[] TevOrderInfos { get; }
    public ITevStageProps?[] TevStageInfos { get; }
    public ITevSwapMode?[] TevSwapModes { get; }
    public ITevSwapModeTable?[] TevSwapModeTables { get; }

    public IAlphaCompare AlphaCompare { get; }
    public IBlendFunction BlendMode { get; }

    public short[] TextureIndices { get; }
    public ITexCoordGen[] TexCoordGens { get; }
    public ITextureMatrixInfo?[] TextureMatrices { get; }

    public (GxWrapMode wrapModeS, GxWrapMode wrapModeT)[]?
        TextureWrapModeOverrides { get; }

    public IDepthFunction DepthFunction { get; }

    public IIndirectTexture? IndirectTexture { get; }

    /// <summary>
    ///   Ripped straight from the decomp.
    /// </summary>
    private static void GetPeInfoValues_(PeInfo peInfo,
                                         MaterialFlags materialFlags,
                                         out IBlendFunction blendFunction,
                                         out IAlphaCompare alphaCompare,
                                         out IDepthFunction depthFunction) {
      if (peInfo.flags.CheckFlag(PeInfoFlags.ENABLED)) {
        blendFunction = new BlendFunctionImpl {
            BlendMode = peInfo.BlendMode,
            SrcFactor = peInfo.SrcFactor,
            DstFactor = peInfo.DstFactor,
            LogicOp = peInfo.LogicOp,
        };
        alphaCompare = new AlphaCompareImpl {
            MergeFunc = peInfo.AlphaCompareOp,
            Func0 = peInfo.CompareType0,
            Reference0 = peInfo.Reference0,
            Func1 = peInfo.CompareType1,
            Reference1 = peInfo.Reference1,
        };
        depthFunction = new DepthFunctionImpl {
            Enable = peInfo.Enable,
            Func = peInfo.DepthCompareType,
            WriteNewValueIntoDepthBuffer = peInfo.WriteNewIntoBuffer,
        };
        return;
      }

      if (materialFlags.CheckFlag(MaterialFlags.INVERT_SPECIAL_BLEND)) {
        blendFunction = new BlendFunctionImpl {
            BlendMode = GxBlendMode.BLEND,
            SrcFactor = GxBlendFactor.ZERO,
            DstFactor = GxBlendFactor.ONE_MINUS_SRC_COLOR,
            LogicOp = GxLogicOp.CLEAR,
        };
        alphaCompare = new AlphaCompareImpl {
            Func0 = GxCompareType.ALWAYS,
            Reference0 = 0,
            MergeFunc = GxAlphaOp.OR,
            Func1 = GxCompareType.ALWAYS,
            Reference1 = 0,
        };
        depthFunction = new DepthFunctionImpl {
            Enable = true,
            Func = GxCompareType.L_EQUAL,
            WriteNewValueIntoDepthBuffer = false
        };
        return;
      }

      if (materialFlags.CheckFlag(MaterialFlags.OPAQUE)) {
        blendFunction = new BlendFunctionImpl {
            BlendMode = GxBlendMode.NONE,
            SrcFactor = GxBlendFactor.ONE,
            DstFactor = GxBlendFactor.ZERO,
            LogicOp = GxLogicOp.COPY,
        };
        alphaCompare = new AlphaCompareImpl {
            Func0 = GxCompareType.ALWAYS,
            Reference0 = 0,
            MergeFunc = GxAlphaOp.OR,
            Func1 = GxCompareType.ALWAYS,
            Reference1 = 0,
        };
        depthFunction = new DepthFunctionImpl {
            Enable = true,
            Func = GxCompareType.L_EQUAL,
            WriteNewValueIntoDepthBuffer = true,
        };
        return;
      }

      if (materialFlags.CheckFlag(MaterialFlags.ALPHA_CLIP)) {
        blendFunction = new BlendFunctionImpl {
            BlendMode = GxBlendMode.NONE,
            SrcFactor = GxBlendFactor.ONE,
            DstFactor = GxBlendFactor.ZERO,
            LogicOp = GxLogicOp.COPY,
        };
        alphaCompare = new AlphaCompareImpl {
            Func0 = GxCompareType.G_EQUAL,
            Reference0 = .5f,
            MergeFunc = GxAlphaOp.AND,
            Func1 = GxCompareType.L_EQUAL,
            Reference1 = 1,
        };
        depthFunction = new DepthFunctionImpl {
            Enable = true,
            Func = GxCompareType.L_EQUAL,
            WriteNewValueIntoDepthBuffer = true,
        };
        return;
      }

      // Otherwise, TRANSPARENT_BLEND
      Asserts.True(materialFlags.CheckFlag(MaterialFlags.TRANSPARENT_BLEND));
      blendFunction = new BlendFunctionImpl {
          BlendMode = GxBlendMode.BLEND,
          SrcFactor = GxBlendFactor.SRC_ALPHA,
          DstFactor = GxBlendFactor.ONE_MINUS_SRC_ALPHA,
          LogicOp = GxLogicOp.COPY,
      };
      alphaCompare = new AlphaCompareImpl {
          Func0 = GxCompareType.ALWAYS,
          Reference0 = 0,
          MergeFunc = GxAlphaOp.OR,
          Func1 = GxCompareType.ALWAYS,
          Reference1 = 0,
      };
      depthFunction = new DepthFunctionImpl {
          Enable = true,
          Func = GxCompareType.L_EQUAL,
          WriteNewValueIntoDepthBuffer = false,
      };
    }

    private static IColorChannelControl[] GetColorChannelControls_(
        LightingInfo? lightingInfo) {
      IColorChannelControl ccc0, ccc1, ccc2, ccc3;

      // In the game, it only ever uses lights 0 and 1.
      // In the game, it uses a different light mask for the alpha channel.
      var litMask = GxLightMask.LIGHT0 |
                    GxLightMask.LIGHT1 |
                    GxLightMask.LIGHT2;

      if (lightingInfo == null) {
        ccc0 = ccc1 = new ColorChannelControlImpl {
            LightingEnabled = true,
            AmbientSrc = GxColorSrc.REGISTER,
            MaterialSrc = GxColorSrc.REGISTER,
            LitMask = litMask,
            DiffuseFunction = GxDiffuseFunction.CLAMP,
            AttenuationFunction = GxAttenuationFunction.NONE,
        };
      } else {
        ccc0 = new ColorChannelControlImpl {
            LightingEnabled = lightingInfo.LightingEnabledForColor0,
            AmbientSrc = lightingInfo.AmbientColorSrcForColor0,
            MaterialSrc = lightingInfo.MaterialColorSrcForColor0,
            LitMask = litMask,
            DiffuseFunction = lightingInfo.DiffuseFunctionForColor0,
            AttenuationFunction = lightingInfo.LightingEnabledForColor0
                ? GxAttenuationFunction.SPOT
                : GxAttenuationFunction.NONE,
        };
        ccc1 = new ColorChannelControlImpl {
            LightingEnabled = lightingInfo.LightingEnabledForAlpha0,
            MaterialSrc = lightingInfo.MaterialColorSrcForAlpha0,
            AmbientSrc = lightingInfo.AmbientColorSrcForAlpha0,
            LitMask = litMask,
            DiffuseFunction = lightingInfo.DiffuseFunctionForAlpha0,
            AttenuationFunction = lightingInfo.LightingEnabledForAlpha0
                ? GxAttenuationFunction.SPOT
                : GxAttenuationFunction.NONE,
        };
      }

      if (lightingInfo is not { LightingEnabledForColor1: true }) {
        ccc2 = ccc3 = new ColorChannelControlImpl {
            LightingEnabled = false,
            AmbientSrc = GxColorSrc.REGISTER,
            MaterialSrc = GxColorSrc.REGISTER,
            DiffuseFunction = GxDiffuseFunction.NONE,
            AttenuationFunction = GxAttenuationFunction.NONE,
            VertexColorIndex = 0,
        };
      } else {
        var colorAlpha1LitMask = (GxLightMask) 0x80;
        ccc2 = new ColorChannelControlImpl {
            LightingEnabled = true,
            MaterialSrc = GxColorSrc.REGISTER,
            AmbientSrc = GxColorSrc.REGISTER,
            LitMask = colorAlpha1LitMask,
            DiffuseFunction = lightingInfo.DiffuseFunctionForColor1,
            AttenuationFunction = GxAttenuationFunction.SPEC,
            VertexColorIndex = 0,
        };
        ccc3 = new ColorChannelControlImpl {
            LightingEnabled = false,
            MaterialSrc = GxColorSrc.REGISTER,
            AmbientSrc = GxColorSrc.REGISTER,
            LitMask = colorAlpha1LitMask,
            DiffuseFunction = GxDiffuseFunction.CLAMP,
            AttenuationFunction = GxAttenuationFunction.NONE,
            VertexColorIndex = 0,
        };
      }

      return [ccc0, ccc1, ccc2, ccc3];
    }
  }
}