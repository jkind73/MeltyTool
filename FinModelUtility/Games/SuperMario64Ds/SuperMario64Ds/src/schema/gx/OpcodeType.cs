namespace sm64ds.schema.gx;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Arisotura/SM64DSe/blob/master/SM64DSFormats/BMD.cs
/// </summary>
public enum OpcodeType : byte {
  NOOP = 0x00,

  MATRIX_UNK_0_X10 = 0x10,
  MATRIX_UNK_0_X11 = 0x11,
  MATRIX_UNK_0_X12 = 0x12,
  MATRIX_UNK_0_X13 = 0x13,

  MATRIX_RESTORE = 0x14,

  MATRIX_UNK_0_X15 = 0x15,
  MATRIX_UNK_0_X16 = 0x16,
  MATRIX_UNK_0_X17 = 0x17,
  MATRIX_UNK_0_X18 = 0x18,
  MATRIX_UNK_0_X19 = 0x19,
  MATRIX_UNK_0_X1_A = 0x1A,
  MATRIX_UNK_0_X1_B = 0x1B,
  MATRIX_UNK_0_X1_C = 0x1C,

  COLOR = 0x20,
  NORMAL = 0x21,
  TEXCOORD = 0x22,

  VERTEX_0_X23 = 0x23,
  VERTEX_0_X24 = 0x24,
  VERTEX_0_X25 = 0x25,
  VERTEX_0_X26 = 0x26,
  VERTEX_0_X27 = 0x27,
  VERTEX_0_X28 = 0x28,

  UNK_0_X29 = 0x29,
  UNK_0_X2_A = 0x2A,
  UNK_0_X2_B = 0x2B,

  LIGHTING_UNK_0_X30 = 0x30,
  LIGHTING_UNK_0_X31 = 0x31,
  LIGHTING_UNK_0_X32 = 0x32,
  LIGHTING_UNK_0_X33 = 0x33,
  LIGHTING_UNK_0_X34 = 0x34,

  BEGIN_VERTEX_LIST = 0x40,
  END_VERTEX_LIST = 0x41,

  UNK_0_X50 = 0x50,
  UNK_0_X60 = 0x60,
  UNK_0_X70 = 0x70,
  UNK_0_X71 = 0x71,
  UNK_0_X72 = 0x72,
}

public static class OpcodeTypeExtensions {
  public static int GetLength(this OpcodeType opcode)
    => opcode switch {
        OpcodeType.NOOP => 0,
        
        OpcodeType.MATRIX_UNK_0_X10 => 4,
        OpcodeType.MATRIX_UNK_0_X11 => 0,
        OpcodeType.MATRIX_UNK_0_X12 => 4,
        OpcodeType.MATRIX_UNK_0_X13 => 4,

        OpcodeType.MATRIX_RESTORE => 4,

        OpcodeType.MATRIX_UNK_0_X15 => 0,
        OpcodeType.MATRIX_UNK_0_X16 => 64,
        OpcodeType.MATRIX_UNK_0_X17 => 48,
        OpcodeType.MATRIX_UNK_0_X18 => 64,
        OpcodeType.MATRIX_UNK_0_X19 => 48,
        OpcodeType.MATRIX_UNK_0_X1_A => 36,
        OpcodeType.MATRIX_UNK_0_X1_B => 12,
        OpcodeType.MATRIX_UNK_0_X1_C => 12,

        OpcodeType.COLOR => 4,
        OpcodeType.NORMAL => 4,
        OpcodeType.TEXCOORD => 4,

        OpcodeType.VERTEX_0_X23 => 8,
        OpcodeType.VERTEX_0_X24 => 4,
        OpcodeType.VERTEX_0_X25 => 4,
        OpcodeType.VERTEX_0_X26 => 4,
        OpcodeType.VERTEX_0_X27 => 4,
        OpcodeType.VERTEX_0_X28 => 4,

        OpcodeType.UNK_0_X29 => 4,
        OpcodeType.UNK_0_X2_A => 4,
        OpcodeType.UNK_0_X2_B => 4,

        OpcodeType.LIGHTING_UNK_0_X30 => 4,
        OpcodeType.LIGHTING_UNK_0_X31 => 4,
        OpcodeType.LIGHTING_UNK_0_X32 => 4,
        OpcodeType.LIGHTING_UNK_0_X33 => 4,
        OpcodeType.LIGHTING_UNK_0_X34 => 128,

        OpcodeType.BEGIN_VERTEX_LIST => 4,
        OpcodeType.END_VERTEX_LIST => 0,

        OpcodeType.UNK_0_X50 => 4,
        OpcodeType.UNK_0_X60 => 4,
        OpcodeType.UNK_0_X70 => 12,
        OpcodeType.UNK_0_X71 => 8,
        OpcodeType.UNK_0_X72 => 4,
        
        _ => throw new ArgumentOutOfRangeException(nameof(opcode), opcode, null)
    };
}