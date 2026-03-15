using System.Numerics;

using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace tlpe.scb;

[BinarySchema]
public sealed partial class JointSection : ISection {
  [Unknown]
  private uint maybeSize_;

  public Vector3 Translation { get; set; }
  public Vector3 Rotation { get; set; }
  public Vector3 Scale { get; set; }
  
  [Unknown]
  private uint unk6_;

  [WLengthOfString(nameof(ParentName))]
  private uint length1_;

  [WLengthOfString(nameof(Name))]
  private uint length0_;

  [RStringLengthSource(nameof(length0_))]
  public string Name { get; set; }

  [RStringLengthSource(nameof(length1_))]
  public string ParentName { get; set; }
}