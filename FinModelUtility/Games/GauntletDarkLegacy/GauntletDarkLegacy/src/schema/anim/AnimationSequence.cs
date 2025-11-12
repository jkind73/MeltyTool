using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.anim;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/haekb/io_scene_gdl/blob/master/src/anim_model.py#L271
/// </summary>
[BinarySchema]
public sealed partial class AnimationSequence
    : IBinaryDeserializable, IChildOf<AnimationData> {
  public AnimationData Parent { get; set; }

  public ushort Type { get; set; }
  public ushort Size { get; set; }
  public uint DataPointer { get; set; }

  [ReadLogic]
  private void ReadTransforms_(IBinaryReader br) {
    br.SubreadAt(this.Parent.BlockPointer + this.DataPointer,
                 () => {
                   // TODO: Implement
                 });
  }
}