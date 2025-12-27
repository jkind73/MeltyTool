using fin.data.dictionaries;

using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.anim;

[BinarySchema]
[LocalPositions]
public sealed partial class ObjectAnimationHeader
    : IBinaryDeserializable, IChildOf<ATreeHeader> {
  public ATreeHeader Parent { get; set; }

  private uint objAnimPointer_;
  private uint objAnimCount_;

  [Skip]
  public ObjectAnimation[] ObjectAnimations { get; private set; }

  [Skip]
  public ListDictionary<ANodeInfo, ObjectAnimation>
      ObjectAnimationSequencesByBone { get; } = new();

  [ReadLogic]
  private void ReadSequences_(IBinaryReader br) {
    this.ObjectAnimationSequencesByBone.Clear();
    this.ObjectAnimations = new ObjectAnimation[this.objAnimCount_];

    var boneByStartOffset = new Dictionary<long, ANodeInfo>();
    foreach (var bone in this.Parent.ANodeInfos) {
      if (bone.Type is AnimType.OBJ_ANIM) {
        boneByStartOffset[bone.SequenceOffset] = bone;
      }
    }

    br.SubreadAt(
        this.objAnimPointer_,
        () => {
          ANodeInfo? currentBone = null;
          for (var i = 0; i < this.objAnimCount_; ++i) {
            if (boneByStartOffset.TryGetValue(br.Position, out var newBone)) {
              currentBone = newBone;
            }

            var animationSequence = new ObjectAnimation();
            animationSequence.Read(br);
            this.ObjectAnimations[i] = animationSequence; 

            if (currentBone != null) {
              this.ObjectAnimationSequencesByBone.Add(
                  currentBone,
                  animationSequence);
            }
          }
        });
  }
}