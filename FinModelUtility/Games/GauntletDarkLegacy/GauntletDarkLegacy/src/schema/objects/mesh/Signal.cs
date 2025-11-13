using schema.binary;

namespace gdl.schema.objects.mesh;

public enum SignalIndex : byte {
  HEADER,
  VERTEX,
  NORMAL,
  VERTEX_COLOR,
  UV,
}

public enum SignalMode : byte {
  SHORT_VEC2 = 0x6d,

  // Vertex
  CHAR_3 = 0x6a,
  SHORT_3 = 0x69,

  // UV
  CHAR_2 = 0x66,
  SHORT_2 = 0x65,

  INT_3 = 0x68,
}

[BinarySchema]
public sealed partial class Signal : IBinaryDeserializable {
  public SignalIndex Index { get; set; }
  public byte Constant { get; set; }
  public byte DataCount { get; set; }
  public SignalMode Mode { get; set; }
}