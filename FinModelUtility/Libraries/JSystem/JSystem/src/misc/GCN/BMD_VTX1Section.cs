using System;
using System.Numerics;

using fin.color;
using fin.util.asserts;

using gx;
using gx.displayList;

using jsystem.G3D_Binary_File_Format;
using jsystem.schema.j3dgraph.bmd.vtx1;

using schema.binary;

#pragma warning disable CS8604


namespace jsystem.GCN;

public partial class Bmd {
  public partial class Vtx1Section {
    public IColor[][] colors = new IColor[2][];
    public Vector2[][] texCoords = new Vector2[8][];
    public const string SIGNATURE = "VTX1";
    public DataBlockHeader header;
    public uint arrayFormatOffset;
    public uint[] offsets;
    public ArrayFormat[] arrayFormats;
    public Vector3[] positions;
    public Vector3[] normals;
    public Vector3[] tangents;
    public Vector3[] binormals;

    public Vtx1Section(IBinaryReader br, out bool ok) {
      long position1 = br.Position;
      bool ok1;
      this.header = new DataBlockHeader(br, "VTX1", out ok1);
      if (!ok1) {
        ok = false;
      } else {
        this.arrayFormatOffset = br.ReadUInt32();
        this.offsets = br.ReadUInt32s(13);
        long position2 = br.Position;
        int length1 = 0;
        foreach (uint offset in this.offsets) {
          if (offset != 0U)
            ++length1;
        }

        br.Position = position1 + (long) this.arrayFormatOffset;
        this.arrayFormats = br.ReadNews<ArrayFormat>(length1);

        int index1 = 0;
        for (int k = 0; k < 13; ++k) {
          if (this.offsets[k] != 0U) {
            ArrayFormat arrayFormat = this.arrayFormats[index1];
            int length2 = this.GetLength_(k);
            br.Position = position1 + (long) this.offsets[k];
            if (arrayFormat.arrayType is GxVertexAttribute.COLOR0
                                         or GxVertexAttribute.COLOR1) {
              this.ReadColorArray_(arrayFormat, length2, br);
            } else {
              this.ReadVertexArray_(arrayFormat, length2, br);
            }

            ++index1;
          }
        }

        br.Position = position1 + (long) this.header.size;
        ok = true;
      }
    }

    private int GetLength_(int k) {
      int offset = (int) this.offsets[k];
      for (int index = k + 1; index < 13; ++index) {
        if (this.offsets[index] != 0U)
          return (int) this.offsets[index] - offset;
      }

      return (int) this.header.size - offset;
    }

    private void ReadVertexArray_(
        ArrayFormat format,
        int dataLength,
        IBinaryReader br) {
      var axisComponentType = (GxAxisComponentType) format.dataType;
      var valueCount = dataLength / axisComponentType.GetByteCount();
      var componentCount = GxAttributeUtil.GetComponentCount(format.arrayType,
        format.ComponentCountType);
      var vectorCount = valueCount / componentCount;

      switch (format.arrayType) {
        case GxVertexAttribute.POSITION: {
          this.positions = new Vector3[vectorCount];
          for (var i = 0; i < vectorCount; ++i) {
            this.positions[i] = GxAttributeUtil.ReadPosition(
                br,
                format.ComponentCountType,
                axisComponentType,
                format.decimalPoint);
          }

          break;
        }
        case GxVertexAttribute.NORMAL: {
          this.normals = new Vector3[vectorCount];
          for (var i = 0; i < vectorCount; ++i) {
            this.normals[i] = GxAttributeUtil.ReadNormal(
                br,
                format.ComponentCountType,
                axisComponentType,
                format.decimalPoint);
          }

          break;
        }
        case >= GxVertexAttribute.TEX0_COORD
             and <= GxVertexAttribute.TEX7_COORD: {
          var texCoordIndex = format.arrayType - GxVertexAttribute.TEX0_COORD;
          var texCoords = this.texCoords[texCoordIndex]
              = new Vector2[vectorCount];
          for (var i = 0; i < vectorCount; ++i) {
            texCoords[i] = GxAttributeUtil.ReadTexCoord(
                br,
                format.ComponentCountType,
                axisComponentType,
                format.decimalPoint);
          }

          break;
        }
        default:
          throw new NotImplementedException();
      }
    }

    /// <summary>
    ///   Colors are a special case:
    ///   https://wiki.cloudmodding.com/tww/BMD_and_BDL#Data_Types
    /// </summary>
    private void ReadColorArray_(
        ArrayFormat format,
        int byteLength,
        IBinaryReader br) {
      var colorIndex = format.arrayType - GxVertexAttribute.COLOR0;

      var colorDataType = (GxColorComponentType) format.dataType;
      var expectedComponentCount = colorDataType switch {
          GxColorComponentType.RGB565 => 3,
          GxColorComponentType.RGB8   => 3,
          GxColorComponentType.RGBX8  => 4,
          GxColorComponentType.RGBA4  => 4,
          GxColorComponentType.RGBA6  => 4,
          GxColorComponentType.RGBA8  => 4,
          _                           => throw new ArgumentOutOfRangeException()
      };

      var actualComponentCount
          = GxAttributeUtil.GetColorComponentCount(format.ComponentCountType);
      Asserts.Equal(expectedComponentCount, actualComponentCount);

      var colorCount = colorDataType switch {
          GxColorComponentType.RGB565 => byteLength / 2,
          GxColorComponentType.RGB8   => byteLength / 3,
          GxColorComponentType.RGBX8  => byteLength / 4,
          GxColorComponentType.RGBA4  => byteLength / 2,
          GxColorComponentType.RGBA6  => byteLength / 3,
          GxColorComponentType.RGBA8  => byteLength / 4,
          _                           => throw new ArgumentOutOfRangeException()
      };

      var colors = this.colors[colorIndex] = new IColor[colorCount];
      for (var i = 0; i < colorCount; ++i) {
        colors[i] = GxAttributeUtil.ReadColor(br, colorDataType);
      }
    }
  }
}