using System.Drawing;

using f3dzex2.combiner;
using f3dzex2.displaylist.opcodes;
using f3dzex2.image;

using fin.color;

using marioartist.schema.talent_studio;

namespace marioartist.api;

/// <summary>
///   The game uses VERY specific hard-coded logic to set up the display lists,
///   to the point where 100% accuracy is 100% impossible without just
///   straight-up reproducing this logic verbatim.
///
///   It's fucked up that the game hardcodes this logic like this, but there's
///   really nothing else to be done. Just have to suck it up and include their
///   same bullshit here too.
/// </summary>
internal class ChosenPart0Util {
  public static void SetUp(
      IN64Hardware n64Hardware,
      ChosenPart0 chosenPart0,
      int patternIndex) {
    // From decomp, at 0x801168a0

    var rdp = n64Hardware.Rdp;
    var rsp = n64Hardware.Rsp;

    // Like seriously, what is this. What the fuck is this.
    if (patternIndex is 0x3 or 0x4 or 0x5 or 0x8 or 0x9 or 0xb or 0xc) {
      SetUpPartOfTexture(n64Hardware, chosenPart0.Id, patternIndex);
    } else {
      if (patternIndex is 0x0 or 0x6 or 0xa) {
        SetUpPartOfTexture(n64Hardware, chosenPart0.Id, patternIndex);
        SetUpInvertedEnvironmentColorOrSomethingElse(
            n64Hardware,
            chosenPart0.ChosenColor0);
      } else {
        var patternParams = patternIndex switch {
            1 => chosenPart0.Pattern0Params,
            2 => chosenPart0.Pattern1Params,
            _ => chosenPart0.MarkParams,
        };

        if (patternParams.Unk1 == 0 && chosenPart0.Id == 0) {
          rsp.UvType = N64UvType.STANDARD;
          rsp.TexScaleXShort = rsp.TexScaleYShort = 0xffff;

          rdp.SetCombinerCycleParams(
              new CombinerCycleParams {
                  ColorMuxA = GenericColorMux.G_CCMUX_0,
                  ColorMuxB = GenericColorMux.G_CCMUX_ENVIRONMENT,
                  ColorMuxC = GenericColorMux.G_CCMUX_PRIMITIVE_ALPHA,
                  ColorMuxD = GenericColorMux.G_CCMUX_TEXEL0,
                  AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
                  AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
                  AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
                  AlphaMuxD = GenericAlphaMux.G_ACMUX_TEXEL0,
              },
              new CombinerCycleParams {
                  ColorMuxA = GenericColorMux.G_CCMUX_COMBINED,
                  ColorMuxB = GenericColorMux.G_CCMUX_0,
                  ColorMuxC = GenericColorMux.G_CCMUX_SHADE,
                  ColorMuxD = GenericColorMux.G_CCMUX_0,
                  AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
                  AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
                  AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
                  AlphaMuxD = GenericAlphaMux.G_ACMUX_COMBINED,
              });
        } else {
          SetUpTexture(n64Hardware, patternParams, patternIndex);
        }
      }
    }
  }

  public static void SetUpPartOfTexture(IN64Hardware n64Hardware,
                                        int chosenPartId,
                                        int patternIndex) {
    // From decomp, at 0x801167b8

    var rsp = n64Hardware.Rsp;

    if (patternIndex is 0x5 or 0xc) {
      rsp.UvType = N64UvType.SPHERICAL;
      if (chosenPartId is 8) {
        rsp.TexScaleXShort
            = rsp.TexScaleXShort = GetTexScaleShortFromCommand_(0x07d0);
      } else if (chosenPartId is 5) {
        rsp.TexScaleXShort
            = rsp.TexScaleXShort = GetTexScaleShortFromCommand_(0x08ca);
      } else {
        rsp.TexScaleXShort
            = rsp.TexScaleXShort = GetTexScaleShortFromCommand_(0x0fa0);
      }
    } else {
      rsp.UvType = N64UvType.STANDARD;
      rsp.TexScaleXShort
          = rsp.TexScaleXShort = GetTexScaleShortFromCommand_(0xffff);
    }
  }

