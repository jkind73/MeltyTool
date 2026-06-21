using schema.binary;
using schema.binary.attributes;

namespace bar.schema;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/8fdc6d5b60fd2365b5ff69d6a64a1602ba00badd/src/BeetleAdventureRacing/Scenes.ts#L337
/// </summary>
[BinarySchema]
public sealed partial class BarSceneUvmo : IBinaryDeserializable {
  [SequenceLengthSource(0x1840)]
  private byte[] unk_;

  [SequenceLengthSource(0x22)]
  public BarSceneEntry[] Entries { get; set; }
}

[BinarySchema]
public sealed partial class BarSceneEntry : IBinaryDeserializable {
  public short UvtrIndex { get; set; }
  public short UvenIndex { get; set; }

  [SequenceLengthSource(0x98)]
  private byte[] unk_;
}