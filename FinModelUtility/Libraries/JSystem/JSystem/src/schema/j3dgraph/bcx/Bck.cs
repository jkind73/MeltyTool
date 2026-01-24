// Decompiled with JetBrains decompiler
// Type: MKDS_Course_Modifier.GCN.BCK
// Assembly: MKDS Course Modifier, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DAEF8B62-698B-42D0-BEDD-3770EB8C9FE8
// Assembly location: R:\Documents\CSharpWorkspace\Pikmin2Utility\MKDS Course Modifier\MKDS Course Modifier.exe

using System;
using System.IO;
using System.Linq;

using fin.util.asserts;

using jsystem.G3D_Binary_File_Format;

using schema.binary;
using schema.binary.attributes;


namespace jsystem.schema.j3dgraph.bcx;

/// <summary>
///   BCK files define joint animations with sparse keyframes.
///
///   https://wiki.cloudmodding.com/tww/BCK
/// </summary>
[Endianness(Endianness.BigEndian)]
public partial class Bck : IBcx {
  public BckHeader header;
  public Ank1Section ank1;

  public Bck(byte[] file) {
    using var br =
        new SchemaBinaryReader((Stream) new MemoryStream(file),
                               Endianness.BigEndian);
    this.header = br.ReadNew<BckHeader>();
    this.ank1 = new Ank1Section(br, out _);
  }

  public IAnx1 Anx1 => this.ank1;

  [BinarySchema]
  public sealed partial class BckHeader : IBinaryConvertible {
    private readonly string magic_ = "J3D1bck1";

    [WSizeOfStreamInBytes]
    private uint fileSize_;

    private readonly uint sectionCount_ = 1;

    [SequenceLengthSource(16)]
    private byte[] padding_;
  }

  public enum LoopMode : byte {
    ONCE = 0,
    ONCE_AND_RESET = 1,
    LOOP = 2,
    MIRRORED_ONCE = 3,
    MIRRORED_LOOP = 4,
  }

  public partial class Ank1Section : IAnx1 {
    public const string SIGNATURE = "ANK1";
    public DataBlockHeader header;
    public LoopMode loopMode;
    public byte angleMultiplier;
    public ushort animLength;
    public ushort nrJoints;
    public ushort nrScale;
    public ushort nrRot;
    public ushort nrTrans;
    public uint jointOffset;
    public uint scaleOffset;
    public uint rotOffset;
    public uint transOffset;
    public float[] scale;
    public short[] rotation;
    public float[] translation;

    public Ank1Section(IBinaryReader br, out bool ok) {
      bool ok1;
      this.header = new DataBlockHeader(br, "ANK1", out ok1);
      if (!ok1) {
        ok = false;
      } else {
        this.loopMode = (LoopMode) br.ReadByte();
        this.angleMultiplier = br.ReadByte();
        this.animLength = br.ReadUInt16();
        this.nrJoints = br.ReadUInt16();
        this.nrScale = br.ReadUInt16();
        this.nrRot = br.ReadUInt16();
        this.nrTrans = br.ReadUInt16();
        this.jointOffset = br.ReadUInt32();
        this.scaleOffset = br.ReadUInt32();
        this.rotOffset = br.ReadUInt32();
        this.transOffset = br.ReadUInt32();
        br.Position = (long) (32U + this.scaleOffset);
        this.scale = br.ReadSingles((int) this.nrScale);
        br.Position = (long) (32U + this.rotOffset);
        this.rotation = br.ReadInt16s((int) this.nrRot);
        br.Position = (long) (32U + this.transOffset);
        this.translation = br.ReadSingles((int) this.nrTrans);
        var rotScale = MathF.Pow(2, this.angleMultiplier - 15) * MathF.PI;
        br.Position = (long) (32U + this.jointOffset);
        this.Joints = new AnimatedJoint[(int) this.nrJoints];
        for (int index = 0; index < (int) this.nrJoints; ++index) {
          var animatedJoint = new AnimatedJoint(br);
          animatedJoint.SetValues(this.scale,
                                  this.rotation,
                                  this.translation,
                                  rotScale);
          this.Joints[index] = animatedJoint;
        }
        ok = true;
      }
    }

    public int FrameCount => this.animLength;
    public IAnimatedJoint[] Joints { get; }


    public partial class AnimatedJoint : IAnimatedJoint {
      public AnimComponent[] axes;

      public AnimatedJoint(IBinaryReader br) {
        this.axes = new AnimComponent[3];
        for (var i = 0; i < this.axes.Length; ++i) {
          this.axes[i] = br.ReadNew<AnimComponent>();
        }
      }

