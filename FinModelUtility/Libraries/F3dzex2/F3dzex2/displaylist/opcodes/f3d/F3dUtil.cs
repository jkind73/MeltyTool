using System;

using f3dzex2.combiner;
using f3dzex2.image;

using fin.math;
using fin.math.fixedPoint;

using schema.binary;


namespace f3dzex2.displaylist.opcodes.f3d;

public static class F3dUtil {
  public static SetCombineOpcodeCommand ParseSetCombineOpcodeCommand(
      IBinaryReader br) {
    //           aaaa cccc ceee gggi iiik kkkk 
    // bbbb jjjj mmmo oodd dfff hhhl llnn nppp

    var first = br.ReadUInt24();
    var second = br.ReadUInt32();

    var a = BitLogic.ExtractFromRight(first, 20, 4);
    var c = BitLogic.ExtractFromRight(first, 15, 5);
    var e = BitLogic.ExtractFromRight(first, 12, 3);
    var g = BitLogic.ExtractFromRight(first, 9, 3);
    var i = BitLogic.ExtractFromRight(first, 5, 4);
    var k = BitLogic.ExtractFromRight(first, 0, 5);

    var b = BitLogic.ExtractFromRight(second, 28, 4);
    var j = BitLogic.ExtractFromRight(second, 24, 4);
    var m = BitLogic.ExtractFromRight(second, 21, 3);
    var o = BitLogic.ExtractFromRight(second, 18, 3);
    var d = BitLogic.ExtractFromRight(second, 15, 3);
    var f = BitLogic.ExtractFromRight(second, 12, 3);
    var h = BitLogic.ExtractFromRight(second, 9, 3);
    var l = BitLogic.ExtractFromRight(second, 6, 3);
    var n = BitLogic.ExtractFromRight(second, 3, 3);
    var p = BitLogic.ExtractFromRight(second, 0, 3);

    return new SetCombineOpcodeCommand {
        CombinerCycleParams0 = new CombinerCycleParams {
            ColorMuxA = GetColorMuxA_(a),
            ColorMuxB = GetColorMuxB_(b),
            ColorMuxC = GetColorMuxC_(c),
            ColorMuxD = GetColorMuxD_(d),
            AlphaMuxA = GetAlphaMuxABD_(e),
            AlphaMuxB = GetAlphaMuxABD_(f),
            AlphaMuxC = GetAlphaMuxC_(g),
            AlphaMuxD = GetAlphaMuxABD_(h),
        },
        CombinerCycleParams1 = new CombinerCycleParams {
            ColorMuxA = GetColorMuxA_(i),
            ColorMuxB = GetColorMuxB_(j),
            ColorMuxC = GetColorMuxC_(k),
            ColorMuxD = GetColorMuxD_(l),
            AlphaMuxA = GetAlphaMuxABD_(m),
            AlphaMuxB = GetAlphaMuxABD_(n),
            AlphaMuxC = GetAlphaMuxC_(o),
            AlphaMuxD = GetAlphaMuxABD_(p),
        }
    };
  }

  /// <summary>
  ///   http://ultra64.ca/files/documentation/online-manuals/man/pro-man/pro12/index12.6.html#:~:text=Color%20Combiner%20(CC)%20can%20perform,in%20one%2Dcycle%20mode).
  /// </summary>
  private static GenericColorMux GetColorMuxA_(uint value) => value switch {
      0 => GenericColorMux.G_CCMUX_COMBINED,
      1 => GenericColorMux.G_CCMUX_TEXEL0,
      2 => GenericColorMux.G_CCMUX_TEXEL1,
      3 => GenericColorMux.G_CCMUX_PRIMITIVE,
      4 => GenericColorMux.G_CCMUX_SHADE,
      5 => GenericColorMux.G_CCMUX_ENVIRONMENT,
      6 => GenericColorMux.G_CCMUX_1,
      7 => GenericColorMux.G_CCMUX_NOISE,
      >= 8 and <= 15 => GenericColorMux.G_CCMUX_0,
      _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
  };

  private static GenericColorMux GetColorMuxB_(uint value) => value switch {
      0 => GenericColorMux.G_CCMUX_COMBINED,
      1 => GenericColorMux.G_CCMUX_TEXEL0,
      2 => GenericColorMux.G_CCMUX_TEXEL1,
      3 => GenericColorMux.G_CCMUX_PRIMITIVE,
      4 => GenericColorMux.G_CCMUX_SHADE,
      5 => GenericColorMux.G_CCMUX_ENVIRONMENT,
      6 => GenericColorMux.G_CCMUX_CENTER,
      7 => GenericColorMux.G_CCMUX_K4,
      >= 8 and <= 15 => GenericColorMux.G_CCMUX_0,
      _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
  };

  private static GenericColorMux GetColorMuxC_(uint value) => value switch {
      0 => GenericColorMux.G_CCMUX_COMBINED,
      1 => GenericColorMux.G_CCMUX_TEXEL0,
      2 => GenericColorMux.G_CCMUX_TEXEL1,
      3 => GenericColorMux.G_CCMUX_PRIMITIVE,
      4 => GenericColorMux.G_CCMUX_SHADE,
      5 => GenericColorMux.G_CCMUX_ENVIRONMENT,
      6 => GenericColorMux.G_CCMUX_SCALE,
      7 => GenericColorMux.G_CCMUX_COMBINED_ALPHA,
      8 => GenericColorMux.G_CCMUX_TEXEL0_ALPHA,
      9 => GenericColorMux.G_CCMUX_TEXEL1_ALPHA,
      10 => GenericColorMux.G_CCMUX_PRIMITIVE_ALPHA,
      11 => GenericColorMux.G_CCMUX_SHADE_ALPHA,
      12 => GenericColorMux.G_CCMUX_ENV_ALPHA,
      13 => GenericColorMux.G_CCMUX_LOD_FRAC,
      14 => GenericColorMux.G_CCMUX_PRIM_LOD_FRAC,
      15 => GenericColorMux.G_CCMUX_K5,
      >= 16 and <= 31 => GenericColorMux.G_CCMUX_0,
      _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
  };

