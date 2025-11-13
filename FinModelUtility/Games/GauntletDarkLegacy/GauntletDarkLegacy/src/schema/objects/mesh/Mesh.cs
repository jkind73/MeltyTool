using System.Numerics;

using fin.color;

using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.objects.mesh;

public class Primitive {
  public required IReadOnlyList<Vector3> Positions { get; init; }
  public required IReadOnlyList<Uv> Uvs { get; init; }
  public required IReadOnlyList<Vector3> Normals { get; init; }
  public required IReadOnlyList<IColor> VertexColors { get; init; }
}

/// <summary>
///   Shamlessly stolen from:
///   https://github.com/haekb/gdl-tools/blob/master/Addons/GDLFormat/Models/Objects.gd#L527
/// </summary>
public sealed class Mesh : IBinaryDeserializable, IChildOf<Object> {
  public Object Parent { get; set; }

  public List<Primitive> Primitives { get; set; } = new();

  public void Read(IBinaryReader br) {
    this.Primitives.Clear();

    var objCount = this.Parent.SubObjectCount;
    while (objCount-- > 0) {
      var positions = new List<Vector3>();
      var uvs = new List<Uv>();
      var normals = new List<Vector3>();
      var vertexColors = new List<IColor>();

      var lastPosition = br.Position;
      var unpackCommand = br.ReadUInt32();

      var unk0 = br.ReadUInt16();
      var unk1 = br.ReadUInt16();

      var unpackSize = (unpackCommand << 4) & 0xFFFFFFFF;
      var totalSize = 0L;

      // SubObj Count - 1 is how much extra data we have
      // ---
      // Once we hit unpack size, check if we'll bleed into another object, if
      // not, then check if there's a new "unpack_command". If there is, mark
      // it as merge with previous then grab that mesh data.
      while (totalSize < unpackSize) {
        while (true) {
          br.Align(4);

          var signal = br.ReadNew<Signal>();
          switch (signal.Index) {
            case SignalIndex.HEADER: {
              var unk2 = br.ReadVector2();
              break;
            }
            case SignalIndex.VERTEX: {
              for (var i = 0; i < signal.DataCount; ++i) {
                // Shamelessly stolen from:
                // https://github.com/haekb/gdl-tools/blob/master/Addons/GDLFormat/Models/Objects.gd#L305
                switch (signal.Mode) {
                  case SignalMode.SHORT_3: {
                    var x = br.ReadInt16();
                    var y = br.ReadInt16();
                    var z = br.ReadInt16();
                    positions.Add(new Vector3(x, y, z));
                    break;
                  }
                  case SignalMode.INT_3: {
                    var x = br.ReadInt32();
                    var y = br.ReadInt32();
                    var z = br.ReadInt32();
                    positions.Add(new Vector3(x, y, z));
                    break;
                  }
                  case SignalMode.CHAR_3:
                  default: {
                    var x = br.ReadSByte();
                    var y = br.ReadSByte();
                    var z = br.ReadSByte();
                    positions.Add(new Vector3(x, y, z));
                    break;
                  }
                }
              }

              break;
            }
            case SignalIndex.UV: {
              for (var i = 0; i < signal.DataCount; ++i) {
                var uv = new Uv(signal.Mode);
                uv.Read(br);
                uvs.Add(uv);
              }

              break;
            }
            case SignalIndex.NORMAL: {
              for (var i = 0; i < signal.DataCount; ++i) {
                // Shamelessly stolen from:
                // https://github.com/haekb/gdl-tools/blob/master/Addons/GDLFormat/Models/Objects.gd#L367
                switch (signal.Mode) {
                  case SignalMode.INT_3: {
                    var x = br.ReadInt32();
                    var y = br.ReadInt32();
                    var z = br.ReadInt32();
                    normals.Add(new Vector3(x, y, z) / (1f * 0xffff));
                    break;
                  }
                  default: {
                    var packedNormal = br.ReadUInt16();

                    var normalScale = 0.06666667f;
                    var x = ((packedNormal & 0x1f) - 0xf) * normalScale;
                    var y = (((packedNormal >> 5) & 0xf) - 0xf) * normalScale;
                    var z = (((packedNormal >> 10) & 0xf) - 0xf) * normalScale;

                    normals.Add(new Vector3(x, y, z));
                    break;
                  }
                }
              }

              break;
            }
            case SignalIndex.VERTEX_COLOR: {
              for (var i = 0; i < signal.DataCount; ++i) {
                // Shamelessly stolen from:
                // https://github.com/haekb/gdl-tools/blob/master/Addons/GDLFormat/Models/Objects.gd#L410
                var value = br.ReadUInt16();
                ColorUtil.SplitRgb5A1(value,
                                      out var r,
                                      out var g,
                                      out var b,
                                      out _);
                vertexColors.Add(FinColor.FromRgbBytes(r, g, b));
              }

              break;
            }
          }

          if (signal.Index >= SignalIndex.UV) {
            break;
          }
        }

        br.Align(4);

        var vifCommands = br.ReadUInt32();

        var currentPosition = br.Position;
        totalSize += currentPosition - lastPosition;
      }

      br.Align(16);

      this.Primitives.Add(new Primitive {
          Positions = positions,
          Uvs = uvs,
          Normals = normals,
          VertexColors = vertexColors,
      });
    }
  }
}