// Decompiled with JetBrains decompiler
// Type: MKDS_Course_Modifier.GCN.BCA
// Assembly: MKDS Course Modifier, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DAEF8B62-698B-42D0-BEDD-3770EB8C9FE8
// Assembly location: R:\Documents\CSharpWorkspace\Pikmin2Utility\MKDS Course Modifier\MKDS Course Modifier.exe

using System;
using System.IO;
using System.Linq;

using jsystem.G3D_Binary_File_Format;

using schema.binary;
using schema.binary.attributes;


namespace jsystem.schema.j3dgraph.bcx;

/// <summary>
///   BCA files define joint animations where each frame is defined.
/// </summary>
[Endianness(Endianness.BigEndian)]
public partial class Bca : IBcx {
  public BcaHeader header;
  public Anf1Section anf1;

  public Bca(byte[] file) {
    using var br = new SchemaBinaryReader((Stream) new MemoryStream(file),
                                          Endianness.BigEndian);
    this.header = br.ReadNew<BcaHeader>();
    this.anf1 = new Anf1Section(br, out _);
  }

  public IAnx1 Anx1 => this.anf1;

  [BinarySchema]
  public sealed partial class BcaHeader : IBinaryConvertible {
    private readonly string magic_ = "J3D1bca1";

    [WSizeOfStreamInBytes]
    private uint fileSize_;

    private readonly uint sectionCount_ = 1;

    [SequenceLengthSource(16)]
    private byte[] padding_;
  }

  public partial class Anf1Section : IAnx1 {
    public const string SIGNATURE = "ANF1";
    public DataBlockHeader header;
    public byte loopFlags;
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

    public Anf1Section(IBinaryReader br, out bool ok) {
      bool ok1;

      this.header = new DataBlockHeader(br, "ANF1", out ok1);
      if (!ok1) {
        ok = false;
      } else {
        this.loopFlags = br.ReadByte();
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
        float rotScale = (float) (1 * Math.PI / 32768f);
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
          float totScale) {
        this.Values =
            new JointAnim(
                this,
                scales,
                rotations,
                translations,
                totScale);
      }

      public float GetAnimValue(IJointAnimKey[] keys, float t) {
        if (keys.Length == 0)
          return 0.0f;
        return keys.Length == 1 ? keys[0].Value : keys[(int) t].Value;
      }

      [BinarySchema]
      public sealed partial class AnimComponent : IBinaryConvertible {
        public AnimIndex S { get; } = new();
        public AnimIndex R { get; } = new();
        public AnimIndex T { get; } = new();

        [BinarySchema]
        public sealed partial class AnimIndex : IBinaryConvertible {
          public ushort count;
          public ushort index;
        }
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
            AnimComponent.AnimIndex component) {
          dst = new IJointAnimKey[component.count];
          if (component.count <= 0)
            throw new Exception("Count <= 0");
          if (component.count == 1) {
            dst[0] = new Key(0, src[component.index]);
          } else {
            for (var index = 0; index < component.count; ++index)
              dst[index] =
                  new Key(index, src[component.index + index]);
          }
        }

        private void SetKeysR_(
            out IJointAnimKey[] dst,
            short[] src,
            float rotScale,
            AnimComponent.AnimIndex component) {
          dst = new IJointAnimKey[component.count];
          if (component.count <= 0)
            throw new Exception("Count <= 0");
          if (component.count == 1) {
            dst[0] =
                new Key(0, src[component.index] * rotScale);
          } else {
            for (var index = 0; index < component.count; ++index)
              dst[index] =
                  new Key(index,
                          src[component.index + index] *
                          rotScale);
          }
        }

        private sealed class Key(int frame, float value) : IJointAnimKey {
          public int Frame { get; } = frame;
          public float Value { get; } = value;
        }
      }
    }
  }
}