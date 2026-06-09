using System.Numerics;

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
public sealed partial class Uvmd : IBinaryConvertible {
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
public sealed partial class UvmdUnk0 : IBinaryConvertible {
  [SequenceLengthSource(10)]
  public float[] UnkFloats { get; set; }

  [SequenceLengthSource(3)]
  public short[] UnkShorts { get; set; }

  public byte UnkByte { get; set; }
}

[BinarySchema]
public sealed partial class UvmdLod : IBinaryConvertible {
  [WLengthOfSequence(nameof(ModelParts))]
  private byte modelPartCount_;

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool Billboard { get; set; }

  public float F { get; set; }

  [RSequenceLengthSource(nameof(modelPartCount_))]
  public UvmdModelPart[] ModelParts { get; set; }
}

[BinarySchema]
public sealed partial class UvmdModelPart : IBinaryConvertible {
  public byte B5 { get; set; }
  public byte B6 { get; set; }
  public byte B7 { get; set; }

  public Vector3 Vec1 { get; set; }
  public Vector3 Vec2 { get; set; }

  public byte StackByte1 { get; set; }
  public byte StackByte2 { get; set; }

  [SequenceLengthSource(SchemaIntegerType.BYTE)]
  public UvmdMaterial[] Materials { get; set; }
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/BeetleAdventureRacing/ParsedFiles/Common.ts#L83
/// </summary>
[BinarySchema]
public sealed partial class UvmdMaterial : IBinaryConvertible {

}