using System.Numerics;

using fin.math;
using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb.skl;

[BinarySchema]
public sealed partial class Bone : IBinaryConvertible {
  // Because only 12 bits are used, 4095 is the max bone count. (In
  // versions > OoT3D anyway)
  private ushort flags_;

  [Skip]
  public ushort Id => (ushort) (this.flags_ & 0xFFF); // Get boneID

  // M-1:
  // Other 4 bits are probably more flags, but they're not used in any of
  // the three games
  // Though I probably missed a few compressed files. It's most likely
  // these flags below:
  // IsSegmentScaleCompensate, IsCompressible, IsNeededRendering, HasSkinningMatrix
  [Skip]
  public bool HasSkinningMatrix => this.flags_.GetBit(4);

  public short parentId;

  public Vector3 Scale { get; set; }
  public Vector3 Rotation { get; set; }
  public Vector3 Translation { get; set; }

  [Skip]
  private bool HasUnk => CmbHeader.FileVersion > Version.OCARINA_OF_TIME_3D;

  [Unknown]
  [RIfBoolean(nameof(HasUnk))]
  public uint? unk0;
}