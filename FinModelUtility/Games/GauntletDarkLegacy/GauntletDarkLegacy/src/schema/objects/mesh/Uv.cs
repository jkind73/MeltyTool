using System.Numerics;

using schema.binary;

namespace gdl.schema.objects.mesh;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/haekb/gdl-tools/blob/master/Addons/GDLFormat/Models/Objects.gd#L334
/// </summary>
public sealed class Uv(SignalMode mode) : IBinaryDeserializable {
  public Vector2 Value { get; private set; }
  public Vector2? LightmapUv { get; private set; }

  public void Read(IBinaryReader br) {
    this.LightmapUv = null;
    switch (mode) {
      case SignalMode.CHAR_2: {
        var u = br.ReadSByte() / 128f;
        var v = br.ReadSByte() / 128f;
        this.Value = new Vector2(u, v);
        break;
      }
      case SignalMode.SHORT_2: {
        var u = br.ReadInt16() / 128f;
        var v = br.ReadInt16() / 128f;
        this.Value = new Vector2(u, v);
        break;
      }
      case SignalMode.SHORT_VEC2: {
        var u = br.ReadInt16() / 128f;
        var v = br.ReadInt16() / 128f;
        this.Value = new Vector2(u, v);

        var u2 = br.ReadInt16() / 32768f;
        var v2 = br.ReadInt16() / 32768f;
        this.LightmapUv = new Vector2(u2, v2);
        break;
      }
      default: {
        var u = br.ReadSByte() / 128f;
        var v = br.ReadSByte() / 128f;
        this.Value = new Vector2(u, v);
        break;
      }
    }
  }
}