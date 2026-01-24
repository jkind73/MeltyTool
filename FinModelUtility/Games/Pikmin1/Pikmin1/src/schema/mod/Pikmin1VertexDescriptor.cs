using System.Collections.Generic;

using gx;
using gx.displayList;

namespace pikmin1.schema.mod;

public sealed class Pikmin1VertexDescriptor(uint value, bool hasNormals)
    : BVertexDescriptor(value) {
  public bool UseNbt { get; private set; }

  protected override IEnumerable<(GxVertexAttribute, GxAttributeType?,
          GxColorComponentType?)>
      GetEnumerator(uint value) {
    if ((value & 0b1) == 1) {
      yield return (GxVertexAttribute.POS_MAT_IDX, null, null);
    }

    value >>= 1;

    if ((value & 0b1) == 1) {
      yield return (GxVertexAttribute.TEX1_MAT_IDX, null, null);
    }

    value >>= 1;

    // Position is implied to be always enabled
    yield return (GxVertexAttribute.POSITION, GxAttributeType.INDEX_16, null);

    if (hasNormals) {
      yield return (GxVertexAttribute.NORMAL, GxAttributeType.INDEX_16, null);
    }

    if ((value & 0b1) == 1) {
      yield return (GxVertexAttribute.COLOR0, GxAttributeType.INDEX_16, null);
    }

    value >>= 1;

    for (uint i = 0; i < 8; ++i) {
      if ((value & 0b1) == 1) {
        yield return (GxVertexAttribute.TEX0_COORD + i,
                      GxAttributeType.INDEX_16,
                      null);
      }

      value >>= 1;
    }

    this.UseNbt = (value & 0x20) != 0;
  }
}