  public static void SetUpInvertedEnvironmentColorOrSomethingElse(
      IN64Hardware n64Hardware,
      ChosenColor? chosenColor) {
    // From decomp, at 0x8010e1b8

    // TODO: Figure out what the heck the fallback color is meant to be.
    var rawColor = chosenColor?.Color.ToSystemColor() ?? Color.White;

    var rsp = n64Hardware.Rsp;
    rsp.EnvironmentColor = Color.FromArgb(
        rawColor.A,
        0xff - rawColor.R,
        0xff - rawColor.G,
        0xff - rawColor.B);
  }

  public static void SetUpTexture(
      IN64Hardware n64Hardware,
      ChosenPart0PatternParams patternParams,
      int patternIndex) {
    // From decomp, at 0x801160d0

    var rdp = n64Hardware.Rdp;
    var rsp = n64Hardware.Rsp;
    var tmem = rdp.Tmem;

    if (patternIndex == 0x7) {
      rsp.UvType = N64UvType.STANDARD;
      rdp.SetCombinerCycleParams(
          new CombinerCycleParams {
              ColorMuxA = GenericColorMux.G_CCMUX_TEXEL0,
              ColorMuxB = GenericColorMux.G_CCMUX_0,
              ColorMuxC = GenericColorMux.G_CCMUX_ENVIRONMENT,
              ColorMuxD = GenericColorMux.G_CCMUX_0,
              AlphaMuxA = GenericAlphaMux.G_ACMUX_TEXEL0,
              AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxC = GenericAlphaMux.G_ACMUX_ENVIRONMENT,
              AlphaMuxD = GenericAlphaMux.G_ACMUX_0,
          },
          new CombinerCycleParams {
              ColorMuxA = GenericColorMux.G_CCMUX_COMBINED,
              ColorMuxB = GenericColorMux.G_CCMUX_0,
              ColorMuxC = GenericColorMux.G_CCMUX_SHADE,
              ColorMuxD = GenericColorMux.G_CCMUX_0,
              AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxD = GenericAlphaMux.G_ACMUX_COMBINED,
          });
      rsp.EnvironmentColor = Color.FromArgb(0xf0, 0xf0, 0xf0, 0xff);
      tmem.SetImageSimple(
          patternParams.ImageSegmentedAddress,
          N64ColorFormat.RGBA,
          BitsPerTexel._16BPT,
          32,
          32,
          F3dWrapMode.CLAMP,
          F3dWrapMode.CLAMP,
          // TODO: Pretty sure this is right...???
          tileDescriptorIndex: 1);
      return;
    }

    var patternMaterialType = patternParams.MaterialType;

    ushort scale;
    if (patternMaterialType == PatternMaterialType.SPHERICAL) {
      rsp.UvType = N64UvType.SPHERICAL;
      // TODO: I might be stupid, this might be 0x07c0?
      scale = GetTexScaleShortFromCommand_(0x07c0);
    } else {
      rsp.UvType = N64UvType.STANDARD;
      scale = GetTexScaleShortFromCommand_(0xffff);
    }

    if (patternMaterialType == PatternMaterialType.SPHERICAL) {
      rdp.SetCombinerCycleParams(
          new CombinerCycleParams {
              ColorMuxA = GenericColorMux.G_CCMUX_1,
              ColorMuxB = GenericColorMux.G_CCMUX_TEXEL1,
              ColorMuxC = GenericColorMux.G_CCMUX_ENV_ALPHA,
              ColorMuxD = GenericColorMux.G_CCMUX_TEXEL1,
              AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxD = GenericAlphaMux.G_ACMUX_TEXEL0,
          },
          new CombinerCycleParams {
              ColorMuxA = GenericColorMux.G_CCMUX_COMBINED,
              ColorMuxB = GenericColorMux.G_CCMUX_0,
              ColorMuxC = GenericColorMux.G_CCMUX_SHADE,
              ColorMuxD = GenericColorMux.G_CCMUX_0,
              AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxD = GenericAlphaMux.G_ACMUX_COMBINED,
          });
      rsp.EnvironmentColor = Color.FromArgb(0x30, 0xff, 0xff, 0xff);
      // TODO: Does something weird with uls/ult/lrs/lrt, might need to copy this here?
      tmem.SetImageSimple(
          patternParams.ImageSegmentedAddress,
          N64ColorFormat.RGBA,
          BitsPerTexel._16BPT,
          32,
          32,
          F3dWrapMode.REPEAT,
          F3dWrapMode.MIRROR_REPEAT,
          scale: scale,
          // TODO: Pretty sure this is right...???
          tileDescriptorIndex: 1);
    } else {
      ushort shift = patternMaterialType switch {
          PatternMaterialType.BLEND_2X2
              or PatternMaterialType.MULTIPLY_2X2 => 0xf,
          _ => 0,
      };

      if (patternMaterialType is PatternMaterialType.MULTIPLY_1X1
                                 or PatternMaterialType.MULTIPLY_2X2) {
        rdp.SetCombinerCycleParams(
            new CombinerCycleParams {
                ColorMuxA = GenericColorMux.G_CCMUX_TEXEL1,
                ColorMuxB = GenericColorMux.G_CCMUX_ENVIRONMENT,
                ColorMuxC = GenericColorMux.G_CCMUX_PRIM_LOD_FRAC,
                ColorMuxD = GenericColorMux.G_CCMUX_TEXEL0,
                AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
                AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
                AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
                AlphaMuxD = GenericAlphaMux.G_ACMUX_TEXEL0,
            },
            new CombinerCycleParams {
                ColorMuxA = GenericColorMux.G_CCMUX_COMBINED,
                ColorMuxB = GenericColorMux.G_CCMUX_0,
                ColorMuxC = GenericColorMux.G_CCMUX_SHADE,
                ColorMuxD = GenericColorMux.G_CCMUX_0,
                AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
                AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
                AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
                AlphaMuxD = GenericAlphaMux.G_ACMUX_COMBINED,
            });
        rsp.EnvironmentColor = Color.FromArgb(0xff,
                                              0xd7,
                                              0xd7,
                                              0xd7);
      } else {
        rdp.SetCombinerCycleParams(
            new CombinerCycleParams {
                ColorMuxA = GenericColorMux.G_CCMUX_TEXEL1,
                ColorMuxB = GenericColorMux.G_CCMUX_ENVIRONMENT,
                ColorMuxC = GenericColorMux.G_CCMUX_TEXEL0,
                ColorMuxD = GenericColorMux.G_CCMUX_TEXEL0,
                AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
                AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
                AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
                AlphaMuxD = GenericAlphaMux.G_ACMUX_TEXEL0,
            },
            new CombinerCycleParams {
                ColorMuxA = GenericColorMux.G_CCMUX_COMBINED,
                ColorMuxB = GenericColorMux.G_CCMUX_0,
                ColorMuxC = GenericColorMux.G_CCMUX_SHADE,
                ColorMuxD = GenericColorMux.G_CCMUX_0,
                AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
                AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
                AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
                AlphaMuxD = GenericAlphaMux.G_ACMUX_COMBINED,
            });

        rsp.EnvironmentColor = Color.FromArgb(0xff, 0xc8, 0xc8, 0xc8);
      }

      tmem.SetImageSimple(
          patternParams.ImageSegmentedAddress,
          N64ColorFormat.RGBA,
          BitsPerTexel._16BPT,
          32,
          32,
          F3dWrapMode.REPEAT,
          F3dWrapMode.REPEAT,
          scale: scale,
          shift: shift,
          // TODO: Pretty sure this is right...???
          tileDescriptorIndex: 1);
    }
  }

