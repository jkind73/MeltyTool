using schema.binary;
using schema.binary.attributes;

namespace ttyd.schema.world.chunks;

[BinarySchema]
public sealed partial class NamedChunkTable
    : IChildOf<World>, IBinaryDeserializable {
  public World Parent { get; set; }

  [RSequenceLengthSource(nameof(Parent.NamedChunkTableCount))]
  public NamedChunkEntry[] Entries { get; set; }
}

[BinarySchema]
public sealed partial class NamedChunkEntry
    : IChildOf<NamedChunkTable>, IBinaryDeserializable {
  public NamedChunkTable Parent { get; set; }

  private uint chunkOffsetRelativeToMainData_;
  private uint chunkNameOffsetRelativeToTable_;

  [Skip]
  public INamedChunkData? Data { get; set; }

  [ReadLogic]
  private void SwitchDataType_(IBinaryReader br) {
    var chunkName = br.SubreadAt(
        this.Parent.Parent.NamedChunkTableOffset +
        this.chunkNameOffsetRelativeToTable_,
        br.ReadStringNT);

    var chunkOffset = this.Parent.Parent.MainDataOffset +
                      this.chunkOffsetRelativeToMainData_;

    this.Data = chunkName switch {
        "animation_table"     => null,
        "curve_table"         => null,
        "fog_table"           => null,
        "information"         => null,
        "light_table"         => null,
        "material_name_table" => null,
        "texture_table"       => null,
        "vcd_table"           => null,
    };

    this.Data?.Read(br);
  }
}