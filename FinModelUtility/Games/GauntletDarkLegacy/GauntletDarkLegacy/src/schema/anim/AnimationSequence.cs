using System.Numerics;

using fin.util.enums;

using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.anim;

[Flags]
public enum SequenceType : ushort {
  ROTATION_X = 1 << 0,
  ROTATION_Y = 1 << 1,
  ROTATION_Z = 1 << 2,

  POSITION_X = 1 << 4,
  POSITION_Y = 1 << 5,
  POSITION_Z = 1 << 6,

  SCALE_X = 1 << 8,
  SCALE_Y = 1 << 9,
  SCALE_Z = 1 << 10,

  IS_COMPRESSED = 0x2000,
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/haekb/io_scene_gdl/blob/master/src/anim_model.py#L271
/// </summary>
[BinarySchema]
public sealed partial class AnimationSequence
    : IBinaryDeserializable, IChildOf<AnimationData> {
  public AnimationData Parent { get; set; }

  [Skip]
  public AnimationHeader Header { get; set; }

  public SequenceType Type { get; set; }
  public ushort Size { get; set; }
  public uint DataPointer { get; set; }

  [Skip]
  public Vector3[] Rotations { get; set; }

  [Skip]
  public Vector3[] Positions { get; set; }

  [Skip]
  public Vector3[] Scales { get; set; }

  [ReadLogic]
  private void ReadTransforms_(IBinaryReader br) {
    var frameCount = this.Header.FrameCount;

    this.Rotations = new Vector3[frameCount];
    this.Positions = new Vector3[frameCount];
    this.Scales = new Vector3[frameCount];

    br.SubreadAt(
        this.Parent.BlockPointer + this.DataPointer,
        () => {
          // Decompiled code, this will skip past the useless header in the block data
          br.Position += ((frameCount + 0x1f) >> 5) * 4;

          var isCompressed = this.Type.CheckFlag(SequenceType.IS_COMPRESSED);
          var flip = (this.Header.Flags & 1) != 0;

          for (var f = 0; f < frameCount; ++f) {
            var rotation = Vector3.Zero;
            var position = Vector3.Zero;
            var scale = Vector3.One;

            if (isCompressed) {
              if (this.Type.CheckFlag(SequenceType.ROTATION_X)) {
                rotation.X = this.Parent.CompressAng[ReadWeirdByte_(br)];
              }

              if (this.Type.CheckFlag(SequenceType.ROTATION_Y)) {
                rotation.Y = this.Parent.CompressAng[ReadWeirdByte_(br)];
                if (flip) {
                  rotation.Y *= -1;
                }
              }

              if (this.Type.CheckFlag(SequenceType.ROTATION_Z)) {
                rotation.Z = this.Parent.CompressAng[ReadWeirdByte_(br)];
                if (flip) {
                  rotation.Z *= -1;
                }
              }

              if (this.Type.CheckFlag(SequenceType.POSITION_X)) {
                position.X = this.Parent.CompressPos[ReadWeirdByte_(br)];
              }

              if (this.Type.CheckFlag(SequenceType.POSITION_Y)) {
                position.Y = this.Parent.CompressPos[ReadWeirdByte_(br)];
              }

              if (this.Type.CheckFlag(SequenceType.POSITION_Z)) {
                position.Z = this.Parent.CompressPos[ReadWeirdByte_(br)];
              }

              if (this.Type.CheckFlag(SequenceType.SCALE_X)) {
                scale.X = ReadWeirdByte_(br) / 256f;
              }

              if (this.Type.CheckFlag(SequenceType.SCALE_Y)) {
                scale.Y = ReadWeirdByte_(br) / 256f;
              }

              if (this.Type.CheckFlag(SequenceType.SCALE_Z)) {
                scale.Z = ReadWeirdByte_(br) / 256f;
              }
            }

            this.Rotations[f] = rotation;
            this.Positions[f] = position;
            this.Scales[f] = scale;
          }
        });
  }

  private static byte ReadWeirdByte_(IBinaryReader br) {
    var b = br.ReadByte();
    return (byte) (b > 127 ? b - 256 : b);
  }
}