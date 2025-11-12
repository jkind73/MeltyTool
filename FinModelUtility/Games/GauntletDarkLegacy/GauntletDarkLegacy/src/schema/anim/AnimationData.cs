using fin.data.dictionaries;

using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.anim;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/haekb/io_scene_gdl/blob/master/src/anim_model.py#L193
/// </summary>
[BinarySchema]
[LocalPositions]
public sealed partial class AnimationData
    : IBinaryDeserializable, IChildOf<SkeletonData> {
  public SkeletonData Parent { get; set; }

  public uint CompressAngPointer { get; set; }
  public uint CompressPosPointer { get; set; }
  public uint CompressUnitPointer { get; set; }

  public uint BlockPointer { get; set; }

  public uint SequencePointer { get; set; }
  private uint sequenceCount_;
  public uint ObjectCount { get; set; }

  [Skip]
  public ListDictionary<Bone, AnimationSequence> SequencesByBone { get; }
    = new();

  [ReadLogic]
  private void ReadSequences_(IBinaryReader br) {
    this.SequencesByBone.Clear();
    foreach (var bone in this.Parent.Bones) {
      br.SubreadAt(
          bone.SequencePointer,
          () => {
            for (var i = 0; i < this.sequenceCount_; ++i) {
              var animationSequence = new AnimationSequence { Parent = this };
              animationSequence.Read(br);
              this.SequencesByBone.Add(bone, animationSequence);
            }
          });
    }
  }
}