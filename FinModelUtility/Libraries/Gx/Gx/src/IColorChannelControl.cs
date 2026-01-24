namespace gx;

public enum GxColorSrc : byte {
  REGISTER = 0, // Use Register Colors
  VERTEX = 1    // Use Vertex Colors
}

public enum GxAttenuationFunction : byte {
  // No attenuation
  NONE = 2,

  // Specular Computation
  SPEC = 0,

  // Spot Light Attenuation
  SPOT = 1
}

[Flags]
public enum GxLightMask : byte {
  LIGHT0 = 0x01,
  LIGHT1 = 0x02,
  LIGHT2 = 0x04,
  LIGHT3 = 0x08,
  LIGHT4 = 0x10,
  LIGHT5 = 0x20,
  LIGHT6 = 0x40,
  LIGHT7 = 0x80,
  NONE = 0x00
}

public static class GxLightMaskExtensions {
  public static IEnumerable<int> GetActiveLights(this GxLightMask lightMask) {
      for (var i = 0; i < 8; ++i) {
        var bitMask = 1 << i;
        if (((int) lightMask & bitMask) != 0) {
          yield return i;
        }
      }
    }
}

public enum GxDiffuseFunction : byte {
  NONE = 0,
  SIGNED = 1,
  CLAMP = 2
}


public interface IColorChannelControl {
  bool LightingEnabled { get; }

  GxColorSrc MaterialSrc { get; }

  /// <summary>
  ///   Which lights affect the vertex.
  /// </summary>
  GxLightMask LitMask { get; }

  /// <summary>
  ///   How to merge the lights together.
  /// </summary>
  GxDiffuseFunction DiffuseFunction { get; }

  /// <summary>
  ///   What type of attenuation function to use for the lights.
  /// </summary>
  GxAttenuationFunction AttenuationFunction { get; }

  GxColorSrc AmbientSrc { get; }

  int? VertexColorIndex => null;
}

public sealed class ColorChannelControlImpl : IColorChannelControl {
  public bool LightingEnabled { get; set; }
  public GxColorSrc MaterialSrc { get; set; }
  public GxLightMask LitMask { get; set; }
  public GxDiffuseFunction DiffuseFunction { get; set; }
  public GxAttenuationFunction AttenuationFunction { get; set; }
  public GxColorSrc AmbientSrc { get; set; }
  public int? VertexColorIndex { get; set; }
}