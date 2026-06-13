using System;

using f3dzex2.displaylist.opcodes.f3d;
using f3dzex2.image;
using f3dzex2.io;
using f3dzex2.model;

using fin.math;
using fin.math.fixedPoint;
using fin.schema;

using schema.binary;


namespace f3dzex2.displaylist.opcodes.f3dzex2;

public sealed class F3dzex2OpcodeParser : IOpcodeParser {
  public IOpcodeCommand Parse(IReadOnlyN64Memory n64Memory,
                              IDisplayListReader dlr,
                              SchemaBinaryReader br) {
    var baseOffset = br.Position;
    var opcode = (F3dzex2Opcode) br.ReadByte();
    var opcodeCommand = this.ParseOpcodeCommand_(n64Memory, dlr, br, opcode);
    br.Position = baseOffset + this.GetCommandLength_(opcode);
    return opcodeCommand;
  }

  public DisplayListType Type => DisplayListType.F3DZEX2;

  [Unknown]
  private IOpcodeCommand ParseOpcodeCommand_(IReadOnlyN64Memory n64Memory,
                                             IDisplayListReader dlr,
                                             SchemaBinaryReader br,
                                             F3dzex2Opcode opcode) {
    switch (opcode) {
      case F3dzex2Opcode.G_NOOP:
        return new NoopOpcodeCommand(opcode.ToString());
      case F3dzex2Opcode.G_DL: {
        var storeReturnAddress = br.ReadByte() == 0;
        br.AssertUInt16(0);
        var address = br.ReadUInt32();
        return new DlOpcodeCommand {
            SegmentedAddress = address,
            PossibleBranches =
                dlr.ReadPossibleDisplayLists(n64Memory, this, address),
            PushCurrentDlToStack = storeReturnAddress
        };
      }
      case F3dzex2Opcode.G_ENDDL:
        return new EndDlOpcodeCommand();
      case F3dzex2Opcode.G_VTX: {
        var numVerticesToLoad = (byte) (br.ReadUInt16() >> 4);
        var indexToBeginStoringVertices =
            (byte) ((br.ReadByte() >> 1) - numVerticesToLoad);
        using var sbr = n64Memory.OpenAtSegmentedAddress(br.ReadUInt32());
        return new VtxOpcodeCommand {
            IndexToBeginStoringVertices = indexToBeginStoringVertices,
            Vertices = sbr.ReadNews<F3dVertex>(numVerticesToLoad),
        };
      }
      case F3dzex2Opcode.G_TRI1:
        return F3dzex2Util.ParseTri1OpcodeCommand(br);
      case F3dzex2Opcode.G_TRI2: 
        return F3dzex2Util.ParseTri2OpcodeCommand(br);
      case F3dzex2Opcode.G_TEXTURE: {
        br.AssertByte(0);

        var mipmapLevelsAndTileDescriptor = br.ReadByte();
        var tileDescriptor =
            (TileDescriptorIndex) BitLogic.ExtractFromRight(
                mipmapLevelsAndTileDescriptor,
                0,
                3);
        var maximumNumberOfMipmapsMinus1 =
            (byte) BitLogic.ExtractFromRight(mipmapLevelsAndTileDescriptor,
                                             3,
                                             3);
        var newTileDescriptorState =
            (TileDescriptorState) (br.ReadByte() >> 1);
        var horizontalScale = br.ReadUInt16();
        var verticalScale = br.ReadUInt16();

        return new TextureOpcodeCommand {
            TileDescriptorIndex = tileDescriptor,
            NewTileDescriptorState = newTileDescriptorState,
            HorizontalScaling = horizontalScale,
            VerticalScaling = verticalScale,
            MaximumNumberOfMipmapsMinus1 = maximumNumberOfMipmapsMinus1,
        };
      }
      case F3dzex2Opcode.G_POPMTX: {
        br.AssertByte(0x38);
        br.AssertByte(0);
        br.AssertByte(2);

        var numMatrices = br.ReadUInt32() / 64;
        return new PopMtxOpcodeCommand {NumberOfMatrices = numMatrices};
      }
      case F3dzex2Opcode.G_SETCOMBINE: {
        return F3dUtil.ParseSetCombineOpcodeCommand(br);
      }
      case F3dzex2Opcode.G_SETTIMG: {
        N64ImageParser.SplitN64ImageFormat(br.ReadByte(),
                                           out var colorFormat,
                                           out var bitSize);

        var args = br.ReadUInt16();
        var width = (ushort) (args + 1);

        return new SetTimgOpcodeCommand {
            ColorFormat = colorFormat,
            BitsPerTexel = bitSize,
            Width = width,
            TextureSegmentedAddress = br.ReadUInt32(),
        };
      }
      case F3dzex2Opcode.G_SETTILE:
        return F3dUtil.ParseSetTileOpcodeCommand(br);
      case F3dzex2Opcode.G_SETPRIMCOLOR: {
        var lodInfo = br.ReadUInt24();
        return new SetPrimColorOpcodeCommand {
            R = br.ReadByte(),
            G = br.ReadByte(),
            B = br.ReadByte(),
            A = br.ReadByte(),
        };
      }
      case F3dzex2Opcode.G_SETENVCOLOR:
        br.Position += 3;
        return new SetEnvColorOpcodeCommand {
            R = br.ReadByte(),
            G = br.ReadByte(),
            B = br.ReadByte(),
            A = br.ReadByte(),
        };
      case F3dzex2Opcode.G_SETTILESIZE: {
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

        var tileDescriptor
            = (TileDescriptorIndex) second.ExtractFromRight(24, 4);
        var lrs = FixedPointFloatUtil.Convert16(
            (ushort) second.ExtractFromRight(12, 12),
            false,
            10,
            2);
        var lrt = FixedPointFloatUtil.Convert16(
            (ushort) second.ExtractFromRight(0, 12),
            false,
            10,
            2);

        return new SetTileSizeOpcodeCommand {
            TileDescriptorIndex = tileDescriptor,
            Uls = uls,
            Ult = ult,
            Lrs = lrs,
            Lrt = lrt,
        };
      }
      case F3dzex2Opcode.G_LOADBLOCK:
        return F3dUtil.ParseLoadBlockOpcodeCommand(br);
      case F3dzex2Opcode.G_GEOMETRYMODE: {
        return new GeometryModeOpcodeCommand {
            FlagsToDisable = (GeometryMode) (~br.ReadUInt24() & 0xFFFFFF),
            FlagsToEnable = (GeometryMode) br.ReadUInt32(),
        };
      }
      case F3dzex2Opcode.G_MTX: {
        br.AssertUInt16(0x3800);
        var mtxParams = (byte) (br.ReadByte() ^ 0x01);
        var address = br.ReadUInt32();
        return new MtxOpcodeCommand {
            Params = mtxParams, RamAddress = address,
        };
      }
      case F3dzex2Opcode.G_LOADTLUT: {
        return F3dzex2Util.ParseLoadTlutOpcodeCommand(br);
      }
      case F3dzex2Opcode.G_MODIFYVTX:
        return new ModifyVtxOpcodeCommand();
      case F3dzex2Opcode.G_SETOTHERMODE_L:
        return F3dzex2Util.ParseSetOtherModeLOpcodeCommand(br);
      case F3dzex2Opcode.G_SETOTHERMODE_H:
        return F3dzex2Util.ParseSetOtherModeHOpcodeCommand(br);
      case F3dzex2Opcode.G_TEXRECT:
      case F3dzex2Opcode.G_TEXRECTFLIP: {
        br.Position -= 1;

        var first = br.ReadUInt32();
        var second = br.ReadUInt32();

        return new NoopOpcodeCommand(opcode.ToString());
      }
      case F3dzex2Opcode.G_LOADTILE: {
        var first = br.ReadUInt24();
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
        var lrs = FixedPointFloatUtil.Convert16(
            (ushort) second.ExtractFromRight(12, 12),
            false,
            10,
            2);
        var lrt = FixedPointFloatUtil.Convert16(
            (ushort) second.ExtractFromRight(0, 12),
            false,
            10,
            2);

        return new LoadTileOpcodeCommand {
            TileDescriptorIndex = tileDescriptor,
            Uls = uls,
            Ult = ult,
            Lrs = lrs,
            Lrt = lrt,
        };
      }
      // TODO: Especially implement these
      case F3dzex2Opcode.G_SETCIMG:
      case F3dzex2Opcode.G_SETZIMG:
        return new NoopOpcodeCommand(opcode.ToString());
      // TODO: Implement these
      case F3dzex2Opcode.G_CULLDL:
      case F3dzex2Opcode.G_BRANCH_Z:
        return new NoopOpcodeCommand(opcode.ToString());
      case F3dzex2Opcode.G_RDPPIPESYNC:
      case F3dzex2Opcode.G_RDPTILESYNC:
      case F3dzex2Opcode.G_RDPFULLSYNC:
      case F3dzex2Opcode.G_RDPLOADSYNC:
        return new NoopOpcodeCommand(opcode.ToString());

      default:
        throw new ArgumentOutOfRangeException(nameof(opcode), opcode, null);
    }
  }

