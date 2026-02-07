namespace danganronpa.schema.gmo;

/// <summary>
///   Shamelessly stolen from:
///   https://wiki.ffrtt.ru/index.php/PSP/GMO_Format#Chunks
/// </summary>
public enum ChunkType : ushort {
  START = 0x2,
  SUB_FILE = 0x3,
  BONE_INFO = 0x4,
  MODEL_SURFACE = 0x5,
  MESH = 0x6,
  VERTEX_ARRAY = 0x7,
  MATERIAL = 0x8,
  TEXTURE_REFERENCE = 0x9,
  TEXTURE = 0xA,
  SHARED_VERTEX_DATA = 0xB,
  TEXTURE_ANIMATION = 0xF,
  UV_SCALE_AND_BIAS = 0x8015,
  MESH_MATERIAL_INFO = 0x8061,
  MESH_INDEX_DATA = 0x8066,
  MATERIAL_TEXTURE_BLEND = 0x8081,
  MATERIAL_RGBA = 0x8082,
  MATERIAL_SPECULARITY = 0x8083,
}

public sealed class Chunk  {
  public ChunkType Type { get; }
  public short HeaderSize { get; }
  public long DataSize { get; }
}
