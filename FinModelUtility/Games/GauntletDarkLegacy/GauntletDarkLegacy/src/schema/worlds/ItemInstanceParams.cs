using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.worlds;

public interface IItemInstanceParams : IBinaryDeserializable;

[BinarySchema]
public sealed partial class ContainerItemInstanceParams : IItemInstanceParams {
  public uint Index { get; set; }
  public ushort Value { get; set; }
  private readonly byte[] padding_ = new byte[6];
}

[BinarySchema]
public sealed partial class TrapItemInstanceParams : IItemInstanceParams {
  public ushort Damage { get; set; }
  public ushort Interval { get; set; }
  private readonly byte[] padding_ = new byte[8];
}

[BinarySchema]
public sealed partial class EnemyItemInstanceParams : IItemInstanceParams {
  public ushort Strength { get; set; }
  public ushort Ai { get; set; }
  public float Radius { get; set; }
  public ushort Interval { get; set; }
  public ushort Dummy { get; set; }
}

[BinarySchema]
public sealed partial class ExitItemInstanceParams : IItemInstanceParams {
  public uint Next { get; set; }

  [StringLengthSource(4)]
  public string Description { get; set; }

  private readonly byte[] padding_ = new byte[4];
}

[BinarySchema]
public sealed partial class GeneratorItemInstanceParams : IItemInstanceParams {
  public ushort Strength { get; set; }
  public ushort Ai { get; set; }
  public ushort MaxEnemies { get; set; }
  public ushort Interval { get; set; }
  private readonly byte[] padding_ = new byte[4];
}

[BinarySchema]
public sealed partial class ObstacleItemInstanceParams : IItemInstanceParams {
  public ushort SubType { get; set; }
  public ushort Strength { get; set; }
  private readonly byte[] padding_ = new byte[8];
}

[BinarySchema]
public sealed partial class PowerupItemInstanceParams : IItemInstanceParams {
  public ushort Value { get; set; }
  private readonly byte[] padding_ = new byte[10];
}

[BinarySchema]
public sealed partial class RotatorItemInstanceParams : IItemInstanceParams {
  public uint TargetWorldObjectIndex { get; set; }
  public float Speed { get; set; }
  public float Delta { get; set; }
}

[BinarySchema]
public sealed partial class SoundItemInstanceParams : IItemInstanceParams {
  public float Radius { get; set; }
  public uint MusicArea { get; set; }
  public ushort Fade { get; set; }
  public ushort Flags { get; set; }
  private readonly byte[] padding_ = new byte[2];
}

[BinarySchema]
public sealed partial class TransporterItemInstanceParams : IItemInstanceParams {
  public uint Id { get; set; }
  public uint DestinationId { get; set; }
  private readonly byte[] padding_ = new byte[4];
}

[BinarySchema]
public sealed partial class TriggerItemInstanceParams : IItemInstanceParams {
  public uint TargetWorldObjectIndex { get; set; }
  public ushort Flags { get; set; }
  public byte Radius { get; set; }
  public byte SoundId { get; set; }
  public byte Id { get; set; }
  public byte NextId { get; set; }
  public ushort StartY { get; set; }
  public ushort EndY { get; set; }
}

[BinarySchema]
public sealed partial class UnimplementedItemInstanceParams : IItemInstanceParams {
  private readonly byte[] padding_ = new byte[12];
}