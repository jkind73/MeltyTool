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
public sealed partial class AnimHeader
    : IBinaryDeserializable, IChildOf<ATreeHeader> {
  public ATreeHeader Parent { get; set; }

  public uint CompressAngPointer { get; set; }
  public uint CompressPosPointer { get; set; }
  public uint CompressScalePointer { get; set; }

  public uint BlockPointer { get; set; }

  public uint SequencePointer { get; set; }
  private uint sequenceCount_;
  public uint ObjectCount { get; set; }

  [RAtPositionOrNull(nameof(CompressAngPointer))]
  [SequenceLengthSource(256)]
  public float[] CompressedAngles { get; set; }

  [RAtPositionOrNull(nameof(CompressPosPointer))]
  [SequenceLengthSource(256)]
  public float[] CompressedPositions { get; set; }

  [RAtPositionOrNull(nameof(CompressScalePointer))]
  [SequenceLengthSource(256)]
  public float[] CompressedScales { get; set; }

  [Skip]
  public ListDictionary<ANodeInfo, AnimationSequence> SequencesByBone { get; }
    = new();

  [ReadLogic]
  private void ReadSequences_(IBinaryReader br) {
    this.SequencesByBone.Clear();
    foreach (var bone in this.Parent.ANodeInfos) {
      if (bone.Type is AnimType.SKEL_ANIM) {
        br.SubreadAt(
            bone.AnimSeqInfoOffset,
            () => {
              for (var i = 0; i < this.sequenceCount_; ++i) {
                var animationSequence = new AnimationSequence {
                    Parent = this,
                    Header = this.Parent.ATreeSequences[i],
                };
                animationSequence.Read(br);
                this.SequencesByBone.Add(bone, animationSequence);
              }
            });
      }
    }
  }
}