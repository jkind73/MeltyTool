using System.Drawing;

using f3dzex2.combiner;
using f3dzex2.displaylist.opcodes;
using f3dzex2.image;
using f3dzex2.io;

using fin.math;
using fin.model;

using marioartist.schema.talent_studio;

using OneOf;

namespace marioartist.api;

public static class JankTstltUtil {
  public static void ResetRspAndRdp(IN64Hardware n64Hardware) {
    // TODO: The way these are reset in the original game is actually really, really, really complicated
    var rsp = n64Hardware.Rsp;
    rsp.UvType = N64UvType.LINEAR;
    rsp.EnvironmentColor = Color.White;
    rsp.PrimColor = Color.White;

    var rdp = n64Hardware.Rdp;
    rdp.ForceBlending = false;
    rdp.P0 = BlenderPm.G_BL_CLR_MEM;
    rdp.A0 = BlenderA.G_BL_0;
    rdp.M0 = BlenderPm.G_BL_CLR_IN;
    rdp.B0 = BlenderB.G_BL_1;
    rdp.P1 = BlenderPm.G_BL_CLR_MEM;
    rdp.A1 = BlenderA.G_BL_0;
    rdp.M1 = BlenderPm.G_BL_CLR_IN;
    rdp.B1 = BlenderB.G_BL_1;
    rdp.CycleType = CycleType.TWO_CYCLE;

    rdp.UseCoverageForAlpha = true;
  }

  public static void SetAdditiveBlending(IN64Hardware n64Hardware) {
    var rsp = n64Hardware.Rsp;
    rsp.UvType = N64UvType.LINEAR;
    rsp.UvType = N64UvType.STANDARD;
    rsp.EnvironmentColor = Color.White;
    rsp.PrimColor = Color.White;

    var rdp = n64Hardware.Rdp;
    rdp.ForceBlending = true;
    rdp.P0 = BlenderPm.G_BL_CLR_MEM;
    rdp.A0 = BlenderA.G_BL_A_IN;
    rdp.M0 = BlenderPm.G_BL_CLR_IN;
    rdp.B0 = BlenderB.G_BL_1MA;
    rdp.P1 = BlenderPm.G_BL_CLR_MEM;
    rdp.A1 = BlenderA.G_BL_A_IN;
    rdp.M1 = BlenderPm.G_BL_CLR_IN;
    rdp.B1 = BlenderB.G_BL_1MA;
    rdp.CycleType = CycleType.TWO_CYCLE;

    rdp.UseCoverageForAlpha = false;
  }

  public static void SetCombiner(
      IN64Hardware n64Hardware,
      bool withTexture0,
      bool withAlpha,
      OneOf<uint, Color>? patternSegmentedOffsetOrColor = null,
      PatternMaterialType patternMaterialType = PatternMaterialType.BLEND_1X1) {
    var rdp = n64Hardware.Rdp;
    var rsp = n64Hardware.Rsp;

    // Based on decomp, early in function at 0x801150e8
    rsp.PrimLodFraction = 1f * 0x7f / 0x100;

    ushort shift = patternMaterialType switch {
        PatternMaterialType.BLEND_2X2
            or PatternMaterialType.MULTIPLY_2X2 => 0xf,
        _ => 0,
    };

    rsp.UvType = patternMaterialType == PatternMaterialType.SPHERICAL
        ? N64UvType.SPHERICAL
        : N64UvType.STANDARD;
    rdp.Tmem.GsSpTexture(BitLogic.ConvertDoubleToBinaryFraction(1),
                         BitLogic.ConvertDoubleToBinaryFraction(1),
                         0,
                         0,
                         withTexture0
                             ? TileDescriptorState.ENABLED
                             : TileDescriptorState.DISABLED);

    switch (withTexture0, patternSegmentedOffsetOrColor) {
      case (true, not null): {
        if (patternSegmentedOffsetOrColor.Value.TryPickT0(
                out var patternSegmentedOffset,
                out var color)) {
          rdp.Tmem.SetImageSimple(
              patternSegmentedOffset,
              N64ColorFormat.RGBA,
              BitsPerTexel._16BPT,
              32,
              32,
              F3dWrapMode.REPEAT,
              F3dWrapMode.REPEAT,
              1,
              shift);

          rdp.SetCombinerCycleParams(
              FromBlendingTexture0AndTexture1WithEnvColorAndShade(
                  patternMaterialType,
                  withAlpha));

          // From decomp, 0x801162a4 onwards
          var (envValue, envAlpha) = patternMaterialType switch {
              PatternMaterialType.BLEND_1X1
                  or PatternMaterialType.BLEND_2X2 => (0xc8, 0xff),
              PatternMaterialType.MULTIPLY_1X1
                  or PatternMaterialType.MULTIPLY_2X2 => (0xd7, 0xff),
              PatternMaterialType.SPHERICAL => (0xff, 0x30),
          };
          rsp.EnvironmentColor = Color.FromArgb(envAlpha,
                                                envValue,
                                                envValue,
                                                envValue);
        } else {
          rdp.SetCombinerCycleParams(
              CombinerCycleParams
                  .FromTexture0AndLightingAndPrimitive(withAlpha));
          rsp.PrimColor = color;
        }

        break;
      }
      case (true, null): {
        rdp.SetCombinerCycleParams(
            CombinerCycleParams.FromTexture0AndShade(withAlpha));
        break;
      }
      case (false, not null): {
        if (patternSegmentedOffsetOrColor.Value.TryPickT0(
                out var patternSegmentedOffset,
                out var color)) {
          rdp.Tmem.SetImageSimple(
              patternSegmentedOffset,
              N64ColorFormat.RGBA,
              BitsPerTexel._16BPT,
              32,
              32,
              F3dWrapMode.REPEAT,
              F3dWrapMode.REPEAT,
              0,
              shift);

          rdp.SetCombinerCycleParams(
              CombinerCycleParams.FromTexture0AndShade(withAlpha));
        } else {
          rdp.SetCombinerCycleParams(
              CombinerCycleParams.FromPrimitiveAndLighting(withAlpha));
          rsp.PrimColor = color;
        }

        break;
      }
      default: {
        rdp.SetCombinerCycleParams(
            CombinerCycleParams.FromTexture0AndShade(withAlpha));
        break;
      }
    }
  }

