using gdl.schema.objects.mesh;

using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.objects;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/haekb/io_scene_gdl/blob/master/src/model.py#L327
/// </summary>
[BinarySchema]
public sealed partial class Object : IBinaryDeserializable {
  public float InvRad { get; set; }
  public float BndRad { get; set; }
  public uint Flags { get; set; }
  public uint SubObjectCount { get; set; }
  public ushort SubObject0Qwc { get; set; }
  public ushort SubObject0TextureIndex { get; set; }
  public ushort SubObject0LmIndex { get; set; }
  public short SubObject0Lodk { get; set; }
  public uint SubObjectPointer { get; set; }
  public uint DataPointer { get; set; }
  public uint VertexCount { get; set; }
  public uint TriangleCount { get; set; }
  public uint Index { get; set; }
  public uint ObjectDefPointer { get; set; }

  [SequenceLengthSource(4)]
  public uint[] Unk0 { get; set; }

  [RAtPosition(nameof(DataPointer))]
  public Mesh Mesh { get; } = new();

  [RAtPosition(nameof(SubObjectPointer))]
  [RSequenceLengthSource(nameof(SubObjectCount))]
  public SubObject[] SubObjects { get; set; }
}