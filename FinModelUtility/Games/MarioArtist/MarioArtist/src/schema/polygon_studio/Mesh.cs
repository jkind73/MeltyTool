using schema.binary;
using schema.binary.attributes;

namespace marioartist.schema.polygon_studio;

[BinarySchema]
[LocalPositions]
public sealed partial class Mesh : IBinaryDeserializable {
  public uint VertexCount { get; set; }
  public uint Unk0 { get; set; }
  public uint TriangleCount { get; set; }
  public uint Unk1 { get; set; }
  public uint VertexDefinitionsOffset { get; set; }
  public uint VertexDefinitionsSize { get; set; }
  public uint Unk2sOffset { get; set; }
  public uint Unk2sSize { get; set; }
  public uint TriangleDefinitionsOffset { get; set; }
  public uint TriangleDefinitionsSize { get; set; }
  public uint Unk3 { get; set; }
  public uint Unk4 { get; set; }
  public uint NextMeshOffset { get; set; }
  public uint Unk5 { get; set; }

  [ReadLogic]
  public void Read_(IBinaryReader br) {
    ;
  }

  [RAtPositionOrNull(nameof(VertexDefinitionsOffset), -1)]
  [RSequenceLengthSource(nameof(VertexCount))]
  public Vertex[]? Vertices { get; set; }

  [RAtPositionOrNull(nameof(TriangleDefinitionsOffset), -1)]
  [RSequenceLengthSource(nameof(TriangleCount))]
  public Triangle[]? Triangles { get; set; }

  [RAtPositionOrNull(nameof(NextMeshOffset), -1)]
  public Mesh? NextMesh { get; set; }
}