  // From decomp, 0x801162a4 onwards
  public static (CombinerCycleParams, CombinerCycleParams)
      FromBlendingTexture0AndTexture1WithEnvColorAndShade(
          PatternMaterialType patternMaterialType,
          bool withAlpha)
    => (patternMaterialType switch {
            PatternMaterialType.BLEND_1X1 or PatternMaterialType.BLEND_2X2 =>
                new() {
                    ColorMuxA = GenericColorMux.G_CCMUX_TEXEL1,
                    ColorMuxB = GenericColorMux.G_CCMUX_ENVIRONMENT,
                    ColorMuxC = GenericColorMux.G_CCMUX_TEXEL0,
                    ColorMuxD = GenericColorMux.G_CCMUX_TEXEL0,
                    AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
                    AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
                    AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
                    AlphaMuxD = withAlpha
                        ? GenericAlphaMux.G_ACMUX_TEXEL0
                        : GenericAlphaMux.G_ACMUX_1,
                },
            PatternMaterialType.MULTIPLY_1X1
                or PatternMaterialType.MULTIPLY_2X2 =>
                new() {
                    ColorMuxA = GenericColorMux.G_CCMUX_TEXEL1,
                    ColorMuxB = GenericColorMux.G_CCMUX_ENVIRONMENT,
                    ColorMuxC = GenericColorMux.G_CCMUX_PRIM_LOD_FRAC,
                    ColorMuxD = GenericColorMux.G_CCMUX_TEXEL0,
                    AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
                    AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
                    AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
                    AlphaMuxD = withAlpha
                        ? GenericAlphaMux.G_ACMUX_TEXEL0
                        : GenericAlphaMux.G_ACMUX_1,
                },
            PatternMaterialType.SPHERICAL =>
                new() {
                    ColorMuxA = GenericColorMux.G_CCMUX_1,
                    ColorMuxB = GenericColorMux.G_CCMUX_TEXEL1,
                    ColorMuxC = GenericColorMux.G_CCMUX_ENV_ALPHA,
                    ColorMuxD = GenericColorMux.G_CCMUX_TEXEL1,
                    AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
                    AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
                    AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
                    AlphaMuxD = withAlpha
                        ? GenericAlphaMux.G_ACMUX_TEXEL0
                        : GenericAlphaMux.G_ACMUX_1,
                },
        },
        new() {
            ColorMuxA = GenericColorMux.G_CCMUX_COMBINED,
            ColorMuxB = GenericColorMux.G_CCMUX_0,
            ColorMuxC = GenericColorMux.G_CCMUX_SHADE,
            ColorMuxD = GenericColorMux.G_CCMUX_0,
            AlphaMuxA = GenericAlphaMux.G_ACMUX_0,
            AlphaMuxB = GenericAlphaMux.G_ACMUX_0,
            AlphaMuxC = GenericAlphaMux.G_ACMUX_0,
            AlphaMuxD = GenericAlphaMux.G_ACMUX_COMBINED,
        });

  public static CullingMode GetCullingModeForMeshSetId(uint meshSetId)
    => meshSetId switch {
        // HACK: Disables culling for glasses
        8 => CullingMode.SHOW_BOTH,
        // HACK: Disables culling for head accessories
        10 => CullingMode.SHOW_BOTH,
        // By default, only shows front face
        _ => CullingMode.SHOW_FRONT_ONLY,
    };

  public static CullingMode GetCullingModeForChosenPartId(int chosenPartId)
    => chosenPartId switch {
        // HACK: Disables culling for hats
        3 => CullingMode.SHOW_BOTH,
        // HACK: Disables culling for shirt collars
        5 => CullingMode.SHOW_BOTH,
        // HACK: Disables culling for accessories worn on back
        6 => CullingMode.SHOW_BOTH,
        // HACK: Disables culling for shoelaces
        7 => CullingMode.SHOW_BOTH,
        // By default, only shows front face
        _ => CullingMode.SHOW_FRONT_ONLY,
    };

  public static void SetCombinerForPattern(
      IN64Hardware n64Hardware,
      ChosenPart0 chosenPart0,
      int patternI,
      bool withTexture) {
    if (!(patternI is 1 or 2 or 3)) {
      return;
    }
    
    var isMark = patternI == 3;
    SetCombiner(n64Hardware,
                 withTexture,
                 isMark,
                 OneOf<uint, Color>.FromT0(
                     patternI switch {
                         1 => chosenPart0.Pattern0Params.ImageSegmentedAddress,
                         2 => chosenPart0.Pattern1Params.ImageSegmentedAddress,
                         3 => chosenPart0.MarkParams.ImageSegmentedAddress,
                     }),
                 patternI switch {
                     1 => chosenPart0.Pattern0Params.MaterialType,
                     2 => chosenPart0.Pattern1Params.MaterialType,
                     3 => PatternMaterialType.BLEND_1X1,
                 });

    if (isMark) {
      SetAdditiveBlending(n64Hardware);
    }
  }
}