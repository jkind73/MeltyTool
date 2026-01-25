using System;

using f3dzex2.image;
using f3dzex2.io;
using f3dzex2.model;

using fin.math;

using schema.binary;


namespace f3dzex2.displaylist.opcodes.f3d;

public sealed class F3dOpcodeParser : IOpcodeParser {
  public IOpcodeCommand Parse(IReadOnlyN64Memory n64Memory,
                              IDisplayListReader dlr,
                              SchemaBinaryReader br) {
    var baseOffset = br.Position;
    var opcode = (F3dOpcode) br.ReadByte();
    var opcodeCommand = ParseOpcodeCommand_(n64Memory, dlr, br, opcode);
    br.Position = baseOffset + GetCommandLength_(opcode);
    return opcodeCommand;
  }

  public DisplayListType Type => DisplayListType.FAST_3D;

  private IOpcodeCommand ParseOpcodeCommand_(IReadOnlyN64Memory n64Memory,
                                             IDisplayListReader dlr,
                                             SchemaBinaryReader br,
                                             F3dOpcode opcode) {
    switch (opcode) {
      case F3dOpcode.G_MTX: {
        var mtxParams = br.ReadByte();
        br.AssertUInt16(0);
        var address = br.ReadUInt32();
        return new MtxOpcodeCommand {
            Params = mtxParams, RamAddress = address,
        };
      }
      case F3dOpcode.G_POPMTX: {
        br.AssertUInt24(0);
        br.AssertUInt32(0);
        return new PopMtxOpcodeCommand {NumberOfMatrices = 1};
      }
      case F3dOpcode.G_VTX: {
        var numVerticesMinusOneAndWriteOffset = br.ReadByte();
        var numVertices = BitLogic.ExtractFromRight(
            numVerticesMinusOneAndWriteOffset,
            4,
            4) + 1;
        var writeOffset = BitLogic.ExtractFromRight(
            numVerticesMinusOneAndWriteOffset,
            0,
            4);
        br.AssertUInt16((ushort) (numVertices * 0x10));

        var segmentedAddress = br.ReadUInt32();
        using var sbr = n64Memory.OpenAtSegmentedAddress(segmentedAddress);

        return new VtxOpcodeCommand {
            Vertices = sbr.ReadNews<F3dVertex>((int) numVertices),
            IndexToBeginStoringVertices = (byte) writeOffset,
        };
      }
      case F3dOpcode.G_DL: {
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
      case F3dOpcode.G_ENDDL: {
        br.AssertUInt24(0);
        br.AssertUInt32(0);
        return new EndDlOpcodeCommand();
      }
      case F3dOpcode.G_TRI1: {
        br.AssertUInt32(0);
        return new Tri1OpcodeCommand {
            VertexOrder = TriVertexOrder.ABC,
            VertexIndexA = (byte) (br.ReadByte() / 0xA),
            VertexIndexB = (byte) (br.ReadByte() / 0xA),
            VertexIndexC = (byte) (br.ReadByte() / 0xA),
        };
      }
      case F3dOpcode.G_SETENVCOLOR: {
        br.AssertUInt24(0);
        return new SetEnvColorOpcodeCommand {
            R = br.ReadByte(),
            G = br.ReadByte(),
            B = br.ReadByte(),
            A = br.ReadByte(),
        };
      }
      case F3dOpcode.G_SETFOGCOLOR: {
        br.AssertUInt24(0);
        return new SetFogColorOpcodeCommand {
            R = br.ReadByte(),
            G = br.ReadByte(),
            B = br.ReadByte(),
            A = br.ReadByte(),
        };
      }
      case F3dOpcode.G_SETTIMG: {
        N64ImageParser.SplitN64ImageFormat(br.ReadByte(),
                                           out var colorFormat,
                                           out var bitSize);
        br.AssertUInt16(0);
        return new SetTimgOpcodeCommand {
            ColorFormat = colorFormat,
            BitsPerTexel = bitSize,
            TextureSegmentedAddress = br.ReadUInt32(),
        };
      }
      case F3dOpcode.G_SETGEOMETRYMODE: {
        br.AssertUInt24(0);
        return new SetGeometryModeOpcodeCommand {
            FlagsToEnable = (GeometryMode) br.ReadUInt32()
        };
      }
      case F3dOpcode.G_CLEARGEOMETRYMODE: {
        br.AssertUInt24(0);
        return new ClearGeometryModeOpcodeCommand {
            FlagsToDisable = (GeometryMode) br.ReadUInt32()
        };
      }
      case F3dOpcode.G_TEXTURE: {
        br.AssertByte(0);

        var mipmapLevelsAndTileDescriptor = br.ReadByte();
        var tileDescriptor =
            (TileDescriptorIndex) BitLogic.ExtractFromRight(
                mipmapLevelsAndTileDescriptor,
                0,
                3);
        var maximumNumberOfMipmaps =
            (byte) BitLogic.ExtractFromRight(mipmapLevelsAndTileDescriptor,
                                             3,
                                             3);
        var newTileDescriptorState = (TileDescriptorState) br.ReadByte();
        var horizontalScale = br.ReadUInt16();
        var verticalScale = br.ReadUInt16();

        return new TextureOpcodeCommand {
            TileDescriptorIndex = tileDescriptor,
            NewTileDescriptorState = newTileDescriptorState,
            HorizontalScaling = horizontalScale,
            VerticalScaling = verticalScale,
            MaximumNumberOfMipmaps = maximumNumberOfMipmaps,
        };
      }
      case F3dOpcode.G_SETTILE:
        return F3dUtil.ParseSetTileOpcodeCommand(br);
      case F3dOpcode.G_SETTILESIZE: {
        br.Position--;

        var first = br.ReadUInt32();
        var second = br.ReadUInt32();

        var ul = TmemUtil.ParseCoordAxes(first);

        var tileDescriptor
            = (TileDescriptorIndex) second.ExtractFromRight(24, 4);
        var lr = TmemUtil.ParseCoordAxes(second);

        return new SetTileSizeOpcodeCommand {
            TileDescriptorIndex = tileDescriptor,
            Uls = ul.X,
            Ult = ul.Y,
            Lrs = lr.X,
            Lrt = lr.Y,
        };
      }
      case F3dOpcode.G_SETCOMBINE: {
        return F3dUtil.ParseSetCombineOpcodeCommand(br);
      }
      case F3dOpcode.G_LOADBLOCK:
        return F3dUtil.ParseLoadBlockOpcodeCommand(br);
      case F3dOpcode.G_MOVEMEM: {
        var commandType = (DmemAddress) br.ReadByte();
        var sizeInBytes = br.ReadUInt16();
        var segmentedAddress = br.ReadUInt32();

        return new MoveMemOpcodeCommand {
            DmemAddress = commandType, SegmentedAddress = segmentedAddress,
        };
      }
      // TODO: Implement these
      case F3dOpcode.G_MOVEWORD:
      case F3dOpcode.G_SETOTHERMODE_L:
      case F3dOpcode.G_SETOTHERMODE_H:
        return new NoopOpcodeCommand();
      case F3dOpcode.G_RDPLOADSYNC:
      case F3dOpcode.G_RDPPIPESYNC:
      case F3dOpcode.G_RDPTILESYNC:
      case F3dOpcode.G_RDPFULLSYNC:
      case F3dOpcode.G_SPNOOP:
      case F3dOpcode.G_NOOP: {
        br.AssertUInt24(0);
        br.AssertUInt32(0);
        return new NoopOpcodeCommand();
      }
      default:
        throw new ArgumentOutOfRangeException(nameof(opcode), opcode, null);
    }
  }

  private int GetCommandLength_(F3dOpcode opcode) {
    switch (opcode) {
      case F3dOpcode.G_SPNOOP:
      case F3dOpcode.G_MTX:
      case F3dOpcode.G_MOVEMEM:
      case F3dOpcode.G_VTX:
      case F3dOpcode.G_DL:
      case F3dOpcode.G_RDPHALF_CONT:
      case F3dOpcode.G_RDPHALF_2:
      case F3dOpcode.G_RDPHALF_1:
      case F3dOpcode.G_CLEARGEOMETRYMODE:
      case F3dOpcode.G_SETGEOMETRYMODE:
      case F3dOpcode.G_ENDDL:
      case F3dOpcode.G_SETOTHERMODE_H:
      case F3dOpcode.G_SETOTHERMODE_L:
      case F3dOpcode.G_TEXTURE:
      case F3dOpcode.G_MOVEWORD:
      case F3dOpcode.G_POPMTX:
      case F3dOpcode.G_CULLDL:
      case F3dOpcode.G_TRI1:
      case F3dOpcode.G_NOOP:
      case F3dOpcode.G_RDPLOADSYNC:
      case F3dOpcode.G_RDPPIPESYNC:
      case F3dOpcode.G_RDPTILESYNC:
      case F3dOpcode.G_RDPFULLSYNC:
      case F3dOpcode.G_SETKEYGB:
      case F3dOpcode.G_SETKEYR:
      case F3dOpcode.G_SETSCISSOR:
      case F3dOpcode.G_SETPRIMDEPTH:
      case F3dOpcode.G_RDPSETOTHERMODE:
      case F3dOpcode.G_LOADTLUT:
      case F3dOpcode.G_SETTILESIZE:
      case F3dOpcode.G_LOADBLOCK:
      case F3dOpcode.G_LOADTILE:
      case F3dOpcode.G_SETTILE:
      case F3dOpcode.G_FILLRECT:
      case F3dOpcode.G_SETFILLCOLOR:
      case F3dOpcode.G_SETFOGCOLOR:
      case F3dOpcode.G_SETBLENDCOLOR:
      case F3dOpcode.G_SETPRIMCOLOR:
      case F3dOpcode.G_SETENVCOLOR:
      case F3dOpcode.G_SETCOMBINE:
      case F3dOpcode.G_SETTIMG:
      case F3dOpcode.G_SETZIMG:
      case F3dOpcode.G_SETCIMG:
        return 1 * 2 * 4;
      case F3dOpcode.G_TEXRECT:
      case F3dOpcode.G_TEXRECTFLIP:
        return 3 * 2 * 4;
      default:
        throw new ArgumentOutOfRangeException(nameof(opcode), opcode, null);
    }
  }
}