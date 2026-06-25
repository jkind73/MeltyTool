using System.Numerics;

using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace tlpe.scb;

[BinarySchema]
[Endianness(Endianness.LittleEndian)]
public sealed partial class Scb : IBinaryConvertible {
  [Unknown]
  private uint maybeSize_;

  [Unknown]
  private uint unkCount0_;

  [Unknown]
  private uint unkCount1_;

  [Unknown]
  private uint unkCount2_;

  [Unknown]
  private Vector3 unkVec3_;

  [Skip]
  public ISection[] Sections { get; set; }

  [ReadLogic]
  private void ReadSections_(IBinaryReader br) {
    var sections = new List<ISection>();

    while (!br.Eof) {
      var sectionType = (SectionType) br.ReadUInt32();
      switch (sectionType) {
        case 0: {
          break;
        }
        case SectionType.SECTION_1: {
          sections.Add(br.ReadNew<JointSection>());
          break;
        }
        case SectionType.ANIMATION: {
          sections.Add(br.ReadNew<AnimationSection>());
          break;
        }
        case SectionType.MESH: {
          sections.Add(br.ReadNew<MeshSection>());
          break;
        }
        case SectionType.SECTION_6: {
          sections.Add(br.ReadNew<MaterialSection>());
          break;
        }
        default: throw new ArgumentOutOfRangeException();
      }
    }

    this.Sections = sections.ToArray();
  }

  public void Write(IBinaryWriter bw) {
    bw.WriteUInt32(this.maybeSize_);
    bw.WriteUInt32(this.unkCount0_);
    bw.WriteUInt32(this.unkCount1_);
    bw.WriteUInt32(this.unkCount2_);
    bw.WriteVector3(this.unkVec3_);

    foreach (var section in this.Sections) {
      section.Write(bw);
    }
  }
}