      public IJointAnim Values { get; private set; }

      public void SetValues(
          float[] scales,
          short[] rotations,
          float[] translations,
          float rotScale) {
        this.Values =
            new JointAnim(
                this,
                scales,
                rotations,
                translations,
                rotScale);
      }

      [BinarySchema]
      public sealed partial class AnimComponent : IBinaryConvertible {
        public AnimIndex S { get; } = new();
        public AnimIndex R { get; } = new();
        public AnimIndex T { get; } = new();
      }

      [BinarySchema]
      public sealed partial class AnimIndex : IBinaryConvertible {
        public ushort count;
        public ushort index;
        public ushort tangentMode;
      }

      public sealed class JointAnim : IJointAnim {
        public JointAnim(
            AnimatedJoint joint,
            float[] scales,
            short[] rotations,
            float[] translations,
            float rotScale) {
          this.Scales = joint.axes.Select(
                                 axisSrc => {
                                   this.SetKeysSt_(
                                       out var axis,
                                       scales,
                                       axisSrc.S);
                                   return axis;
                                 })
                             .ToArray();
          this.Rotations = joint.axes.Select(
                                    axisSrc => {
                                      this.SetKeysR_(out var axis,
                                        rotations,
                                        rotScale,
                                        axisSrc.R);
                                      return axis;
                                    })
                                .ToArray();
          this.Translations = joint.axes.Select(
                                       axisSrc => {
                                         this.SetKeysSt_(out var axis,
                                           translations,
                                           axisSrc.T);
                                         return axis;
                                       })
                                   .ToArray();
        }

        public IJointAnimKey[][] Scales { get; }
        public IJointAnimKey[][] Rotations { get; }
        public IJointAnimKey[][] Translations { get; }

        private void SetKeysSt_(
            out IJointAnimKey[] dst,
            float[] src,
            AnimIndex component) {
          dst = new IJointAnimKey[component.count];
          if (component.count <= 0)
            throw new Exception("Count <= 0");
          if (component.count == 1) {
            dst[0] =
                new Key(
                    0,
                    src[component.index],
                    0,
                    0);
          } else {
            var tangentMode = component.tangentMode;
            var hasTwoTangents = tangentMode == 1;
            Asserts.True(tangentMode == 0 || tangentMode == 1);

            var stride = hasTwoTangents ? 4 : 3;
            for (var index = 0; index < component.count; ++index) {
              var i = component.index + stride * index;

              var time = (int) src[i + 0];
              var value = src[i + 1];

              float incomingTangent, outgoingTangent;
              if (hasTwoTangents) {
                incomingTangent = src[i + 2];
                outgoingTangent = src[i + 3];
              } else {
                incomingTangent = outgoingTangent = src[i + 2];
              }

              dst[index] =
                  new Key(
                      time,
                      value,
                      incomingTangent,
                      outgoingTangent);
            }
          }
        }

        private void SetKeysR_(
            out IJointAnimKey[] dst,
            short[] src,
            float rotScale,
            AnimIndex component) {
          dst =
              new IJointAnimKey[component.count];
          if (component.count <= 0)
            throw new Exception("Count <= 0");
          if (component.count == 1) {
            dst[0] = new Key(
                0,
                src[component.index] * rotScale,
                0,
                0);
          } else {
            var tangentMode = component.tangentMode;
            var hasTwoTangents = tangentMode == 1;
            Asserts.True(tangentMode == 0 || tangentMode == 1);

            var stride = hasTwoTangents ? 4 : 3;
            for (var index = 0; index < component.count; ++index) {
              var i = component.index + stride * index;

              var time = src[i + 0];
              var value = src[i + 1] * rotScale;

              float incomingTangent, outgoingTangent;
              if (hasTwoTangents) {
                incomingTangent = src[i + 2] * rotScale;
                outgoingTangent = src[i + 3] * rotScale;
              } else {
                incomingTangent = outgoingTangent = src[i + 2] * rotScale;
              }

              dst[index] =
                  new Key(
                      time,
                      value,
                      incomingTangent,
                      outgoingTangent);
            }
          }
        }

        public sealed class Key(
            int frame,
            float value,
            float incomingTangent,
            float outgoingTangent)
            : IJointAnimKey {
          public int Frame { get; } = frame;
          public float Value { get; } = value;
          public float IncomingTangent { get; } = incomingTangent;
          public float OutgoingTangent { get; } = outgoingTangent;
        }
      }
    }
  }
}