  private static GenericColorMux GetColorMuxD_(uint value) => value switch {
      0 => GenericColorMux.G_CCMUX_COMBINED,
      1 => GenericColorMux.G_CCMUX_TEXEL0,
      2 => GenericColorMux.G_CCMUX_TEXEL1,
      3 => GenericColorMux.G_CCMUX_PRIMITIVE,
      4 => GenericColorMux.G_CCMUX_SHADE,
      5 => GenericColorMux.G_CCMUX_ENVIRONMENT,
      6 => GenericColorMux.G_CCMUX_1,
      7 => GenericColorMux.G_CCMUX_0,
      _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
  };

  private static GenericAlphaMux GetAlphaMuxABD_(uint value) => value switch {
      0 => GenericAlphaMux.G_ACMUX_COMBINED,
      1 => GenericAlphaMux.G_ACMUX_TEXEL0,
      2 => GenericAlphaMux.G_ACMUX_TEXEL1,
      3 => GenericAlphaMux.G_ACMUX_PRIMITIVE,
      4 => GenericAlphaMux.G_ACMUX_SHADE,
      5 => GenericAlphaMux.G_ACMUX_ENVIRONMENT,
      6 => GenericAlphaMux.G_ACMUX_1,
      7 => GenericAlphaMux.G_ACMUX_0,
      _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
  };

  private static GenericAlphaMux GetAlphaMuxC_(uint value) => value switch {
      0 => GenericAlphaMux.G_ACMUX_LOD_FRACTION,
      1 => GenericAlphaMux.G_ACMUX_TEXEL0,
      2 => GenericAlphaMux.G_ACMUX_TEXEL1,
      3 => GenericAlphaMux.G_ACMUX_PRIMITIVE,
      4 => GenericAlphaMux.G_ACMUX_SHADE,
      5 => GenericAlphaMux.G_ACMUX_ENVIRONMENT,
      6 => GenericAlphaMux.G_ACMUX_PRIM_LOD_FRAC,
      7 => GenericAlphaMux.G_ACMUX_0,
      _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
  };

  public static LoadBlockOpcodeCommand ParseLoadBlockOpcodeCommand(
      IBinaryReader br) {
    br.Position -= 1;

    var first = br.ReadUInt32();
    var second = br.ReadUInt32();

    var uls = FixedPointFloatUtil.Convert16(
        (ushort) first.ExtractFromRight(12, 12),
        false,
        10,
        2);
    var ult = FixedPointFloatUtil.Convert16(
        (ushort) first.ExtractFromRight(0, 12),
        false,
        10,
        2);

    var tileDescriptor =
        (TileDescriptorIndex) second.ExtractFromRight(24, 4);
    var texels = (ushort) second.ExtractFromRight(12, 12);
    var dxt = (ushort) second.ExtractFromRight(0, 12);

    return new LoadBlockOpcodeCommand {
        TileDescriptorIndex = tileDescriptor,
        Uls = uls,
        Ult = ult,
        Texels = texels,
        Dxt = dxt,
    };
  }

  public static SetTileOpcodeCommand ParseSetTileOpcodeCommand(
      IBinaryReader br) {
    br.Position -= 1;
    var first = br.ReadUInt32();
    var second = br.ReadUInt32();

    var colorFormat =
        (N64ColorFormat) BitLogic.ExtractFromRight(first, 21, 3);
    var bitSize =
        (BitsPerTexel) BitLogic.ExtractFromRight(first, 19, 2);
    var num64BitValuesPerRow =
        (ushort) BitLogic.ExtractFromRight(first, 9, 9);
    var offsetOfTextureInTmem =
        (ushort) BitLogic.ExtractFromRight(first, 0, 9);

    var tileDescriptor =
        (TileDescriptorIndex) BitLogic.ExtractFromRight(second, 24, 3);
    var palette = (ushort) BitLogic.ExtractFromRight(second, 20, 4);

    var wrapModeT = (F3dWrapMode) second.ExtractFromRight(18, 2);
    var maskT = (ushort) second.ExtractFromRight(14, 4);
    var shiftT = (ushort) second.ExtractFromRight(10, 4);

    var wrapModeS = (F3dWrapMode) second.ExtractFromRight(8, 2);
    var maskS = (ushort) second.ExtractFromRight(4, 4);
    var shiftS = (ushort) second.ExtractFromRight(0, 4);

    return new SetTileOpcodeCommand {
        TileDescriptorIndex = tileDescriptor,
        ColorFormat = colorFormat,
        BitsPerTexel = bitSize,
        Palette = palette,
        WrapModeS = wrapModeS,
        MaskS = maskS,
        ShiftS = shiftS,
        WrapModeT = wrapModeT,
        MaskT = maskT,
        ShiftT = shiftT,
        Num64BitValuesPerRow = num64BitValuesPerRow,
        OffsetOfTextureInTmem = offsetOfTextureInTmem,
    };
  }
}