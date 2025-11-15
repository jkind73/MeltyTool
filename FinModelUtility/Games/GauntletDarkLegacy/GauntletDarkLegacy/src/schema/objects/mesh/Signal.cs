using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.objects.mesh;

public enum SignalIndex : byte {
  GEOM,
  VERTEX,
  NORMAL,
  VERTEX_COLOR,
  UV,
}

public enum SignalMode : byte {
  NULL = 0,
  STRIP_0_END = 0x14,
  STRIP_N_END = 0x17,

  UINT32_UV = 0x64,
  UINT16_UV = 0x65,
  UINT8_UV = 0x66,

  SINT32_XYZ = 0x68,
  SINT16_XYZ = 0x69,
  SINT8_XYZ = 0x6a,

  FLOAT32 = 0x6c,
  UINT16_LMUV = 0x6d,

  UINT16_BITPACKED = 0x6f,
}

[BinarySchema]
public sealed partial class Signal : IBinaryDeserializable {
  public SignalIndex Index { get; set; }
  public byte Constant { get; set; }
  public byte DataCount { get; set; }
  public SignalMode Mode { get; set; }

  public override string ToString()
    => $"Signal[{Index}]: {Mode}, {DataCount}, {Constant}";

  [Skip]
  public bool IsValid
    => this.Index switch {
        SignalIndex.GEOM => this.Mode is SignalMode.NULL
                                         or SignalMode.STRIP_0_END
                                         or SignalMode.STRIP_N_END
                                         or SignalMode.FLOAT32,
        SignalIndex.VERTEX => this.Mode is SignalMode.FLOAT32
                                           or SignalMode.SINT32_XYZ
                                           or SignalMode.SINT16_XYZ
                                           or SignalMode.SINT8_XYZ,
        SignalIndex.NORMAL       => this.Mode is SignalMode.UINT16_BITPACKED,
        SignalIndex.VERTEX_COLOR => this.Mode is SignalMode.UINT16_BITPACKED,
        SignalIndex.UV => this.Mode is SignalMode.UINT32_UV
                                       or SignalMode.UINT16_UV
                                       or SignalMode.UINT8_UV
                                       or SignalMode.UINT16_LMUV,
        _ => false
    };
}