using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb.luts;

[BinarySchema]
public sealed partial class LutSet : IBinaryConvertible {
  public ushort bitFlags; //Not sure

  [WLengthOfSequence(nameof(keyframes))]
  private ushort keyframeCount_;

  public short start;
  public short end;

  [RSequenceLengthSource(nameof(keyframeCount_))]
  public LutKeyframe[] keyframes;

  [Unknown]
  public float unk1;

  [Unknown]
  public float unk2;
}