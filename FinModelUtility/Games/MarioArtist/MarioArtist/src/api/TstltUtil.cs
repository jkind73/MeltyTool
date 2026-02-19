using System.Drawing;
using System.Numerics;

using f3dzex2.combiner;
using f3dzex2.displaylist;
using f3dzex2.displaylist.opcodes;
using f3dzex2.displaylist.opcodes.f3dzex2;
using f3dzex2.image;
using f3dzex2.io;
using f3dzex2.model;

using fin.math;
using fin.model;
using fin.util.enumerables;

using marioartist.schema.talent_studio;

using OneOf;


namespace marioartist.api;

public static class TstltUtil {
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

  public static IMesh? AddDisplayLists(
      IModel model,
      Segment segment,
      N64Hardware<N64Memory> n64Hardware,
      DlModelBuilder dlModelBuilder,
      string meshName,
      bool isLeft,
      IEnumerable<(uint, Matrix4x4?, IBoneWeights)>
          displayListSegmentedOffsetAndBones) {
    n64Hardware.Memory.SetSegment(0xF, segment);

    var displayListReader = new DisplayListReader();
    var f3dzex2OpcodeParser = new F3dzex2OpcodeParser();
    var displayListTuples = displayListSegmentedOffsetAndBones
                            .Select((t, i) => {
                              try {
                                var displayList
                                    = displayListReader.ReadDisplayList(
                                        n64Hardware.Memory,
                                        f3dzex2OpcodeParser,
                                        t.Item1);
                                return (displayList, t.Item2, t.Item3) as
                                    (IDisplayList, Matrix4x4?, IBoneWeights)?;
                              } catch (Exception e) {
                                return null;
                              }
                            })
                            .WhereNonnull()
                            .Select(t => t!.Value)
                            .ToArray();

    if (!displayListTuples.Any()) {
      return null;
    }

    var mesh = dlModelBuilder.StartNewMesh(meshName);

    var rsp = n64Hardware.Rsp;
    foreach (var (displayList, matrix, boneWeights) in displayListTuples) {
      dlModelBuilder.Matrix = matrix ?? Matrix4x4.Identity;

      rsp.ActiveBoneWeights = boneWeights;

      try {
        dlModelBuilder.AddDl(displayList);
      } catch (Exception e) {
        ;
      }
    }

    dlModelBuilder.Matrix = Matrix4x4.Identity;

    foreach (var p in mesh.Primitives) {
      p.SetVertexOrder(isLeft
                           ? VertexOrder.CLOCKWISE
                           : VertexOrder.COUNTER_CLOCKWISE);
    }

    return mesh;
  }

  public static void SetCombiner(
      IN64Hardware<N64Memory> n64Hardware,
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
                         TileDescriptorIndex.TX_LOADTILE,
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

  public static CullingMode GetCullingModeForChosenPartId(uint chosenPartId)
    => chosenPartId switch {
        // HACK: Disables culling for hats
        3 => CullingMode.SHOW_BOTH,
        // HACK: Disables culling for accessories worn on back
        6 => CullingMode.SHOW_BOTH,
        // By default, only shows front face
        _ => CullingMode.SHOW_FRONT_ONLY,
    };
}