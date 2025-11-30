using System.Collections;

using CommunityToolkit.HighPerformance;

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

  IS_COMPRESSED = 1 << 13,
  FIRST_FRAME_ONLY = 1 << 14,
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
  public int FrameCount => this.Type.CheckFlag(SequenceType.FIRST_FRAME_ONLY)
      ? 1
      : this.Header.FrameCount;

  [Skip]
  public float?[] RotationXs { get; set; }

  [Skip]
  public float?[] RotationYs { get; set; }

  [Skip]
  public float?[] RotationZs { get; set; }

  [Skip]
  public float?[] PositionXs { get; set; }

  [Skip]
  public float?[] PositionYs { get; set; }

  [Skip]
  public float?[] PositionZs { get; set; }

  [Skip]
  public float?[] ScaleXs { get; set; }

  [Skip]
  public float?[] ScaleYs { get; set; }

  [Skip]
  public float?[] ScaleZs { get; set; }

  [ReadLogic]
  private void ReadTransforms_(IBinaryReader br) {
    var frameCount = this.FrameCount;

    this.RotationXs = new float?[frameCount];
    this.RotationYs = new float?[frameCount];
    this.RotationZs = new float?[frameCount];
    this.PositionXs = new float?[frameCount];
    this.PositionYs = new float?[frameCount];
    this.PositionZs = new float?[frameCount];
    this.ScaleXs = new float?[frameCount];
    this.ScaleYs = new float?[frameCount];
    this.ScaleZs = new float?[frameCount];

    if (frameCount == 0 || this.Size == 0) {
      return;
    }

    br.SubreadAt(
        this.Parent.BlockPointer + this.DataPointer,
        () => {
          var hasFrames = new BitArray(br.ReadUInt32s((frameCount + 0x1f) >> 5)
                                         .AsBytes()
                                         .ToArray());

          this.ReadUncompressedFrame_(br,
                                      out var rotationX0,
                                      out var rotationY0,
                                      out var rotationZ0,
                                      out var positionX0,
                                      out var positionY0,
                                      out var positionZ0,
                                      out var scaleX0,
                                      out var scaleY0,
                                      out var scaleZ0);

          this.RotationXs[0] = rotationX0;
          this.RotationYs[0] = rotationY0;
          this.RotationZs[0] = rotationZ0;

          this.PositionXs[0] = positionX0;
          this.PositionYs[0] = positionY0;
          this.PositionZs[0] = positionZ0;

          this.ScaleXs[0] = scaleX0;
          this.ScaleYs[0] = scaleY0;
          this.ScaleZs[0] = scaleZ0;

          var isCompressed = this.Type.CheckFlag(SequenceType.IS_COMPRESSED);

          var rotationX = rotationX0;
          var rotationY = rotationY0;
          var rotationZ = rotationZ0;
          var positionX = positionX0;
          var positionY = positionY0;
          var positionZ = positionZ0;
          var scaleX = scaleX0;
          var scaleY = scaleY0;
          var scaleZ = scaleZ0;

          for (var f = 1; f < frameCount; ++f) {
            if (!hasFrames[f]) {
              continue;
            }

            if (!isCompressed) {
              this.ReadUncompressedFrame_(br,
                                          out rotationX,
                                          out rotationY,
                                          out rotationZ,
                                          out positionX,
                                          out positionY,
                                          out positionZ,
                                          out scaleX,
                                          out scaleY,
                                          out scaleZ);
            } else {
              this.ReadCompressedFrame_(br,
                                        out var deltaRotationX,
                                        out var deltaRotationY,
                                        out var deltaRotationZ,
                                        out var deltaPositionX,
                                        out var deltaPositionY,
                                        out var deltaPositionZ,
                                        out var deltaScaleX,
                                        out var deltaScaleY,
                                        out var deltaScaleZ);
              rotationX = Add_(rotationX, deltaRotationX);
              rotationY = Add_(rotationY, deltaRotationY);
              rotationZ = Add_(rotationZ, deltaRotationZ);
              positionX = Add_(positionX, deltaPositionX);
              positionY = Add_(positionY, deltaPositionY);
              positionZ = Add_(positionZ, deltaPositionZ);
              scaleX = Add_(scaleX, deltaScaleX);
              scaleY = Add_(scaleY, deltaScaleY);
              scaleZ = Add_(scaleZ, deltaScaleZ);
            }

            this.RotationXs[f] = rotationX;
            this.RotationYs[f] = rotationY;
            this.RotationZs[f] = rotationZ;

            this.PositionXs[f] = positionX;
            this.PositionYs[f] = positionY;
            this.PositionZs[f] = positionZ;

            this.ScaleXs[f] = scaleX;
            this.ScaleYs[f] = scaleY;
            this.ScaleZs[f] = scaleZ;
          }
        });
  }

  private void ReadUncompressedFrame_(IBinaryReader br,
                                      out float? rotationX,
                                      out float? rotationY,
                                      out float? rotationZ,
                                      out float? positionX,
                                      out float? positionY,
                                      out float? positionZ,
                                      out float? scaleX,
                                      out float? scaleY,
                                      out float? scaleZ) {
    if (this.Type.CheckFlag(SequenceType.ROTATION_X)) {
      rotationX = br.ReadSingle();
    } else {
      rotationX = null;
    }

    if (this.Type.CheckFlag(SequenceType.ROTATION_Y)) {
      rotationY = br.ReadSingle();
    } else {
      rotationY = null;
    }

    if (this.Type.CheckFlag(SequenceType.ROTATION_Z)) {
      rotationZ = br.ReadSingle();
    } else {
      rotationZ = null;
    }


    if (this.Type.CheckFlag(SequenceType.POSITION_X)) {
      positionX = br.ReadSingle();
    } else {
      positionX = null;
    }

    if (this.Type.CheckFlag(SequenceType.POSITION_Y)) {
      positionY = br.ReadSingle();
    } else {
      positionY = null;
    }

    if (this.Type.CheckFlag(SequenceType.POSITION_Z)) {
      positionZ = br.ReadSingle();
    } else {
      positionZ = null;
    }


    if (this.Type.CheckFlag(SequenceType.SCALE_X)) {
      scaleX = br.ReadSingle();
    } else {
      scaleX = null;
    }

    if (this.Type.CheckFlag(SequenceType.SCALE_Y)) {
      scaleY = br.ReadSingle();
    } else {
      scaleY = null;
    }

    if (this.Type.CheckFlag(SequenceType.SCALE_Z)) {
      scaleZ = br.ReadSingle();
    } else {
      scaleZ = null;
    }
  }

  private void ReadCompressedFrame_(IBinaryReader br,
                                    out float? rotationX,
                                    out float? rotationY,
                                    out float? rotationZ,
                                    out float? positionX,
                                    out float? positionY,
                                    out float? positionZ,
                                    out float? scaleX,
                                    out float? scaleY,
                                    out float? scaleZ) {
    if (this.Type.CheckFlag(SequenceType.ROTATION_X)) {
      rotationX = this.Parent.CompressAng[br.ReadByte()];
    } else {
      rotationX = null;
    }

    if (this.Type.CheckFlag(SequenceType.ROTATION_Y)) {
      rotationY = this.Parent.CompressAng[br.ReadByte()];
    } else {
      rotationY = null;
    }

    if (this.Type.CheckFlag(SequenceType.ROTATION_Z)) {
      rotationZ = this.Parent.CompressAng[br.ReadByte()];
    } else {
      rotationZ = null;
    }


    if (this.Type.CheckFlag(SequenceType.POSITION_X)) {
      positionX = this.Parent.CompressPos[br.ReadByte()];
    } else {
      positionX = null;
    }

    if (this.Type.CheckFlag(SequenceType.POSITION_Y)) {
      positionY = this.Parent.CompressPos[br.ReadByte()];
    } else {
      positionY = null;
    }

    if (this.Type.CheckFlag(SequenceType.POSITION_Z)) {
      positionZ = this.Parent.CompressPos[br.ReadByte()];
    } else {
      positionZ = null;
    }


    if (this.Type.CheckFlag(SequenceType.SCALE_X)) {
      scaleX = this.Parent.CompressScale[br.ReadByte()];
    } else {
      scaleX = null;
    }

    if (this.Type.CheckFlag(SequenceType.SCALE_Y)) {
      scaleY = this.Parent.CompressScale[br.ReadByte()];
    } else {
      scaleY = null;
    }

    if (this.Type.CheckFlag(SequenceType.SCALE_Z)) {
      scaleZ = this.Parent.CompressScale[br.ReadByte()];
    } else {
      scaleZ = null;
    }
  }

  private static float? Add_(float? lhs, float? rhs) {
    if (lhs != null && rhs != null) {
      return lhs + rhs;
    }

    return null;
  }
}