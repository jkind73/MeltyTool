using System.Numerics;

using f3dzex2.rdp;

using schema.binary;
using schema.binary.attributes;

namespace bar.schema;

/// <summary>
///   "ConTour", a single part of a UVTR track.
/// 
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/BeetleAdventureRacing/ParsedFiles/UVCT.ts
/// </summary>
[BinarySchema]
public sealed partial class Uvct : IBinaryDeserializable {
  public ushort VertexCount { get; set; }

  [WLengthOfSequence(nameof(Triangles))]
  private ushort triangleCount_;

  [WLengthOfSequence(nameof(Models))]
  private ushort modelCount_;

  [WLengthOfSequence(nameof(MaterialMeshes))]
  private ushort materialMeshCount_;

  [SequenceLengthSource(3)]
  public float[] Unk0 { get; set; }

  [RSequenceLengthSource(nameof(triangleCount_))]
  public UvctTriangle[] Triangles { get; set; }

  [RSequenceLengthSource(nameof(modelCount_))]
  public UvctModel[] Models { get; set; }

  [RSequenceLengthSource(nameof(materialMeshCount_))]
  public UvctMaterialMesh[] MaterialMeshes { get; set; }

  [SequenceLengthSource(4)]
  public int[] Unk1 { get; set; }
}

[BinarySchema]
public sealed partial class UvctTriangle : IBinaryDeserializable {
  public ushort Vertex0 { get; set; }
  public ushort Vertex1 { get; set; }
  public ushort Vertex2 { get; set; }
  public ushort Flags { get; set; }
}

[BinarySchema]
public sealed partial class UvctModel : IBinaryDeserializable {
  [SequenceLengthSource(SchemaIntegerType.BYTE)]
  public RdpMatrix4x4[] RdpMatrices { get; set; }

  public short ModelIndex { get; set; }
  public Vector3 Position { get; set; }

  public float Unk0 { get; set; }
  public ushort Unk1 { get; set; }
  public ushort Unk2 { get; set; }
}

[BinarySchema]
public sealed partial class UvctMaterialMesh : IBinaryDeserializable {
  public UvmdMaterialMesh Impl { get; } = new();

  [SequenceLengthSource(4)]
  public ushort[] Unk0 { get; set; }

  [SequenceLengthSource(4)]
  public uint[] Unk1 { get; set; }
}