namespace gx;

public enum GxTexGenType : byte {
  MATRIX3_X4 = 0,
  MATRIX2_X4 = 1,
  BUMP0 = 2,
  BUMP1 = 3,
  BUMP2 = 4,
  BUMP3 = 5,
  BUMP4 = 6,
  BUMP5 = 7,
  BUMP6 = 8,
  BUMP7 = 9,
  SRTG = 10
}

public enum GxTexGenSrc : byte {
  POSITION = 0,
  NORMAL = 1,
  BINORMAL = 2,
  TANGENT = 3,
  TEX0 = 4,
  TEX1 = 5,
  TEX2 = 6,
  TEX3 = 7,
  TEX4 = 8,
  TEX5 = 9,
  TEX6 = 10,
  TEX7 = 11,
  TEX_COORD0 = 12,
  TEX_COORD1 = 13,
  TEX_COORD2 = 14,
  TEX_COORD3 = 15,
  TEX_COORD4 = 16,
  TEX_COORD5 = 17,
  TEX_COORD6 = 18,
  COLOR0 = 19,
  COLOR1 = 20,
}

public enum GxTexMatrix : byte {
  TEX_MTX0 = 30,
  TEX_MTX1 = 33,
  TEX_MTX2 = 36,
  TEX_MTX3 = 39,
  TEX_MTX4 = 42,
  TEX_MTX5 = 45,
  TEX_MTX6 = 48,
  TEX_MTX7 = 51,
  TEX_MTX8 = 54,
  TEX_MTX9 = 57,
  IDENTITY = 60,
}

public interface ITexCoordGen {
  GxTexGenType TexGenType { get; }
  GxTexGenSrc TexGenSrc { get; }
  GxTexMatrix TexMatrix { get; }
}

public record TexCoordGenImpl(
    GxTexGenType TexGenType,
    GxTexGenSrc TexGenSrc,
    GxTexMatrix TexMatrix) : ITexCoordGen;