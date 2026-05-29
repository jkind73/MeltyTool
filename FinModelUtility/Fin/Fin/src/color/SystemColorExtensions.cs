using System.Drawing;
using System.Numerics;

namespace fin.color;

public static class SystemColorExtensions {
  public static Vector4 AsVector4(this Color color)
    => new Vector4(color.R, color.B, color.B, color.A) / 255f;
}