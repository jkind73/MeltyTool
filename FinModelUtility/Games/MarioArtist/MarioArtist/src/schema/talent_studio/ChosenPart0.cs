using fin.schema.color;

using schema.binary;


namespace marioartist.schema.talent_studio;

[BinarySchema]
public sealed partial class ChosenColor : IBinaryDeserializable {
  public uint Index { get; set; }
  public Rgba32 Color { get; set; }
}

// Blend/Multiply are kind of just random names, I'm not really sure what these
// are meant to be.
public enum PatternMaterialType : uint {
  BLEND_1X1,
  BLEND_2X2,
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

  public ChosenPart0PatternParams Pattern0Params { get; } = new();
  public ChosenPart0PatternParams Pattern1Params { get; } = new();
  public ChosenPart0PatternParams MarkParams { get; } = new();
}

[BinarySchema]
public sealed partial class ChosenPart0PatternParams : IBinaryDeserializable {
  public uint Unk0 { get; set; }
  public uint Unk1 { get; set; }
  public uint ImageSegmentedAddress { get; set; }
  public PatternMaterialType MaterialType { get; set; }
}