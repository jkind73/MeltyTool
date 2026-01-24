using schema.binary;

namespace sm64ds.schema.gx;

public enum PolygonType {
  TRIANGLES,
  QUADS,
  TRIANGLE_STRIP,
  QUAD_STRIP,
}

public sealed class OpcodeReader {
  public static IOpcode[] ReadOpcodes(IBinaryReader br) {
    var opcodes = new LinkedList<IOpcode>();

    while (!br.Eof) {
      var opcode0 = (OpcodeType) br.ReadByte();
      var opcode1 = (OpcodeType) br.ReadByte();
      var opcode2 = (OpcodeType) br.ReadByte();
      var opcode3 = (OpcodeType) br.ReadByte();

      opcodes.AddLast(ReadOpcode_(br, opcode0));
      opcodes.AddLast(ReadOpcode_(br, opcode1));
      opcodes.AddLast(ReadOpcode_(br, opcode2));
      opcodes.AddLast(ReadOpcode_(br, opcode3));
    }

    return opcodes.ToArray();
  }

  private static IOpcode ReadOpcode_(IBinaryReader br, OpcodeType type) {
    switch (type) {
      case OpcodeType.NOOP: {
        return br.ReadNew<NoopOpcode>();
      }
      case OpcodeType.MATRIX_RESTORE: {
        return br.ReadNew<MatrixRestoreOpcode>();
      }
      case OpcodeType.COLOR: {
        return br.ReadNew<ColorOpcode>();
      }
      case OpcodeType.NORMAL: {
        return br.ReadNew<NormalOpcode>();
      }
      case OpcodeType.TEXCOORD: {
        return br.ReadNew<TexCoordOpcode>();
      }
      case OpcodeType.VERTEX_0_X23: {
        return br.ReadNew<Vertex0X23Opcode>();
      }
      case OpcodeType.VERTEX_0_X24: {
        return br.ReadNew<Vertex0X24Opcode>();
      }
      case OpcodeType.VERTEX_0_X25: {
        return br.ReadNew<Vertex0X25Opcode>();
      }
      case OpcodeType.VERTEX_0_X26: {
        return br.ReadNew<Vertex0X26Opcode>();
      }
      case OpcodeType.VERTEX_0_X27: {
        return br.ReadNew<Vertex0X27Opcode>();
      }
      case OpcodeType.VERTEX_0_X28: {
        return br.ReadNew<Vertex0X28Opcode>();
      }
      case OpcodeType.BEGIN_VERTEX_LIST: {
        return br.ReadNew<BeginVertexListOpcode>();
      }
      case OpcodeType.END_VERTEX_LIST: {
        return br.ReadNew<EndVertexListOpcode>();
      }
      case OpcodeType.MATRIX_UNK_0_X10:
      case OpcodeType.MATRIX_UNK_0_X11:
      case OpcodeType.MATRIX_UNK_0_X12:
      case OpcodeType.MATRIX_UNK_0_X13:
      case OpcodeType.MATRIX_UNK_0_X15:
      case OpcodeType.MATRIX_UNK_0_X16:
      case OpcodeType.MATRIX_UNK_0_X17:
      case OpcodeType.MATRIX_UNK_0_X18:
      case OpcodeType.MATRIX_UNK_0_X19:
      case OpcodeType.MATRIX_UNK_0_X1_A:
      case OpcodeType.MATRIX_UNK_0_X1_B:
      case OpcodeType.MATRIX_UNK_0_X1_C:
      case OpcodeType.UNK_0_X29:
      case OpcodeType.UNK_0_X2_A:
      case OpcodeType.UNK_0_X2_B:
      case OpcodeType.LIGHTING_UNK_0_X30:
      case OpcodeType.LIGHTING_UNK_0_X31:
      case OpcodeType.LIGHTING_UNK_0_X32:
      case OpcodeType.LIGHTING_UNK_0_X33:
      case OpcodeType.LIGHTING_UNK_0_X34:
      case OpcodeType.UNK_0_X50:
      case OpcodeType.UNK_0_X60:
      case OpcodeType.UNK_0_X70:
      case OpcodeType.UNK_0_X71:
      case OpcodeType.UNK_0_X72: {
        var opcode = new UnhandledOpcode(type);
        opcode.Read(br);
        return opcode;
      }
      default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
    }
  }
}