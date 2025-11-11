using schema.binary;
using schema.binary.attributes;

namespace gdl.schema;

/// <summary>
///   An animated model.
///
///   Shamelessly stolen from:
///   https://github.com/haekb/io_scene_gdl/blob/master/src/anim_model.py#L5
/// </summary>
[BinarySchema]
[Endianness(Endianness.LittleEndian)]
public sealed partial class Anim : IBinaryDeserializable {
  public ushort SkeletonCount { get; set; }
  public ushort Version { get; set; }
  public uint SkeletonDataPointer { get; set; }
  public uint EffectCount { get; set; }
  public uint EffectPointer { get; set; }

  [RAtPosition(nameof(SkeletonDataPointer))]
  [RSequenceLengthSource(nameof(SkeletonCount))]
  public Skeleton[] Skeletons { get; set; }
}
