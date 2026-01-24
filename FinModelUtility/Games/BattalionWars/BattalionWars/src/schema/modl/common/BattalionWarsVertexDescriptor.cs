using fin.math;

using gx;
using gx.displayList;

namespace modl.schema.modl.common;

public sealed class BattalionWarsVertexDescriptor(uint value)
    : BVertexDescriptor(value) {
  protected override IEnumerable<(GxVertexAttribute, GxAttributeType?,
          GxColorComponentType?)>
      GetEnumerator(uint value) {
    if (value.GetBit(0)) {
      yield return (GxVertexAttribute.POS_MAT_IDX, null, null);
    }

    value >>= 1;

    for (uint i = 0; i < 8; ++i) {
      if (value.GetBit(0)) {
        yield return (GxVertexAttribute.TEX0_MAT_IDX + i, null, null);
      }

      value >>= 1;
    }

    var positionFormat = (GxAttributeType) (value & 3);
    if (positionFormat != GxAttributeType.NOT_PRESENT) {
      yield return (GxVertexAttribute.POSITION, positionFormat, null);
    }

    value >>= 2;

    var normalFormat = (GxAttributeType) (value & 3);
    if (normalFormat != GxAttributeType.NOT_PRESENT) {
      yield return (GxVertexAttribute.NORMAL, normalFormat, null);
    }

    value >>= 2;

    var colorFormat0 = (GxAttributeType) (value & 3);
    if (colorFormat0 != GxAttributeType.NOT_PRESENT) {
      yield return (GxVertexAttribute.COLOR0, colorFormat0, null);
    }

    value >>= 2;

    var colorFormat1 = (GxAttributeType) (value & 3);
    if (colorFormat1 != GxAttributeType.NOT_PRESENT) {
      yield return (GxVertexAttribute.COLOR1, colorFormat1, null);
    }

    value >>= 2;

    for (uint i = 0; i < 8; ++i) {
      if (value.GetBit(0)) {
        yield return (GxVertexAttribute.TEX0_COORD + i, null, null);
      }

      value >>= 1;
    }

    if (value != 0) {
      throw new NotImplementedException();
    }
  }
}