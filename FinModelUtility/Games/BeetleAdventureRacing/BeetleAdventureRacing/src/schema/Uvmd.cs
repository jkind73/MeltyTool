using System.Numerics;

using fin.math;
using fin.schema.color;
using fin.schema.vector;

using schema.binary;
using schema.binary.attributes;

namespace bar.schema;

/// <summary>
///   "Model Data"
/// 
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/BeetleAdventureRacing/ParsedFiles/UVMD.ts
/// </summary>
[BinarySchema]
public sealed partial class Uvmd : IBinaryDeserializable {
  [WLengthOfSequence(nameof(Lods))]
  private byte lodCount_;

  [WLengthOfSequence(nameof(Transforms))]
  private byte transformCount_;

  public byte B3 { get; set; }

  private readonly byte padding_ = 0;

  public ushort VertexCount { get; set; }
  public ushort MaterialCount { get; set; }
  public ushort CommandCount { get; set; }

  [Skip]
  private bool HasUnk0 => (this.B3 & 0x80) != 0;

  [RIfBoolean(nameof(HasUnk0))]
  public UvmdUnk0? Unk0 { get; set; }

  public float Float1 { get; set; }
  public float Float2 { get; set; }
  public float Float3 { get; set; }

  [RSequenceLengthSource(nameof(lodCount_))]
  public UvmdLod[] Lods { get; set; }

  [RSequenceLengthSource(nameof(transformCount_))]
  public Matrix4x4[] Transforms { get; set; }
}

[BinarySchema]
public sealed partial class UvmdUnk0 : IBinaryDeserializable {
  [SequenceLengthSource(10)]
  public float[] UnkFloats { get; set; }

  [SequenceLengthSource(3)]
  public short[] UnkShorts { get; set; }

  public byte UnkByte { get; set; }
}

[BinarySchema]
public sealed partial class UvmdLod : IBinaryDeserializable {
  [WLengthOfSequence(nameof(ModelParts))]
  private byte modelPartCount_;

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool Billboard { get; set; }

  public float F { get; set; }

  [RSequenceLengthSource(nameof(modelPartCount_))]
  public UvmdModelPart[] ModelParts { get; set; }
}

[BinarySchema]
public sealed partial class UvmdModelPart : IBinaryDeserializable {
  public byte B5 { get; set; }
  public byte B6 { get; set; }
  public byte B7 { get; set; }

  public Vector3 Vec1 { get; set; }
  public Vector3 Vec2 { get; set; }

  public byte StackByte1 { get; set; }
  public byte StackByte2 { get; set; }

  [SequenceLengthSource(SchemaIntegerType.BYTE)]
  public UvmdMaterialMesh[] MaterialMeshes { get; set; }
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/BeetleAdventureRacing/ParsedFiles/Common.ts#L83
/// </summary>
[BinarySchema]
public sealed partial class UvmdMaterialMesh : IBinaryDeserializable {
  public uint RenderOptions { get; set; }

  [Skip]
  public uint UvtxIndex => this.RenderOptions.ExtractFromRight(0, 12);

  public uint LightPackedColor1 { get; set; }
  public uint LightPackedColor2 { get; set; }
  public uint LightPackedColor3 { get; set; }

  [WLengthOfSequence(nameof(Vertices))]
  private ushort vertexCount_;

  [WLengthOfSequence(nameof(Triangles))]
  private ushort triangleCount_;

  public ushort AlmostAlwaysVertexCount { get; set; }
  public ushort LoadCommandCount { get; set; }
  public ushort ShortCount { get; set; }
  public ushort CommandCount { get; set; }

  [RSequenceLengthSource(nameof(vertexCount_))]
  public UvmdVertex[] Vertices { get; set; }

  [Skip]
  public (ushort, ushort, ushort)[] Triangles { get; set; }

  [ReadLogic]
  private void ReadTriangles_(IBinaryReader br) {
    Span<ushort> fakeVertexMemory = stackalloc ushort[32];

    this.Triangles = new (ushort, ushort, ushort)[this.triangleCount_];

    var triangleIndex = 0;
    for (var j = 0; j < this.ShortCount; j++) {
      var nextShort = br.ReadUInt16();

      // If the highest bit is not set, we read another byte
      // and use that + the short to load new verts
      if ((nextShort & 0x8000) == 0) {
        var extraByte = br.ReadByte();

        // Unpack parameters
        var numVerts = 1 + (((nextShort & 0x6000) >> 10) | ((extraByte & 0xE0) >> 5));
        var destIndex = extraByte & 0x1F;
        var srcIndex = nextShort & 0x1FFF;

        // Fake copy vertices, i.e. copy indices
        for (var v = 0; v < numVerts; v++) {
          fakeVertexMemory[destIndex + v] = (ushort) (srcIndex + v);
        }
      }
      else { // This is just a triangle
        // The indices here are indices into vertex memory, not into
        // the vertex data loaded above.
        var a = (nextShort & 0x7c00) >> 10;
        var b = (nextShort & 0x03e0) >> 5;
        var c = (nextShort & 0x001f) >> 0;

        this.Triangles[triangleIndex++] = (
            fakeVertexMemory[a], fakeVertexMemory[b], fakeVertexMemory[c]);
      }
    }
  }
}

[BinarySchema]
public sealed partial class UvmdVertex : IBinaryConvertible {
  public Vector3s Position { get; } = new();

  public short Unk0 { get; set; }

  public Vector2s TexCoords { get; } = new();
  public Rgba32 Color { get; set; }
}