  private int GetCommandLength_(F3dzex2Opcode opcode) {
    switch (opcode) {
      case F3dzex2Opcode.G_NOOP:
      case F3dzex2Opcode.G_VTX:
      case F3dzex2Opcode.G_MODIFYVTX:
      case F3dzex2Opcode.G_CULLDL:
      case F3dzex2Opcode.G_TRI1:
      case F3dzex2Opcode.G_TRI2:
      case F3dzex2Opcode.G_QUAD:
      case F3dzex2Opcode.G_SPECIAL_3:
      case F3dzex2Opcode.G_SPECIAL_2:
      case F3dzex2Opcode.G_SPECIAL_1:
      case F3dzex2Opcode.G_DMA_IO:
      case F3dzex2Opcode.G_TEXTURE:
      case F3dzex2Opcode.G_POPMTX:
      case F3dzex2Opcode.G_GEOMETRYMODE:
      case F3dzex2Opcode.G_MTX:
      case F3dzex2Opcode.G_MOVEWORD:
      case F3dzex2Opcode.G_MOVEMEM:
      case F3dzex2Opcode.G_DL:
      case F3dzex2Opcode.G_ENDDL:
      case F3dzex2Opcode.G_SPNOOP:
      case F3dzex2Opcode.G_RDPHALF_1:
      case F3dzex2Opcode.G_SETOTHERMODE_L:
      case F3dzex2Opcode.G_SETOTHERMODE_H:
      case F3dzex2Opcode.G_RDPLOADSYNC:
      case F3dzex2Opcode.G_RDPPIPESYNC:
      case F3dzex2Opcode.G_RDPTILESYNC:
      case F3dzex2Opcode.G_RDPFULLSYNC:
      case F3dzex2Opcode.G_SETKEYGB:
      case F3dzex2Opcode.G_SETKEYR:
      case F3dzex2Opcode.G_SETSCISSOR:
      case F3dzex2Opcode.G_SETPRIMDEPTH:
      case F3dzex2Opcode.G_RDPSETOTHERMODE:
      case F3dzex2Opcode.G_LOADTLUT:
      case F3dzex2Opcode.G_RDPHALF_2:
      case F3dzex2Opcode.G_SETTILESIZE:
      case F3dzex2Opcode.G_LOADBLOCK:
      case F3dzex2Opcode.G_LOADTILE:
      case F3dzex2Opcode.G_FILLRECT:
      case F3dzex2Opcode.G_SETFILLCOLOR:
      case F3dzex2Opcode.G_SETFOGCOLOR:
      case F3dzex2Opcode.G_SETBLENDCOLOR:
      case F3dzex2Opcode.G_SETPRIMCOLOR:
      case F3dzex2Opcode.G_SETENVCOLOR:
      case F3dzex2Opcode.G_SETTIMG:
      case F3dzex2Opcode.G_SETZIMG:
      case F3dzex2Opcode.G_SETCIMG:
      case F3dzex2Opcode.G_SETTILE:
      case F3dzex2Opcode.G_SETCOMBINE:
        return 1 * 2 * 4;
      case F3dzex2Opcode.G_BRANCH_Z:
      case F3dzex2Opcode.G_LOAD_UCODE:
      case F3dzex2Opcode.G_SETCONVERT:
        return 2 * 2 * 4;
      case F3dzex2Opcode.G_TEXRECT:
      case F3dzex2Opcode.G_TEXRECTFLIP:
        return 3 * 2 * 4;
      default:
        throw new ArgumentOutOfRangeException(nameof(opcode), opcode, null);
    }
  }
}