using schema.binary;
using schema.binary.attributes;

namespace hw.schema.binary;

public enum BinaryResourceChunkType : ulong {
  UNKNOWN = 0,

  XTD_XTD_HEADER = 0x1111,
  XTD_TERRAIN_CHUNK = 0x2222,
  XTD_ATLAS_CHUNK = 0x8888,
  XTD_TESS_CHUNK = 0xAAAA,
  XTD_LIGHTING_CHUNK = 0xBBBB,
  XTD_AO_CHUNK = 0xCCCC,
  XTD_ALPHA_CHUNK = 0xDDDD,

  XTT_TERRAIN_ATLAS_LINK_CHUNK = 0x2222,
  XTT_ATLAS_CHUNK_ALBEDO = 0x6666,
  XTT_ROAD_CHUNK = 0x8888,
  XTT_FOLIAGE_HEADER_CHUNK = 0xAAAA,
  XTT_FOLIAGE_QN_CHUNK = 0xBBBB,

  UGX_CACHED_DATA_CHUNK = 0x00000700,
  UGX_INDEX_BUFFER_CHUNK = 0x00000701,
  UGX_VERTEX_BUFFER_CHUNK = 0x00000702,
  UGX_GRX_CHUNK = 0x00000703, // granny_file_info
  UGX_MATERIAL_CHUNK = 0x00000704,
  UGX_TREE_CHUNK = 0x00000705,
  UGX_CACHED_DATA_SIGNATURE_HW1_BE = 0xC2340004,
  UGX_CACHED_DATA_SIGNATURE_HW1_LE = 0x040034C2,
  UGX_CACHED_DATA_SIGNATURE_HW2 = 0x060034C2, // 0xC2340006
}

[BinarySchema]
public sealed partial class BinaryResourceChunk : IBinaryDeserializable {
  public BinaryResourceChunkType Type { get; set; }
  public uint Offset { get; set; }
  public uint Size { get; set;}

  [SequenceLengthSource(2)]
  private uint[] unk0_;

  [RAtPosition(nameof(Offset))]
  [RSequenceLengthSource(nameof(Size))]
  public byte[] Data { get; set; }

  public IBinaryReader GetBinaryReader()
    => new SchemaBinaryReader(this.Data, Endianness.BigEndian);
}