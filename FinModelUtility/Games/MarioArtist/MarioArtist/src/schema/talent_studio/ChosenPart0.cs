using fin.schema.color;

using schema.binary;


namespace marioartist.schema.talent_studio;

[BinarySchema]
public sealed partial class ChosenColor : IBinaryDeserializable {
  public uint Index { get; set; }
  public Rgba32 Color { get; set; }
}

public enum PatternMaterialType : uint {
  COMBINE_1X1,
  COMBINE_2X2,
  MULTIPLY_1X1,
  MULTIPLY_2X2,
  SPHERICAL,
}

[BinarySchema]
public sealed partial class ChosenPart0 : IBinaryDeserializable {
  public uint Id { get; set; }
  public uint Unk0 { get; set; }

  public ChosenColor ChosenColor0 { get; } = new();
  public ChosenColor ChosenColor1 { get; } = new();

  public uint[] UnkForPattern0 { get; } = new uint[2];

  public uint Pattern0SegmentedAddress { get; set; }
  public PatternMaterialType Pattern0MaterialType { get; set; }
  public uint[] Unk2 { get; } = new uint[2];

  public uint Pattern1SegmentedAddress { get; set; }
  public PatternMaterialType Pattern1MaterialType { get; set; }
  public uint[] Unk3 { get; } = new uint[2];

  public uint UnkSegmentedAddress2 { get; set; }

  public uint Unk4 { get; set; }
}