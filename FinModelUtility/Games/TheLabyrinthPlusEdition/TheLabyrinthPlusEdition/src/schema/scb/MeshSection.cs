using System.Numerics;

using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace tlpe.scb;

[BinarySchema]
public sealed partial class MeshSection : ISection {
  [Unknown]
  private uint maybeSize_;

  [WLengthOfSequence(nameof(Vertices))]
  private uint vertexCount_;

  [WLengthOfSequence(nameof(Faces))]
  private uint faceCount_;

  [RSequenceLengthSource(nameof(vertexCount_))]
  public Vertex[] Vertices { get; set; }

  [RSequenceLengthSource(nameof(faceCount_))]
  public Face[] Faces { get; set; }
}

[BinarySchema]
public sealed partial class Vertex : IBinaryConvertible {
  public Vector3 Position { get; set; }
  public Vector3 Normal { get; set; }
  public Vector2 Uv0 { get; set; }
  public Vector2 Uv1 { get; set; }
}

[BinarySchema]
public sealed partial class Face : IBinaryConvertible {
  public uint MaterialId { get; set; }

  [Unknown]
  private uint unk1_;

  public ushort Vertex0 { get; set; }
  public ushort Vertex1 { get; set; }
  public ushort Vertex2 { get; set; }

  [Unknown]
  private ushort padding_;
}