using System;
using System.Numerics;

using CommunityToolkit.HighPerformance;

using fin.math.matrix.four;

using schema.binary;
using schema.binary.attributes;


namespace f3dzex2.rdp;

[BinarySchema]
public sealed partial class RdpMatrix4x4 : IBinaryConvertible {
  [SequenceLengthSource(16)]
  public short[] HighValues { get; set; }
 
  [SequenceLengthSource(16)]
  public ushort[] LowValues { get; set; }

  public Matrix4x4 ToMatrix4x4() {
    Span<float> dst = stackalloc float[16];

    for (var i = 0; i < 16; ++i) {
      float value = (this.HighValues[i] << 16) | this.LowValues[i];
      value /= 0x10000;

      dst[i] = value;
    }

    return dst.Cast<float, Matrix4x4>()[0];
  }
}