using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;

using grezzo.schema.cmb.sklm;
using grezzo.schema.cmb.vatr;

using schema.binary;

namespace grezzo.schema.cmb;

public static class DataTypeUtil {
  // TODO: Is there a better way to read arbitrary data types?
  public static float Read(
      IBinaryReader br,
      DataType dataType)
    => dataType switch {
        DataType.BYTE   => br.ReadSByte(),
        DataType.U_BYTE  => br.ReadByte(),
        DataType.SHORT  => br.ReadInt16(),
        DataType.U_SHORT => br.ReadUInt16(),
        DataType.INT    => br.ReadInt32(),
        DataType.U_INT   => br.ReadUInt32(),
        DataType.FLOAT  => br.ReadSingle(),
        _ => throw new ArgumentOutOfRangeException(
            nameof(dataType),
            dataType,
            null)
    };

  public static float Read(
      ReadOnlyMemory<byte> data,
      DataType dataType)
    => dataType switch {
        DataType.BYTE   => (sbyte) data.Span[0],
        DataType.U_BYTE  => data.Span[0],
        DataType.SHORT  => BinaryPrimitives.ReadInt16LittleEndian(data.Span),
        DataType.U_SHORT => BinaryPrimitives.ReadUInt16LittleEndian(data.Span),
        DataType.INT    => BinaryPrimitives.ReadInt32LittleEndian(data.Span),
        DataType.U_INT   => BinaryPrimitives.ReadUInt32LittleEndian(data.Span),
        DataType.FLOAT  => BinaryPrimitives.ReadSingleLittleEndian(data.Span),
        _ => throw new ArgumentOutOfRangeException(
            nameof(dataType),
            dataType,
            null)
    };


  public static float[] Read(
      IBinaryReader br,
      int count,
      DataType dataType) {
      var values = new float[count];

      for (var i = 0; i < count; ++i) {
        values[i] = Read(br, dataType);
      }

      return values;
    }

  public static IEnumerable<float> Read(
      AttributeSlice slice,
      VertexAttribute attribute,
      int itemsPerTuple) {
      if (attribute.Mode == VertexAttributeMode.CONSTANT) {
        foreach (var constant in attribute.Constants.Take(itemsPerTuple)) {
          yield return constant;
        }

        yield break;
      }

      var span = slice.Bytes.AsMemory((int) attribute.Start);

      var dataType = attribute.DataType;
      var size = dataType.GetSize();
      var scale = attribute.Scale;

      for (var i = 0; i < span.Length / size; ++i) {
        yield return scale * Read(span.Slice(size * i, size), dataType);
      }
    }

  public static int GetSize(this DataType dataType)
    => dataType switch {
        DataType.BYTE   => 1,
        DataType.U_BYTE  => 1,
        DataType.SHORT  => 2,
        DataType.U_SHORT => 2,
        DataType.INT    => 4,
        DataType.U_INT   => 4,
        DataType.FLOAT  => 4,
        _ => throw new ArgumentOutOfRangeException(
            nameof(dataType),
            dataType,
            null)
    };
}