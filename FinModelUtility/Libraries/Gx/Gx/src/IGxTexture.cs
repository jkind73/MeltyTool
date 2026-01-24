using fin.image;
using fin.model;

namespace gx;

public enum GxMagTextureFilter : byte {
  GX_NEAR,
  GX_LINEAR,
}

public enum GxMinTextureFilter : byte {
  GX_NEAR,
  GX_LINEAR,
  GX_NEAR_MIP_NEAR,
  GX_LIN_MIP_NEAR,
  GX_NEAR_MIP_LIN,
  GX_LIN_MIP_LIN,
  GX_NEAR2,
  GX_NEAR3,
}

public static class GxTextureFilterExtensions {
  public static TextureMagFilter ToFinMagFilter(
      this GxMagTextureFilter gxMagFilter)
    => gxMagFilter switch {
        GxMagTextureFilter.GX_NEAR   => TextureMagFilter.NEAR,
        GxMagTextureFilter.GX_LINEAR => TextureMagFilter.LINEAR,
        _                               => throw new ArgumentOutOfRangeException(nameof(gxMagFilter), gxMagFilter, null)
    };

  public static TextureMinFilter ToFinMinFilter(
      this GxMinTextureFilter gxMinFilter)
    => gxMinFilter switch {
        GxMinTextureFilter.GX_NEAR   => TextureMinFilter.NEAR,
        GxMinTextureFilter.GX_LINEAR => TextureMinFilter.LINEAR,
        GxMinTextureFilter.GX_NEAR_MIP_NEAR => TextureMinFilter
            .NEAR_MIPMAP_NEAR,
        GxMinTextureFilter.GX_LIN_MIP_NEAR => TextureMinFilter
            .LINEAR_MIPMAP_NEAR,
        GxMinTextureFilter.GX_NEAR_MIP_LIN => TextureMinFilter
            .NEAR_MIPMAP_LINEAR,
        GxMinTextureFilter.GX_LIN_MIP_LIN => TextureMinFilter
            .LINEAR_MIPMAP_LINEAR,
        GxMinTextureFilter.GX_NEAR2 => TextureMinFilter.NEAR,
        GxMinTextureFilter.GX_NEAR3 => TextureMinFilter.NEAR,
    };
};

public interface IGxTexture {
  string Name { get; }
  IReadOnlyImage[] MipmapImages { get; }
  GxWrapMode WrapModeS { get; }
  GxWrapMode WrapModeT { get; }
  GxMagTextureFilter MagTextureFilter { get; }
  GxMinTextureFilter MinTextureFilter { get; }
  ColorType ColorType { get; }
  float MinLod => -1000;
  float MaxLod => 1000;
  float LodBias => 0;
}

public record GxTexture2d(
    string Name,
    IReadOnlyImage[] MipmapImages,
    GxWrapMode WrapModeS,
    GxWrapMode WrapModeT,
    GxMinTextureFilter MinTextureFilter
        = GxMinTextureFilter.GX_LIN_MIP_LIN,
    GxMagTextureFilter MagTextureFilter = GxMagTextureFilter.GX_LINEAR,
    ColorType ColorType = ColorType.COLOR,
    float LodBias = 0) : IGxTexture;