  public static void SetUpOtherModeLAndCombiner(
      IN64Hardware n64Hardware,
      int patternIndex,
      int someValue) {
    // From decomp, at 0x80115d08

    var rdp = n64Hardware.Rdp;
    var rsp = n64Hardware.Rsp;
    var tmem = rdp.Tmem;

    rsp.UvType = N64UvType.STANDARD;
    rsp.TexScaleXShort
        = rsp.TexScaleYShort = GetTexScaleShortFromCommand_(0xffff);

    if (patternIndex is 3 or 5 or 8 or 9) {
      rdp.SetCombinerCycleParams(
          new CombinerCycleParams {
              ColorMuxA = GenericColorMux.G_CCMUX_TEXEL0,
              ColorMuxB = GenericColorMux.G_CCMUX_0,
              ColorMuxC = GenericColorMux.G_CCMUX_SHADE,
              ColorMuxD = GenericColorMux.G_CCMUX_0,
              AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxD = GenericAlphaMux.G_ACMUX_TEXEL0,
          },
          new CombinerCycleParams {
              ColorMuxA = GenericColorMux.G_CCMUX_0,
              ColorMuxB = GenericColorMux.G_CCMUX_0,
              ColorMuxC = GenericColorMux.G_CCMUX_0,
              ColorMuxD = GenericColorMux.G_CCMUX_COMBINED,
              AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxD = GenericAlphaMux.G_ACMUX_COMBINED,
          });
    } else if (patternIndex is 0 or 6) {
      rdp.SetCombinerCycleParams(
          new CombinerCycleParams {
              ColorMuxA = GenericColorMux.G_CCMUX_0,
              ColorMuxB = GenericColorMux.G_CCMUX_ENVIRONMENT,
              ColorMuxC = GenericColorMux.G_CCMUX_PRIM_LOD_FRAC,
              ColorMuxD = GenericColorMux.G_CCMUX_TEXEL0,
              AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxD = GenericAlphaMux.G_ACMUX_TEXEL0,
          },
          new CombinerCycleParams {
              ColorMuxA = GenericColorMux.G_CCMUX_COMBINED,
              ColorMuxB = GenericColorMux.G_CCMUX_0,
              ColorMuxC = GenericColorMux.G_CCMUX_SHADE,
              ColorMuxD = GenericColorMux.G_CCMUX_0,
              AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxD = GenericAlphaMux.G_ACMUX_COMBINED,
          });
    } else if (patternIndex is 4) {
      rdp.SetCombinerCycleParams(
          new CombinerCycleParams {
              ColorMuxA = GenericColorMux.G_CCMUX_TEXEL0,
              ColorMuxB = GenericColorMux.G_CCMUX_0,
              ColorMuxC = GenericColorMux.G_CCMUX_ENVIRONMENT,
              ColorMuxD = GenericColorMux.G_CCMUX_0,
              AlphaMuxA = GenericAlphaMux.G_ACMUX_TEXEL0,
              AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxC = GenericAlphaMux.G_ACMUX_ENVIRONMENT,
              AlphaMuxD = GenericAlphaMux.G_ACMUX_0,
          },
          new CombinerCycleParams {
              ColorMuxA = GenericColorMux.G_CCMUX_COMBINED,
              ColorMuxB = GenericColorMux.G_CCMUX_0,
              ColorMuxC = GenericColorMux.G_CCMUX_SHADE,
              ColorMuxD = GenericColorMux.G_CCMUX_0,
              AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxD = GenericAlphaMux.G_ACMUX_COMBINED,
          });
      rsp.EnvironmentColor = Color.FromArgb(0xff, 0xa7, 0xa7, 0xa7);
    } else if (patternIndex is 0xb or 0xc) {
      rdp.SetCombinerCycleParams(
          new CombinerCycleParams {
              ColorMuxA = GenericColorMux.G_CCMUX_TEXEL0,
              ColorMuxB = GenericColorMux.G_CCMUX_0,
              ColorMuxC = GenericColorMux.G_CCMUX_SHADE,
              ColorMuxD = GenericColorMux.G_CCMUX_0,
              AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxD = GenericAlphaMux.G_ACMUX_TEXEL0,
          },
          new CombinerCycleParams {
              ColorMuxA = GenericColorMux.G_CCMUX_0,
              ColorMuxB = GenericColorMux.G_CCMUX_0,
              ColorMuxC = GenericColorMux.G_CCMUX_0,
              ColorMuxD = GenericColorMux.G_CCMUX_COMBINED,
              AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxD = GenericAlphaMux.G_ACMUX_COMBINED,
          });
    } else {
      rdp.SetCombinerCycleParams(
          new CombinerCycleParams {
              ColorMuxA = GenericColorMux.G_CCMUX_0,
              ColorMuxB = GenericColorMux.G_CCMUX_ENVIRONMENT,
              ColorMuxC = GenericColorMux.G_CCMUX_PRIM_LOD_FRAC,
              ColorMuxD = GenericColorMux.G_CCMUX_TEXEL0,
              AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxD = GenericAlphaMux.G_ACMUX_TEXEL0,
          },
          new CombinerCycleParams {
              ColorMuxA = GenericColorMux.G_CCMUX_COMBINED,
              ColorMuxB = GenericColorMux.G_CCMUX_0,
              ColorMuxC = GenericColorMux.G_CCMUX_SHADE,
              ColorMuxD = GenericColorMux.G_CCMUX_0,
              AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
              AlphaMuxD = GenericAlphaMux.G_ACMUX_COMBINED,
          });
    }

    if (someValue == 0) {
      if (patternIndex is 0 or 1 or 2 or 3 or 4 or 5) {
        rdp.ZCompare = true;
        rdp.ZUpdate = true;
        rdp.ZMode = ZMode.ZMODE_OPA;
        rdp.MultiplyCoverageWithAlpha = false;
        rdp.UseCoverageForAlpha = true;
        rdp.ForceBlending = false;
        rdp.P0 = BlenderPm.G_BL_CLR_IN;
        rdp.A0 = BlenderA.G_BL_0;
        rdp.M0 = BlenderPm.G_BL_CLR_IN;
        rdp.B0 = BlenderB.G_BL_1;
        rdp.P1 = BlenderPm.G_BL_CLR_IN;
        rdp.A1 = BlenderA.G_BL_A_IN;
        rdp.M1 = BlenderPm.G_BL_CLR_MEM;
        rdp.B1 = BlenderB.G_BL_1MA;
      } else if (patternIndex is 6 or 7 or 8) {
        rdp.ZCompare = true;
        rdp.ZUpdate = true;
        rdp.ZMode = ZMode.ZMODE_OPA;
        rdp.MultiplyCoverageWithAlpha = true;
        rdp.UseCoverageForAlpha = true;
        rdp.ForceBlending = false;
        rdp.P0 = BlenderPm.G_BL_CLR_IN;
        rdp.A0 = BlenderA.G_BL_0;
        rdp.M0 = BlenderPm.G_BL_CLR_IN;
        rdp.B0 = BlenderB.G_BL_1;
        rdp.P1 = BlenderPm.G_BL_CLR_IN;
        rdp.A1 = BlenderA.G_BL_A_IN;
        rdp.M1 = BlenderPm.G_BL_CLR_MEM;
        rdp.B1 = BlenderB.G_BL_1MA;
      } else if (patternIndex is 9) {
        rdp.ZCompare = true;
        rdp.ZUpdate = false;
        rdp.ZMode = ZMode.ZMODE_DEC;
        rdp.MultiplyCoverageWithAlpha = false;
        rdp.UseCoverageForAlpha = false;
        rdp.ForceBlending = true;
        rdp.P0 = BlenderPm.G_BL_CLR_IN;
        rdp.A0 = BlenderA.G_BL_0;
        rdp.M0 = BlenderPm.G_BL_CLR_IN;
        rdp.B0 = BlenderB.G_BL_1;
        rdp.P1 = BlenderPm.G_BL_CLR_IN;
        rdp.A1 = BlenderA.G_BL_A_IN;
        rdp.M1 = BlenderPm.G_BL_CLR_MEM;
        rdp.B1 = BlenderB.G_BL_1MA;
      } else if (patternIndex is 0xa or 0xb or 0xc) {
        rdp.ZCompare = true;
        rdp.ZUpdate = false;
        rdp.ZMode = ZMode.ZMODE_XLU;
        rdp.MultiplyCoverageWithAlpha = false;
        rdp.UseCoverageForAlpha = false;
        rdp.ForceBlending = true;
        rdp.P0 = BlenderPm.G_BL_CLR_IN;
        rdp.A0 = BlenderA.G_BL_0;
        rdp.M0 = BlenderPm.G_BL_CLR_IN;
        rdp.B0 = BlenderB.G_BL_1;
        rdp.P1 = BlenderPm.G_BL_CLR_IN;
        rdp.A1 = BlenderA.G_BL_A_IN;
        rdp.M1 = BlenderPm.G_BL_CLR_MEM;
        rdp.B1 = BlenderB.G_BL_1MA;
      }
    } else {
      throw new NotImplementedException();
    }
  }

  // TODO: Might need to flip endianness?
  private static ushort GetTexScaleShortFromCommand_(ushort scale)
    => (ushort) ((scale >> 8) | (scale << 8));
}